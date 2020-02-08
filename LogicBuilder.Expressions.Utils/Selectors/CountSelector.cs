using LogicBuilder.Expressions.Utils.DataSource;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public class CountSelector : SelectorBase
    {
        public override SelectorTypeEnum SelectorTypeEnum => SelectorTypeEnum.Count;
        public FilterGroup FilterGroup { get; set; }

        public override Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters) 
            => FilterGroup == null 
                ? parentExpression.GetCountMethodCall() 
                : parentExpression.GetCountMethodCall(FilterGroup, GetSelectorParameterName());
    }
}
