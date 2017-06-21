namespace BMSF.Reactive.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using ReactiveUI;

    public static class SetAndNotifyUsingBackingPropertyExtensions
    {
        public static TRet RaiseAndSetIfChangedUsingExpr<TObj, TRet>(
            this TObj This,
            object target,
            Expression<Func<TObj, TRet>> outExpr,
            TRet newValue,
            [CallerMemberName] string propertyName = null) where TObj : IReactiveObject
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            var expr = (MemberExpression) outExpr.Body;
            var prop = (PropertyInfo) expr.Member;

            if (EqualityComparer<TRet>.Default.Equals((TRet) prop.GetValue(target), newValue))
            {
                return newValue;
            }

            This.RaisePropertyChanging(propertyName);
            prop.SetValue(target, newValue);
            This.RaisePropertyChanged(propertyName);
            return newValue;
        }

        public static TRet RaiseAndSetIfChangedDelegate<TObj, TTarget, TRet>(
            this TObj This,
            TTarget target,
            Expression<Func<TTarget, TRet>> outExpr,
            TRet newValue,
            [CallerMemberName] string propertyName = null) where TObj : IReactiveObject
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            var expr = (MemberExpression) outExpr.Body;
            var prop = (PropertyInfo) expr.Member;

            if (EqualityComparer<TRet>.Default.Equals((TRet) prop.GetValue(target), newValue))
            {
                return newValue;
            }

            This.RaisePropertyChanging(propertyName);
            prop.SetValue(target, newValue);
            This.RaisePropertyChanged(propertyName);
            return newValue;
        }
    }
}