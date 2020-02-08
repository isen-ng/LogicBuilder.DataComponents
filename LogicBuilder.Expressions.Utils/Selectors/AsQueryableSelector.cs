using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public class AsQueryableSelector : SelectorBase
    {
        public override SelectorTypeEnum SelectorTypeEnum => SelectorTypeEnum.AsQueryable;

        public override Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters)
            => parentExpression.GetAsQueryable();
    }
}
