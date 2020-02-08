using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public class MemberSelector : SelectorBase
    {
        public override SelectorTypeEnum SelectorTypeEnum => SelectorTypeEnum.Member;
        public string MemberFullName { get; set; }

        public override Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters) 
            => Expression.MakeMemberAccess
            (
                parentExpression,
                parentExpression.Type.GetMemberInfoFromFullName(MemberFullName)
            );
    }
}
