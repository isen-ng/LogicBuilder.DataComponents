using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LogicBuilder.Expressions.Utils
{
    public class FindAllParametersExpressionVisitor : ExpressionVisitor
    {
        private HashSet<ParameterExpression> _parameters { get; } = new HashSet<ParameterExpression>();

        public ICollection<ParameterExpression> Parameters => _parameters;

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (!_parameters.Contains(node))
                _parameters.Add(node);

            return base.VisitParameter(node);
        }
    }
}
