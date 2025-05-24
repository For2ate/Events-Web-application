using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq.Expressions;

namespace EventApp.Core.Extensions {

    public static class ExpressionExtensions {

        public static Expression<Func<T, bool>>? CombineAnd<T>(this IEnumerable<Expression<Func<T, bool>>>? predicates) {

            if (predicates == null || !predicates.Any())
                return null;

            Expression<Func<T, bool>>? combined = null;
            ParameterExpression? firstParameter = null;

            foreach (var predicate in predicates) {
                if (predicate == null) continue;

                if (combined == null) {
                    combined = predicate;
                    firstParameter = predicate.Parameters.FirstOrDefault();
                } else {

                    if (firstParameter == null) {
                        firstParameter = predicate.Parameters.FirstOrDefault();
                        if (firstParameter == null) continue;
                    }

                    var rightBody = ParameterRebinder.ReplaceParameters(
                        new Dictionary<ParameterExpression, ParameterExpression> { { predicate.Parameters[0], firstParameter } },
                        predicate.Body
                    );

                    combined = Expression.Lambda<Func<T, bool>>(Expression.AndAlso(combined.Body, rightBody), firstParameter);
                
                }
            }
        
            return combined;
        
        }

    }

    internal class ParameterRebinder : ExpressionVisitor {
        
        private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

        private ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map) {
        
            _map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        
        }

        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp) {
        
            return new ParameterRebinder(map).Visit(exp);
        
        }

        protected override Expression VisitParameter(ParameterExpression p) {

            if (_map.TryGetValue(p, out var replacement)) {
                p = replacement;
            }
            return base.VisitParameter(p);
        
        }
    }


}
