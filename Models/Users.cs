using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace projectone.Models
{
    public class Users
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        [BsonElement("name")]
        public string Name { get; set; } = null!;
        [BsonElement("lastName")]
        public string LastName { get; set; } = null!;
        [BsonElement("username")]
        public string UserName { get; set; } = null!;
        [BsonElement("insertTime")]
        public DateTime InsertTime { get; set; } = DateTime.UtcNow;
        [BsonElement("updateTime")]
        public DateTime? UpdateTime { get; set; }
        [BsonElement("deleteTime")]
        public DateTime? DeleteTime { get; set; }

    }

}
