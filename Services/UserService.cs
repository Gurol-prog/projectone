using Microsoft.Extensions.Options;
using MongoDB.Driver;
using projectone.Config;
using projectone.Models;

namespace projectone.Services
{
    public class UsersService
    {
        private readonly IMongoCollection<Users> _usersCollection;

        public UsersService(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _usersCollection = database.GetCollection<Users>(settings.Value.UsersCollectionName);
        }

        public async Task<List<Users>> GetAllAsync() =>
            await _usersCollection.Find(U => U.DeleteTime == null).ToListAsync();

        public async Task<Users?> GetByIdAsync(string id) =>
        await _usersCollection.Find(user => user.Id == id).FirstOrDefaultAsync();
        public async Task CreateAsync(Users user) =>
        await _usersCollection.InsertOneAsync(user);
        public async Task UpdateAsync(string id, Users userIn) =>
        await _usersCollection.ReplaceOneAsync(user => user.Id == id, userIn);
        public async Task DeleteAsync(string id) =>
        await _usersCollection.DeleteOneAsync(user => user.Id == id);
    }
}