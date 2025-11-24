using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MongoRepository
{

    public interface IRepository<T> where T : IMongoEntityBase<string>
    {
        IMongoCollection<T> Collection { get; }

        bool Add(T entity);
        bool Delete(T delete, Expression<Func<T, bool>> conditions = null);
        bool Update(T update);

        bool Update(Expression<Func<T, bool>> conditions, Expression<Func<T, T>> updateExpression);

        List<T> Find(Expression<Func<T, bool>> conditions = null);
    }
}
