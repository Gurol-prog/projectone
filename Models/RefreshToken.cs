using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace projectone.Models
{
    public class RefreshToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = null!;

        [BsonElement("token")]
        public string Token { get; set; } = null!;

        [BsonElement("expiresAt")]
        public DateTime ExpiresAt { get; set; }

        [BsonElement("insertTime")]
        public DateTime InsertTime { get; set; } = DateTime.UtcNow;

        [BsonElement("updateTime")]
        public DateTime? UpdateTime { get; set; }

        [BsonElement("revokeTime")]
        public DateTime? RevokeTime { get; set; }

        [BsonIgnore]
        public bool IsActive => RevokeTime == null && DateTime.UtcNow <= ExpiresAt;
    }
}
