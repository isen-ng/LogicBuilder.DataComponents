using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public class GroupBySelector : SelectorBase
    {
        public override SelectorTypeEnum SelectorTypeEnum => SelectorTypeEnum.GroupBy;
        public string MemberFullName { get; set; }

        public override Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters)
            => string.IsNullOrEmpty(MemberFullName)
                ? parentExpression.GetGroupBy()
                : parentExpression.GetGroupBy
                (
                    MemberFullName,
                    GetSelectorParameterName()
                );
    }
}
