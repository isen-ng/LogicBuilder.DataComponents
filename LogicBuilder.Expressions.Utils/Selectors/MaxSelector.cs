using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public class MaxSelector : SelectorBase
    {
        public override SelectorTypeEnum SelectorTypeEnum => SelectorTypeEnum.Max;
        public string MemberFullName { get; set; }

        public override Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters)
            => string.IsNullOrEmpty(MemberFullName)
                ? parentExpression.GetMaxMethodCall()
                : parentExpression.GetMaxMethodCall
                (
                    MemberFullName,
                    GetSelectorParameterName()
                );
    }
}
