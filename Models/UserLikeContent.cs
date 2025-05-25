using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace projectone.Models
{
    public class UserLikeContent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = null!;
        
        [BsonElement("contentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ContentId { get; set; } = null!;
        
        [BsonElement("likeType")]
        public string LikeType { get; set; } = null!; // "like" veya "dislike"
        
        [BsonElement("insertTime")]
        public DateTime InsertTime { get; set; } = DateTime.UtcNow;
    }
}