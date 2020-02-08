using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils
{
    public class FindParameterExpressionVisitor : ExpressionVisitor
    {
        private readonly string parameterNameToFind;

        public FindParameterExpressionVisitor(string parameterNameToFind)
        {
            this.parameterNameToFind = parameterNameToFind;
        }

        public ParameterExpression SelectedParameter { get; private set; }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Name == parameterNameToFind)
                SelectedParameter = node;

            return base.VisitParameter(node);
        }
    }
}
