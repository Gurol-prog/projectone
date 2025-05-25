using Microsoft.Extensions.Options;
using MongoDB.Driver;
using projectone.Config;
using projectone.Models;

namespace projectone.Services
{
    public class ContentCommentsService
    {
        private readonly IMongoCollection<ContentComments> _contentCommentsCollection;

        public ContentCommentsService(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _contentCommentsCollection = database.GetCollection<ContentComments>("ContentComments");
        }

        // Tüm aktif yorumları getir
        public async Task<List<ContentComments>> GetAllAsync() =>
            await _contentCommentsCollection.Find(k => k.DeleteTime == null).ToListAsync();

        // ID ile yorum getir
        public async Task<ContentComments?> GetByIdAsync(string id) =>
            await _contentCommentsCollection.Find(x => x.Id == id && x.DeleteTime == null).FirstOrDefaultAsync();

        // Belirli bir içeriğin yorumlarını getir (sadece ana yorumlar - reply'ler değil)
        public async Task<List<ContentComments>> GetCommentsByContentIdAsync(string contentId) =>
            await _contentCommentsCollection
                .Find(x => x.ContentId == contentId && x.DeleteTime == null && x.ParentCommentId == null)
                .SortByDescending(x => x.InsertTime)
                .ToListAsync();

        // Belirli bir yorumun cevaplarını getir
        public async Task<List<ContentComments>> GetRepliesByParentIdAsync(string parentCommentId) =>
            await _contentCommentsCollection
                .Find(x => x.ParentCommentId == parentCommentId && x.DeleteTime == null)
                .SortBy(x => x.InsertTime)
                .ToListAsync();

        // Bir içeriğin tüm yorumlarını hiyerarşik olarak getir
        public async Task<List<ContentComments>> GetCommentsWithRepliesAsync(string contentId)
        {
            // Ana yorumları getir
            var mainComments = await GetCommentsByContentIdAsync(contentId);
            
            // Her ana yorum için reply'leri getir
            foreach (var comment in mainComments)
            {
                comment.Replies = await GetRepliesByParentIdAsync(comment.Id);
            }

            return mainComments;
        }

        // Kullanıcının yorumlarını getir
        public async Task<List<ContentComments>> GetCommentsByUserIdAsync(string userId) =>
            await _contentCommentsCollection
                .Find(x => x.UserId == userId && x.DeleteTime == null)
                .SortByDescending(x => x.InsertTime)
                .ToListAsync();

        // Yorum oluştur
        public async Task<ContentComments> CreateCommentAsync(ContentComments comment)
        {
            await _contentCommentsCollection.InsertOneAsync(comment);
            return comment;
        }

        // Yorum güncelle
        public async Task<bool> UpdateCommentAsync(string id, string newComment)
        {
            var update = Builders<ContentComments>.Update
                .Set(x => x.Comment, newComment)
                .Set(x => x.UpdateTime, DateTime.UtcNow);

            var result = await _contentCommentsCollection.UpdateOneAsync(
                x => x.Id == id && x.DeleteTime == null, 
                update);

            return result.ModifiedCount > 0;
        }

        // Yorum silme (soft delete)
        public async Task<bool> SoftDeleteCommentAsync(string id)
        {
            var update = Builders<ContentComments>.Update.Set(x => x.DeleteTime, DateTime.UtcNow);
            var result = await _contentCommentsCollection.UpdateOneAsync(x => x.Id == id, update);

            // Ana yorum siliniyorsa, tüm reply'leri de sil
            if (result.ModifiedCount > 0)
            {
                await _contentCommentsCollection.UpdateManyAsync(
                    x => x.ParentCommentId == id,
                    update);
            }

            return result.ModifiedCount > 0;
        }

        // Hard delete
        public async Task<bool> DeleteCommentAsync(string id)
        {
            var result = await _contentCommentsCollection.DeleteOneAsync(x => x.Id == id);
            
            // Reply'leri de sil
            if (result.DeletedCount > 0)
            {
                await _contentCommentsCollection.DeleteManyAsync(x => x.ParentCommentId == id);
            }

            return result.DeletedCount > 0;
        }

        // Bir içeriğin tüm yorumlarını sil (içerik silindiğinde)
        public async Task<bool> DeleteAllCommentsByContentIdAsync(string contentId)
        {
            var result = await _contentCommentsCollection.DeleteManyAsync(x => x.ContentId == contentId);
            return result.DeletedCount > 0;
        }

