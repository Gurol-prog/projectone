using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace projectone.Models
{
    public class Content
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = null!;
       [BsonElement("contentName")]
        public string ContentName { get; set; } = null!;
        
        [BsonElement("contentDescription")]
        public string? ContentDescription { get; set; }
        
        [BsonElement("contentText")]
        public string? ContentText { get; set; }
        
        [BsonElement("contentUrl")]
        public string? ContentUrl { get; set; }
        
        [BsonElement("contentType")]
        public string? ContentType { get; set; }
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
    }
}