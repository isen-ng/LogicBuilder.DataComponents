using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public class AverageSelector : SelectorBase
    {
        public override SelectorTypeEnum SelectorTypeEnum => SelectorTypeEnum.Average;
        public string MemberFullName { get; set; }

        public override Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters) 
            => string.IsNullOrEmpty(MemberFullName) 
                ? parentExpression.GetAverageMethodCall()
                : parentExpression.GetAverageMethodCall
                (
                    MemberFullName,
                    GetSelectorParameterName()
                );
    }
}
