using System.Linq.Expressions;

public static class ExpressionCombiner
{
    public static Expression<Func<T, bool>> CombineExpressions<T>(List<Expression<Func<T, bool>>> expressions)
    {
        if (expressions == null || expressions.Count == 0)
        {
            return x => true;
        }

        Expression<Func<T, bool>> combinedExpression = expressions[0];

        for (int i = 1; i < expressions.Count; i++)
        {
            combinedExpression = combinedExpression.And(expressions[i]);
        }

        return combinedExpression;
    }
}