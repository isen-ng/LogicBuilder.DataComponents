using LogicBuilder.Expressions.Utils.Strutures;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public class OrderBySelector : SelectorBase
    {
        public override SelectorTypeEnum SelectorTypeEnum => SelectorTypeEnum.OrderBy;
        public string MemberFullName { get; set; }
        public ListSortDirection ListSortDirection { get; set; }

        public override Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters) 
            => parentExpression.GetOrderByCall(MemberFullName, ListSortDirection, GetSelectorParameterName());
    }
}
