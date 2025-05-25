using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace projectone.Models
{
    public class UsersPassowrd
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string userId { get; set; } = null!;
        [BsonElement("passwordHash")]
        public string passwordHash { get; set; } = null!;
        [BsonElement("insertTime")]
        public DateTime InsertTime { get; set; } = DateTime.UtcNow;
        [BsonElement("updateTime")]
        public DateTime? UpdateTime { get; set; }
        [BsonElement("deleteTime")]
        public DateTime? DeleteTime { get; set; }
    }

}