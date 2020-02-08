using LogicBuilder.Expressions.Utils.DataSource;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public class LastOrDefaultSelector : SelectorBase
    {
        public override SelectorTypeEnum SelectorTypeEnum => SelectorTypeEnum.LastOrDefault;
        public FilterGroup FilterGroup { get; set; }

        public override Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters)
            => FilterGroup == null
                ? parentExpression.GetLastOrDefault()
                : parentExpression.GetLastOrDefault(FilterGroup, GetSelectorParameterName());
    }
}
