using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Runtime.Serialization;


namespace MongoRepository
{
    [DataContract]
    [Serializable]
    [BsonIgnoreExtraElements(Inherited = true)]  //当BSON文档被反序列化时，每个元素的名称用于在类映射中查找匹配的成员。通常，如果没有找到匹配的成员，将抛出异常。如果要在反序列化期间忽略其他元素 使用这个特性
    public abstract class MongoEntityBase : IMongoEntityBase<string>
    {
        protected MongoEntityBase()
        {
            DB_ID = ObjectId.GenerateNewId().ToString();  //对id进行初始化
        }

        [DataMember]
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.ObjectId)]  //因为 ObjectId 这个结构体是不能序列化的,所以使用  [BsonRepresentation(BsonType.ObjectId)] 标记为这个字符串ID在mongo中代表ObjectId
        public virtual string DB_ID { get; set; }
    }

    public interface IMongoEntityBase<TKey>
    {

        [BsonId]
        TKey DB_ID { get; set; }
    }
    public interface IMongoEntityBase : IMongoEntityBase<string>
    {
    }
}
