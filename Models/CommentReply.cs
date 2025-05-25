using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace projectone.Models
{
    public class CommentReply
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
        [BsonElement("commentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CommentId { get; set; } = null!;
        [BsonElement("commentReply")]
        public string? CommentsReply { get; set; }

        [BsonElement("likeCount")]
        public int LikeCount { get; set; } = 0;
        [BsonElement("dislikeCount")]
        public int DislikeCount { get; set; } = 0;
        [BsonElement("inserTime")]
        public DateTime InsertTime { get; set; } = DateTime.UtcNow;
        [BsonElement("updateTime")]
        public DateTime? UpdateTime { get; set; }
        [BsonElement("deleteTime")]
        public DateTime? DeleteTime { get; set; }
    }
}