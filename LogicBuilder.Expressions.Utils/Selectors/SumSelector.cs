using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public class SumSelector : SelectorBase
    {
        public override SelectorTypeEnum SelectorTypeEnum => SelectorTypeEnum.Sum;
        public string MemberFullName { get; set; }

        public override Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters)
            => string.IsNullOrEmpty(MemberFullName)
                ? parentExpression.GetSumMethodCall()
                : parentExpression.GetSumMethodCall
                (
                    MemberFullName,
                    GetSelectorParameterName()
                );
    }
}
