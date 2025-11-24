using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Collections;

namespace MongoRepository
{
    public class Repository<T> : IRepository<T> where T : IMongoEntityBase<string>
    {

        private IMongoCollection<T> _mongoCollection = GlobleManage<T>.MongoCollection;
        public IMongoCollection<T> Collection => _mongoCollection;

        public bool Add(T entity)
        {
            try
            {
                _mongoCollection.InsertOne(entity);
                return true;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public bool Delete(T delete, Expression<Func<T, bool>> conditions = null)
        {
            try
            {
                string _id = string.Empty;
                if (conditions == null)
                {
                    foreach (var item in delete.GetType().GetProperties())
                    {
                        if (item.Name == "DB_ID" && item.GetValue(delete) != null)
                        {
                            _id = item.GetValue(delete).ToString();
                            var result = _mongoCollection.DeleteOne(new BsonDocument("_id", BsonValue.Create(new ObjectId(_id))));
                            return result.IsAcknowledged;
                        }
                    }
                }
                var res = _mongoCollection.DeleteOne(conditions);
                return res.IsAcknowledged;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public bool Update(Expression<Func<T, bool>> conditions, Expression<Func<T, T>> updateExpression)
        {
            try
            {
               
                // 初始化 UpdateDefinition 为 null
                UpdateDefinition<T> updateDefinition = null;

                // 遍历 update 对象的所有属性，生成更新操作
                var memberInitExpression = updateExpression.Body as MemberInitExpression;

                foreach (MemberBinding binding in memberInitExpression.Bindings)
                {
                    //为每个字段添加 Set 操作
                    string propertyName = binding.Member.Name;

                    var memberAssignment = binding as MemberAssignment;
                    if (memberAssignment == null)
                        throw new ArgumentException("The update expression MemberBinding must only by type MemberAssignment.", "updateExpression");

                    Expression memberExpression = memberAssignment.Expression;

                    ParameterExpression parameterExpression = null;
                    memberExpression.Visit((ParameterExpression p) =>
                    {
                        parameterExpression = p;
                        return p;
                    });


                    if (parameterExpression == null)
                    {
                        object value;

                        if (memberExpression.NodeType == ExpressionType.Constant)
                        {
                            var constantExpression = memberExpression as ConstantExpression;
                            if (constantExpression == null)
                                throw new ArgumentException(
                                    "The MemberAssignment expression is not a ConstantExpression.", "updateExpression");

                            value = constantExpression.Value;
                        }
                        else
                        {
                            LambdaExpression lambda = Expression.Lambda(memberExpression, null);
                            value = lambda.Compile().DynamicInvoke();
                        }

                        if (value != null)
                        {
                            // 为每个字段添加 Set 操作
                            // 动态创建 Set 更新操作
                            var setOperation = Builders<T>.Update.Set(propertyName, value);

                            // 如果 updateDefinition 已经有值，则使用 Combine 合并多个更新操作
                            updateDefinition = updateDefinition == null ? setOperation : Builders<T>.Update.Combine(updateDefinition, setOperation);
                        }

                        //TODO:生产需要删除这段代码
                        Console.WriteLine("变更字段：" + propertyName + "->" + value);
                    }
                }

                if (updateDefinition == null)
                {
                    // 没有任何字段需要更新
                    throw new ArgumentException(
                                   "The MemberAssignment expression not fields need to be updated");
                }

                var result = _mongoCollection.UpdateOne<T>(conditions, updateDefinition);

                // 返回更新结果
                return result.IsAcknowledged;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public bool Update(T update)
        {
            try
            {

                ObjectId _id;
                var options = new ReplaceOptions() { IsUpsert = true };

                foreach (var item in update.GetType().GetProperties())
                {
                    if (item.Name == "DB_ID" && item.GetValue(update) != null)
                    {
                        _id = new ObjectId(item.GetValue(update).ToString());
                        var result = _mongoCollection.ReplaceOne(new BsonDocument("_id", BsonValue.Create(_id)), update, options);
                        return result.IsAcknowledged;
                    }
                }

                var res = _mongoCollection.ReplaceOne(null, update, options);
                return res.IsAcknowledged;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public List<T> Find(Expression<Func<T, bool>> conditions = null)
        {
            try
            {
                if (conditions == null)
                {
                    conditions = t => true;
                }

                return _mongoCollection.Find(conditions).ToList() ?? new List<T>();

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
