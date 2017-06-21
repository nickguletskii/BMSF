namespace BMSF.Utilities
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class PropertyUtils
    {
        public static PropertyInfo GetPropertyInfoFromLambda<TSource, TProperty>(
            Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");

            return propInfo;
        }
    }
}