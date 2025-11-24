using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace MongoRepository
{
    internal static class GlobleManage<T>
    {
        private static string _tableName;
        private static string _dateBaseName;
        private static string _mongoServerSettings;
        private static IMongoCollection<T> _mongoCollection;

        public static IMongoCollection<T> MongoCollection
        {
            get => _mongoCollection;

        }
        public static string DateBaseName
        {
            get => _dateBaseName;
        }

        public static string MongoServerSettings
        {
            get => _mongoServerSettings;
        }
        public static string TableName
        {
            get => _tableName;
        }

        static GlobleManage()
        {
            Init();
        }

        private static void Init()
        {
            //初始化连接字符串
            //string[] parm = ConfigurationManager.ConnectionStrings["MongoServerSettings"].ConnectionString.Split('/');

            //_dateBaseName = parm.Last();
            //_mongoServerSettings = ConfigurationManager.ConnectionStrings["MongoServerSettings"].ConnectionString.Replace(@"/" + _dateBaseName, ":30017");


            //根据实体类标注好的Attribute获取表名
            var entitytype = typeof(T);
            var attr = Attribute.GetCustomAttribute(entitytype, typeof(CollectionNameAttribute));
            //若Attribute不为空  获取标注的表名
            if (attr != null)
            {
                _tableName = ((CollectionNameAttribute)attr).Name;

            }
            else
            {
                //否则  如果类型是MongoEntityBase的派生类 获取类名作为表名
                if (typeof(MongoEntityBase).IsAssignableFrom(entitytype))
                {
                    // No attribute found, get the basetype
                    while (!entitytype.BaseType.Equals(typeof(MongoEntityBase)))
                    {
                        entitytype = entitytype.BaseType;
                    }
                }
                _tableName = entitytype.Name;
            }

            //添加实体类映射
            BsonClassMap.RegisterClassMap<T>(cm => cm.AutoMap());

            var username = "root";
            var password = "Ev4gxsiVvu";
            var mongoServer = "192.168.9.23";
            var databaseName="test_db";
            var _client = new MongoClient($"mongodb://{username}:{password}@{mongoServer}:30017/{databaseName}?authSource=admin");
            _mongoCollection = _client.GetDatabase(databaseName).GetCollection<T>(_tableName);
        }
    }
}

public static class ExpressionExtensions
{
    public static Expression Visit<TExpression>(this Expression expression, Func<TExpression, Expression> visitor)
        where TExpression : Expression
    {
        return ExpressionVisitor<TExpression>.Visit(expression, visitor);
    }

    public static TReturn Visit<TExpression, TReturn>(this TReturn expression, Func<TExpression, Expression> visitor)
        where TExpression : Expression
        where TReturn : Expression
    {
        return (TReturn)ExpressionVisitor<TExpression>.Visit(expression, visitor);
    }

    public static Expression<TDelegate> Visit<TExpression, TDelegate>(this Expression<TDelegate> expression, Func<TExpression, Expression> visitor)
        where TExpression : Expression
    {
        return ExpressionVisitor<TExpression>.Visit(expression, visitor);
    }
}

public class ExpressionVisitor<TExpression> : System.Linq.Expressions.ExpressionVisitor where TExpression : Expression
{
    private readonly Func<TExpression, Expression> _visitor;

    public ExpressionVisitor(Func<TExpression, Expression> visitor)
    {
        _visitor = visitor;
    }

    public override Expression Visit(Expression expression)
    {
        if (expression is TExpression && _visitor != null)
            expression = _visitor(expression as TExpression);

        return base.Visit(expression);
    }

    public static Expression Visit(Expression expression, Func<TExpression, Expression> visitor)
    {
        return new ExpressionVisitor<TExpression>(visitor).Visit(expression);
    }

    public static Expression<TDelegate> Visit<TDelegate>(Expression<TDelegate> expression, Func<TExpression, Expression> visitor)
    {
        return (Expression<TDelegate>)new ExpressionVisitor<TExpression>(visitor).Visit(expression);
    }

}

