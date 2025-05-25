using Microsoft.Extensions.Options;
using MongoDB.Driver;
using projectone.Config;
using projectone.Models;

namespace projectone.Services
{
    public class UserLikeService
    {
        private readonly IMongoCollection<UserLikeContent> _userLikeCollection;

        public UserLikeService(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _userLikeCollection = database.GetCollection<UserLikeContent>("UserLikes");
            
            // Unique index oluştur - bir kullanıcı bir içeriği sadece bir kez beğenebilir
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexKeys = Builders<UserLikeContent>.IndexKeys
                .Ascending(x => x.UserId)
                .Ascending(x => x.ContentId);
            _userLikeCollection.Indexes.CreateOne(new CreateIndexModel<UserLikeContent>(indexKeys, indexOptions));
        }

        // Kullanıcının bu içeriği beğenip beğenmediğini kontrol et
        public async Task<UserLikeContent?> GetUserLikeAsync(string userId, string contentId)
        {
            return await _userLikeCollection
                .Find(x => x.UserId == userId && x.ContentId == contentId)
                .FirstOrDefaultAsync();
        }

        // Beğeni ekle
        public async Task<bool> AddLikeAsync(string userId, string contentId, string likeType)
        {
            try
            {
                var userLike = new UserLikeContent
                {
                    UserId = userId,
                    ContentId = contentId,
                    LikeType = likeType,
                    InsertTime = DateTime.UtcNow
                };

                await _userLikeCollection.InsertOneAsync(userLike);
                return true;
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                // Kullanıcı zaten bu içeriği beğenmiş
                return false;
            }
        }

        // Beğeniyi güncelle (like -> dislike veya dislike -> like)
        public async Task<bool> UpdateLikeAsync(string userId, string contentId, string newLikeType)
        {
            var update = Builders<UserLikeContent>.Update.Set(x => x.LikeType, newLikeType);
            var result = await _userLikeCollection.UpdateOneAsync(
                x => x.UserId == userId && x.ContentId == contentId,
                update);

            return result.ModifiedCount > 0;
        }

        // Beğeniyi sil
        public async Task<bool> RemoveLikeAsync(string userId, string contentId)
        {
            var result = await _userLikeCollection.DeleteOneAsync(
                x => x.UserId == userId && x.ContentId == contentId);

            return result.DeletedCount > 0;
        }

        // İçeriğin tüm beğenilerini sil (içerik silindiğinde)
        public async Task<bool> RemoveAllLikesForContentAsync(string contentId)
        {
            var result = await _userLikeCollection.DeleteManyAsync(x => x.ContentId == contentId);
            return result.DeletedCount > 0;
        }

        // İçeriğin beğeni istatistiklerini getir
        public async Task<(int likeCount, int dislikeCount)> GetLikeStatsAsync(string contentId)
        {
            var likes = await _userLikeCollection
                .CountDocumentsAsync(x => x.ContentId == contentId && x.LikeType == "like");
            
            var dislikes = await _userLikeCollection
                .CountDocumentsAsync(x => x.ContentId == contentId && x.LikeType == "dislike");

            return ((int)likes, (int)dislikes);
        }
    }
}