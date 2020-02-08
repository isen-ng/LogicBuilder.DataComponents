using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public class MinSelector : SelectorBase
    {
        public override SelectorTypeEnum SelectorTypeEnum => SelectorTypeEnum.Min;
        public string MemberFullName { get; set; }

        public override Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters)
            => string.IsNullOrEmpty(MemberFullName)
                ? parentExpression.GetMinMethodCall()
                : parentExpression.GetMinMethodCall
                (
                    MemberFullName,
                    GetSelectorParameterName()
                );
    }
}
