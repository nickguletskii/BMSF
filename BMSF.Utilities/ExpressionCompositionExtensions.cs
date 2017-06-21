namespace BMSF.Utilities
{
    using System;
    using System.Linq.Expressions;

    public static class ExpressionCompositionExtensions
    {
        public static Expression<Func<TA, TC>> Compose<TA, TB, TC>(Expression<Func<TB, TC>> f,
            Expression<Func<TA, TB>> g)
        {
            var ex = ReplaceExpressions(f.Body, f.Parameters[0], g.Body);

            return Expression.Lambda<Func<TA, TC>>(ex, g.Parameters[0]);
        }

        private static TExpr ReplaceExpressions<TExpr>(TExpr expression,
            Expression orig,
            Expression replacement)
            where TExpr : Expression
        {
            var replacer = new ExpressionReplacer(orig, replacement);
            return replacer.VisitAndConvert(expression, nameof(ReplaceExpressions));
        }

        private class ExpressionReplacer : ExpressionVisitor
        {
            private readonly Expression _from;
            private readonly Expression _to;

            public ExpressionReplacer(Expression from, Expression to)
            {
                this._from = from;
                this._to = to;
            }

            public override Expression Visit(Expression node)
            {
                if (node == this._from)
                {
                    return this._to;
                }
                return base.Visit(node);
            }
        }
    }
}