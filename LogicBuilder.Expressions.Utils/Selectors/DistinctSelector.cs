using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public class DistinctSelector : SelectorBase
    {
        public override SelectorTypeEnum SelectorTypeEnum => SelectorTypeEnum.Distinct;

        public override Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters)
            => parentExpression.GetDistinct();
    }
}
