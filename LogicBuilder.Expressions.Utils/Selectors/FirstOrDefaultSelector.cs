using LogicBuilder.Expressions.Utils.DataSource;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public class FirstOrDefaultSelector : SelectorBase
    {
        public override SelectorTypeEnum SelectorTypeEnum => SelectorTypeEnum.FirstOrDefault;
        public FilterGroup FilterGroup { get; set; }

        public override Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters)
            => FilterGroup == null
                ? parentExpression.GetFirstOrDefault()
                : parentExpression.GetFirstOrDefault(FilterGroup, GetSelectorParameterName());
    }
}
