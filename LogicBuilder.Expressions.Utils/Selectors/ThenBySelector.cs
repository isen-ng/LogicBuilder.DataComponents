using LogicBuilder.Expressions.Utils.Strutures;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public class ThenBySelector : SelectorBase
    {
        public override SelectorTypeEnum SelectorTypeEnum => SelectorTypeEnum.ThenBy;
        public string MemberFullName { get; set; }
        public ListSortDirection ListSortDirection { get; set; }

        public override Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters)
            => parentExpression.GetThenByCall(MemberFullName, ListSortDirection, GetSelectorParameterName());
    }
}
