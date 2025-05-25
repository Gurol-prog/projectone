using Microsoft.Extensions.Options;
using MongoDB.Driver;
using projectone.Config;
using projectone.Models;

namespace projectone.Services
{
    public class ContentService
    {
        private readonly IMongoCollection<Content> _contentCollection;
        private readonly UserLikeService _userLikeService;

        public ContentService(IOptions<MongoDBSettings> settings,UserLikeService userLikeService)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _contentCollection = database.GetCollection<Content>(settings.Value.ContentCollectionName);
            _userLikeService = userLikeService;
        }

        // Tüm aktif içerikleri getir
        public async Task<List<Content>> GetAllAsync() =>
            await _contentCollection.Find(x => x.DeleteTime == null).ToListAsync();

        // ID ile içerik getir
        public async Task<Content?> GetByIdAsync(string id) =>
            await _contentCollection.Find(x => x.Id == id && x.DeleteTime == null).FirstOrDefaultAsync();

        // Belirli bir kullanıcının tüm içeriklerini getir
        public async Task<List<Content>> GetByUserIdAsync(string userId) =>
            await _contentCollection.Find(x => x.UserId == userId && x.DeleteTime == null).ToListAsync();

        // Belirli bir kullanıcının içeriklerini sayfalama ile getir
        public async Task<List<Content>> GetByUserIdWithPaginationAsync(string userId, int page = 1, int pageSize = 10)
        {
            var skip = (page - 1) * pageSize;
            return await _contentCollection
                .Find(x => x.UserId == userId && x.DeleteTime == null)
                .SortByDescending(x => x.InsertTime)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }

        // Kullanıcının toplam içerik sayısını getir
        public async Task<long> GetContentCountByUserIdAsync(string userId) =>
            await _contentCollection.CountDocumentsAsync(x => x.UserId == userId && x.DeleteTime == null);

        // Yeni içerik oluştur
        public async Task CreateContentAsync(Content content) =>
            await _contentCollection.InsertOneAsync(content);

        // İçerik güncelle
        public async Task UpdateContentAsync(string id, Content contentIn) =>
            await _contentCollection.ReplaceOneAsync(x => x.Id == id, contentIn);

        // Soft delete (DeleteTime set et)
        public async Task SoftDeleteContentAsync(string id)
        {
            var update = Builders<Content>.Update.Set(x => x.DeleteTime, DateTime.UtcNow);
            await _contentCollection.UpdateOneAsync(x => x.Id == id, update);
        }

        // Hard delete (tamamen sil)
        public async Task DeleteContentAsync(string id) =>
            await _contentCollection.DeleteOneAsync(x => x.Id == id);

       // Kullanıcı içeriği beğensin/beğenmesin
        public async Task<(bool success, string message, int likeCount, int dislikeCount)> ToggleLikeAsync(string userId, string contentId, string likeType)
        {
            // İçeriğin var olup olmadığını kontrol et
            var content = await GetByIdAsync(contentId);
            if (content == null)
                return (false, "İçerik bulunamadı", 0, 0);

            // Kullanıcının mevcut beğenisini kontrol et
            var existingLike = await _userLikeService.GetUserLikeAsync(userId, contentId);

            if (existingLike == null)
            {
                // İlk kez beğeniyor
                var success = await _userLikeService.AddLikeAsync(userId, contentId, likeType);
                if (success)
                {
                    await UpdateContentLikeCountsAsync(contentId);
                    var stats = await _userLikeService.GetLikeStatsAsync(contentId);
                    return (true, $"İçerik {(likeType == "like" ? "beğenildi" : "beğenilmedi")}", stats.likeCount, stats.dislikeCount);
                }
                return (false, "Beğeni eklenemedi", 0, 0);
            }
            else if (existingLike.LikeType == likeType)
            {
                // Aynı aksiyonu tekrar yapıyor - beğeniyi geri al
                await _userLikeService.RemoveLikeAsync(userId, contentId);
                await UpdateContentLikeCountsAsync(contentId);
                var stats = await _userLikeService.GetLikeStatsAsync(contentId);
                return (true, $"{(likeType == "like" ? "Beğeni" : "Beğenmeme")} geri alındı", stats.likeCount, stats.dislikeCount);
            }
            else
            {
                // Farklı aksiyon yapıyor (like -> dislike veya dislike -> like)
                await _userLikeService.UpdateLikeAsync(userId, contentId, likeType);
                await UpdateContentLikeCountsAsync(contentId);
                var stats = await _userLikeService.GetLikeStatsAsync(contentId);
                return (true, $"İçerik {(likeType == "like" ? "beğenildi" : "beğenilmedi")}", stats.likeCount, stats.dislikeCount);
            }
        }

        // İçeriğin like/dislike sayılarını güncelle
        private async Task UpdateContentLikeCountsAsync(string contentId)
        {
            var stats = await _userLikeService.GetLikeStatsAsync(contentId);
            
            var update = Builders<Content>.Update
                .Set(x => x.LikeCount, stats.likeCount)
                .Set(x => x.DislikeCount, stats.dislikeCount)
                .Set(x => x.UpdateTime, DateTime.UtcNow);

            await _contentCollection.UpdateOneAsync(x => x.Id == contentId, update);
        }

        // Kullanıcının bir içeriği beğenip beğenmediğini kontrol et
        public async Task<string?> GetUserLikeStatusAsync(string userId, string contentId)
        {
            var userLike = await _userLikeService.GetUserLikeAsync(userId, contentId);
            return userLike?.LikeType;
        }

        // En çok beğenilen içerikleri getir
        public async Task<List<Content>> GetMostLikedContentsAsync(int limit = 10)
        {
            return await _contentCollection
                .Find(x => x.DeleteTime == null)
                .SortByDescending(x => x.LikeCount)
                .Limit(limit)
                .ToListAsync();
        }

        // Trend içerikleri (like - dislike oranına göre)
        public async Task<List<Content>> GetTrendingContentsAsync(int limit = 10)
        {
            return await _contentCollection
                .Find(x => x.DeleteTime == null)
                .SortByDescending(x => x.LikeCount - x.DislikeCount)
                .ThenByDescending(x => x.InsertTime)
                .Limit(limit)
                .ToListAsync();
        }
    }
}