        // Yorum sayısını getir
        public async Task<long> GetCommentCountByContentIdAsync(string contentId) =>
            await _contentCommentsCollection.CountDocumentsAsync(
                x => x.ContentId == contentId && x.DeleteTime == null);

        // Yorum beğeni toggle sistemi (UserLikeCommentService ile birlikte çalışır)
        public async Task<(bool success, string message, int likeCount, int dislikeCount)> ToggleCommentLikeAsync(
            string userId, string commentId, string likeType, UserLikeCommentService userLikeCommentService)
        {
            // Yorumun var olup olmadığını kontrol et
            var comment = await GetByIdAsync(commentId);
            if (comment == null)
                return (false, "Yorum bulunamadı", 0, 0);

            // Kullanıcının mevcut beğenisini kontrol et
            var existingLike = await userLikeCommentService.GetUserCommentLikeAsync(userId, commentId);

            if (existingLike == null)
            {
                // İlk kez beğeniyor
                var success = await userLikeCommentService.AddCommentLikeAsync(userId, commentId, likeType);
                if (success)
                {
                    await UpdateCommentLikeCountsAsync(commentId, userLikeCommentService);
                    var stats = await userLikeCommentService.GetCommentLikeStatsAsync(commentId);
                    return (true, $"Yorum {(likeType == "like" ? "beğenildi" : "beğenilmedi")}", stats.likeCount, stats.dislikeCount);
                }
                return (false, "Beğeni eklenemedi", 0, 0);
            }
            else if (existingLike.LikeType == likeType)
            {
                // Aynı aksiyonu tekrar yapıyor - beğeniyi geri al
                await userLikeCommentService.RemoveCommentLikeAsync(userId, commentId);
                await UpdateCommentLikeCountsAsync(commentId, userLikeCommentService);
                var stats = await userLikeCommentService.GetCommentLikeStatsAsync(commentId);
                return (true, $"{(likeType == "like" ? "Beğeni" : "Beğenmeme")} geri alındı", stats.likeCount, stats.dislikeCount);
            }
            else
            {
                // Farklı aksiyon yapıyor (like -> dislike veya dislike -> like)
                await userLikeCommentService.UpdateCommentLikeAsync(userId, commentId, likeType);
                await UpdateCommentLikeCountsAsync(commentId, userLikeCommentService);
                var stats = await userLikeCommentService.GetCommentLikeStatsAsync(commentId);
                return (true, $"Yorum {(likeType == "like" ? "beğenildi" : "beğenilmedi")}", stats.likeCount, stats.dislikeCount);
            }
        }

        // Yorumun like/dislike sayılarını güncelle
        private async Task UpdateCommentLikeCountsAsync(string commentId, UserLikeCommentService userLikeCommentService)
        {
            var stats = await userLikeCommentService.GetCommentLikeStatsAsync(commentId);
            
            var update = Builders<ContentComments>.Update
                .Set(x => x.LikeCount, stats.likeCount)
                .Set(x => x.DislikeCount, stats.dislikeCount)
                .Set(x => x.UpdateTime, DateTime.UtcNow);

            await _contentCommentsCollection.UpdateOneAsync(x => x.Id == commentId, update);
        }

        // Kullanıcının bir yorumu beğenip beğenmediğini kontrol et
        public async Task<string?> GetUserCommentLikeStatusAsync(string userId, string commentId, UserLikeCommentService userLikeCommentService)
        {
            var userLike = await userLikeCommentService.GetUserCommentLikeAsync(userId, commentId);
            return userLike?.LikeType;
        }

        // En çok beğenilen yorumları getir
        public async Task<List<ContentComments>> GetTopCommentsAsync(string contentId, int limit = 5)
        {
            return await _contentCommentsCollection
                .Find(x => x.ContentId == contentId && x.DeleteTime == null && x.ParentCommentId == null)
                .SortByDescending(x => x.LikeCount - x.DislikeCount)
                .Limit(limit)
                .ToListAsync();
        }

        // Son yorumları getir (sayfalama ile)
        public async Task<List<ContentComments>> GetRecentCommentsAsync(string contentId, int page = 1, int pageSize = 10)
        {
            var skip = (page - 1) * pageSize;
            return await _contentCommentsCollection
                .Find(x => x.ContentId == contentId && x.DeleteTime == null && x.ParentCommentId == null)
                .SortByDescending(x => x.InsertTime)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }
    }
}