using Microsoft.Extensions.Options;
using MongoDB.Driver;
using projectone.Config;
using projectone.Dtos;
using projectone.Models;

namespace projectone.Services
{
    public class UserPasswordService
    {
        private readonly IMongoCollection<UsersPassowrd> _passwordCollection;

        public UserPasswordService(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _passwordCollection = database.GetCollection<UsersPassowrd>(settings.Value.UserspaswordCollectionName);
        }

        public async Task CreateHashedPasswordAsync(UsersPasswordDto dto)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var passwordDoc = new UsersPassowrd
            {
                userId = dto.UserId,
                passwordHash = hashedPassword,
                InsertTime = DateTime.UtcNow
            };
            await _passwordCollection.InsertOneAsync(passwordDoc);
        }

        public async Task<bool> CheckPasswordAsync(string userId, string plainPassword)
        {
            var userPassword = await _passwordCollection
                .Find(x => x.userId == userId)
                .FirstOrDefaultAsync();

            if (userPassword == null)
                return false;

            return BCrypt.Net.BCrypt.Verify(plainPassword, userPassword.passwordHash);
        }
    }
}