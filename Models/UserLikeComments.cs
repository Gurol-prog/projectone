using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace projectone.Models
{
    public class UserLikeComment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = null!;
        
        [BsonElement("commentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CommentId { get; set; } = null!;
        
        [BsonElement("likeType")]
        public string LikeType { get; set; } = null!; // "like" veya "dislike"
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}