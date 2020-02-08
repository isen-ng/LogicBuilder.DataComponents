using LogicBuilder.Expressions.Utils.DataSource;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public class WhereSelector : SelectorBase
    {
        public override SelectorTypeEnum SelectorTypeEnum => SelectorTypeEnum.Where;
        public FilterGroup FilterGroup { get; set; }

        public override Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters) 
            => parentExpression.GetWhere(FilterGroup, GetSelectorParameterName());
    }
}
