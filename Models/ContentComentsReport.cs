using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace projectone.Models
{
    public class ContentComentsReport
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

        [BsonElement("contentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ContentId { get; set; } = null!;

        [BsonElement("report")]
        public string? Report { get; set; }

        [BsonElement("inserTime")]
        public DateTime InsertTime { get; set; } = DateTime.UtcNow;
       
    }
}