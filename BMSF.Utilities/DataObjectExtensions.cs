namespace BMSF.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class DataObjectExtensions
    {
        public static PropertyInfo[] GetPublicProperties(this Type type)
        {
            if (type.IsInterface)
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    var subType = queue.Dequeue();
                    foreach (var subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface)) continue;

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    var typeProperties = subType.GetProperties(
                        BindingFlags.FlattenHierarchy
                        | BindingFlags.Public
                        | BindingFlags.Instance);

                    var newPropertyInfos = typeProperties
                        .Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }

            return type.GetProperties(BindingFlags.FlattenHierarchy
                                      | BindingFlags.Public | BindingFlags.Instance);
        }

        public static TU CopyPropertiesInto<T, TU>(this T source, TU dest,
            Func<PropertyInfo, PropertyInfo, bool> filter = null)
        {
            var sourceProps = typeof(T).GetPublicProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(TU).GetPublicProperties()
                .Where(x => x.CanWrite)
                .ToList();

            foreach (var sourceProp in sourceProps)
            {
                var targetProperty = destProps.FirstOrDefault(x => x.Name == sourceProp.Name);
                if (targetProperty != null && (filter?.Invoke(sourceProp, targetProperty) ?? true))
                {
                    targetProperty.SetValue(dest, sourceProp.GetValue(source, null), null);
                }
            }
            return dest;
        }

        public static T CopyPropertiesInto<T>(this T source, T dest)
        {
            return source.CopyPropertiesInto<T, T>(dest);
        }

        public static T CopyPropertiesIntoNew<T>(this T source) where T : new()
        {
            var newT = new T();
            source.CopyPropertiesInto(newT);
            return newT;
        }
    }
}