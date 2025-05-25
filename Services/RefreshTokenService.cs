using Microsoft.Extensions.Options;
using MongoDB.Driver;
using projectone.Models;
using projectone.Config;

namespace projectone.Services
{
    public class RefreshTokenService
    {
        private readonly IMongoCollection<RefreshToken> _refreshTokenCollection;
        private readonly IConfiguration _configuration;

        public RefreshTokenService(IOptions<MongoDBSettings> settings, IConfiguration configuration)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _refreshTokenCollection = database.GetCollection<RefreshToken>(settings.Value.RefreshTokenCollectionName);
            _configuration = configuration;
        }

        // 📌 Yeni refresh token oluştur ve kaydet
        public async Task<RefreshToken> CreateRefreshTokenAsync(string userId, string newToken)
        {
            var expiresAt = DateTime.UtcNow.AddDays(int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"]!));

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = newToken,
                ExpiresAt = expiresAt,
                InsertTime = DateTime.UtcNow
            };

            await _refreshTokenCollection.InsertOneAsync(refreshToken);
            return refreshToken;
        }

        // 📌 Kullanıcının aktif (henüz expire veya revoke olmamış) refresh token'ını getir
        public async Task<RefreshToken?> GetActiveTokenByUserIdAsync(string userId)
        {
            return await _refreshTokenCollection.Find(x =>
                x.UserId == userId &&
                x.RevokeTime == null &&
                x.ExpiresAt > DateTime.UtcNow
            ).FirstOrDefaultAsync();
        }

        // 📌 Token string'e göre RefreshToken kaydını getir
        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _refreshTokenCollection.Find(x => x.Token == token).FirstOrDefaultAsync();
        }

        // 📌 Token'ı geçersiz kıl
        public async Task RevokeTokenAsync(string token)
        {
            var update = Builders<RefreshToken>.Update
                .Set(x => x.RevokeTime, DateTime.UtcNow)
                .Set(x => x.UpdateTime, DateTime.UtcNow);

            await _refreshTokenCollection.UpdateOneAsync(x => x.Token == token, update);
        }
    }
}