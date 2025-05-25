using Microsoft.Extensions.Options;
using MongoDB.Driver;
using projectone.Config;
using projectone.Models;

namespace projectone.Services
{
    public class UserLikeCommentService
    {
        private readonly IMongoCollection<UserLikeComment> _userLikeCommentCollection;

        public UserLikeCommentService(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _userLikeCommentCollection = database.GetCollection<UserLikeComment>("UserLikeComments");
            
            // Unique index oluştur - bir kullanıcı bir yorumu sadece bir kez beğenebilir
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexKeys = Builders<UserLikeComment>.IndexKeys
                .Ascending(x => x.UserId)
                .Ascending(x => x.CommentId);
            _userLikeCommentCollection.Indexes.CreateOne(new CreateIndexModel<UserLikeComment>(indexKeys, indexOptions));
        }

        // Kullanıcının bu yorumu beğenip beğenmediğini kontrol et
        public async Task<UserLikeComment?> GetUserCommentLikeAsync(string userId, string commentId)
        {
            return await _userLikeCommentCollection
                .Find(x => x.UserId == userId && x.CommentId == commentId)
                .FirstOrDefaultAsync();
        }

        // Beğeni ekle
        public async Task<bool> AddCommentLikeAsync(string userId, string commentId, string likeType)
        {
            try
            {
                var userLikeComment = new UserLikeComment
                {
                    UserId = userId,
                    CommentId = commentId,
                    LikeType = likeType,
                    CreatedAt = DateTime.UtcNow
                };

                await _userLikeCommentCollection.InsertOneAsync(userLikeComment);
                return true;
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                // Kullanıcı zaten bu yorumu beğenmiş
                return false;
            }
        }

        // Beğeniyi güncelle (like -> dislike veya dislike -> like)
        public async Task<bool> UpdateCommentLikeAsync(string userId, string commentId, string newLikeType)
        {
            var update = Builders<UserLikeComment>.Update.Set(x => x.LikeType, newLikeType);
            var result = await _userLikeCommentCollection.UpdateOneAsync(
                x => x.UserId == userId && x.CommentId == commentId,
                update);

            return result.ModifiedCount > 0;
        }

        // Beğeniyi sil
        public async Task<bool> RemoveCommentLikeAsync(string userId, string commentId)
        {
            var result = await _userLikeCommentCollection.DeleteOneAsync(
                x => x.UserId == userId && x.CommentId == commentId);

            return result.DeletedCount > 0;
        }

        // Yorumun tüm beğenilerini sil (yorum silindiğinde)
        public async Task<bool> RemoveAllLikesForCommentAsync(string commentId)
        {
            var result = await _userLikeCommentCollection.DeleteManyAsync(x => x.CommentId == commentId);
            return result.DeletedCount > 0;
        }

        // Yorumun beğeni istatistiklerini getir
        public async Task<(int likeCount, int dislikeCount)> GetCommentLikeStatsAsync(string commentId)
        {
            var likes = await _userLikeCommentCollection
                .CountDocumentsAsync(x => x.CommentId == commentId && x.LikeType == "like");
            
            var dislikes = await _userLikeCommentCollection
                .CountDocumentsAsync(x => x.CommentId == commentId && x.LikeType == "dislike");

            return ((int)likes, (int)dislikes);
        }

        // Kullanıcının bir içeriğin tüm yorumlarındaki beğeni durumlarını getir (performans için)
        public async Task<Dictionary<string, string>> GetUserLikeStatusForCommentsAsync(string userId, List<string> commentIds)
        {
            var userLikes = await _userLikeCommentCollection
                .Find(x => x.UserId == userId && commentIds.Contains(x.CommentId))
                .ToListAsync();

            return userLikes.ToDictionary(x => x.CommentId, x => x.LikeType);
        }

        // Kullanıcının beğendiği yorumları getir
        public async Task<List<string>> GetLikedCommentsByUserAsync(string userId, string likeType = "like")
        {
            var likedComments = await _userLikeCommentCollection
                .Find(x => x.UserId == userId && x.LikeType == likeType)
                .ToListAsync();

            return likedComments.Select(x => x.CommentId).ToList();
        }

        // Yorumun beğenen kullanıcılarını getir (sayfalama ile)
        public async Task<List<UserLikeComment>> GetCommentLikersAsync(string commentId, string likeType = "like", int page = 1, int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;
            return await _userLikeCommentCollection
                .Find(x => x.CommentId == commentId && x.LikeType == likeType)
                .SortByDescending(x => x.CreatedAt)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }
    }
}