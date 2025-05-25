using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace projectone.Models
{
    public class ContentComments
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        
        [BsonElement("contentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ContentId { get; set; } = null!;
        
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = null!;
        
        [BsonElement("comment")]
        public string Comment { get; set; } = null!;
        
        [BsonElement("parentCommentId")] // Alt yorumlar için (reply)
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ParentCommentId { get; set; }
        
        [BsonElement("likeCount")]
        public int LikeCount { get; set; } = 0;
        
        [BsonElement("dislikeCount")]
        public int DislikeCount { get; set; } = 0;
        
        [BsonElement("insertTime")]
        public DateTime InsertTime { get; set; } = DateTime.UtcNow;
        
        [BsonElement("updateTime")]
        public DateTime? UpdateTime { get; set; }
        
        [BsonElement("deleteTime")]
        public DateTime? DeleteTime { get; set; }
        
        // Navigation properties (MongoDB'de kullanılmaz ama DTO'larda yararlı)
        [BsonIgnore]
        public List<ContentComments>? Replies { get; set; }
        
        [BsonIgnore]
        public string? UserName { get; set; } // Kullanıcı adını göstermek için
    }
}