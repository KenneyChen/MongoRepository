using System;
using System.Collections.Generic;
using System.Text;

namespace MongoRepository
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class CollectionNameAttribute : Attribute
    {
        public CollectionNameAttribute(string name)

        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Empty collectionname not allowed", "name");

            this.Name = name;
        }

        public string Name { get; private set; } //定义一个属性 用于获取Collection名称
    }
}
