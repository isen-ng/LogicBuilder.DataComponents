using LogicBuilder.Expressions.Utils.DataSource;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public class SingleSelector : SelectorBase
    {
        public override SelectorTypeEnum SelectorTypeEnum => SelectorTypeEnum.Single;
        public FilterGroup FilterGroup { get; set; }

        public override Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters)
            => FilterGroup == null
                ? parentExpression.GetSingle()
                : parentExpression.GetSingle(FilterGroup, GetSelectorParameterName());
    }
}
