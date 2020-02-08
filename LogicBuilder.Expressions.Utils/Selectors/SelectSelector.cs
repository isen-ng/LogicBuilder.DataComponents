using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public class SelectSelector : SelectorBase
    {
        public override SelectorTypeEnum SelectorTypeEnum => SelectorTypeEnum.Select;

        public ICollection<SelectorDefinition> Members { get; set; }
        public Type DestinationType { get; set; }
        public string BodySourceParameterName { get; set; }

        public override Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters)
        {
            Type sourceType = parentExpression.GetUnderlyingElementType();
            Type queryableType = parentExpression.Type;
            
            ParameterExpression selectorParameter = Expression.Parameter(sourceType, GetSelectorParameterName());

            ParameterExpression bodyDourceParameter = parentExpression.FindParameterByName("s") ?? Expression.Parameter(queryableType, "z");
            List<MemberDetails> memberDetalsList = Members.Select
            (
                member => new MemberDetails 
                {
                    MemberName = member.MemberName,
                    Selector = member.Selectors.BuildBody(bodyDourceParameter, parentParameters),
                    Type = member.ResultType
                }
            ).ToList();

            return DestinationType == null
                ? parentExpression.GetSelectNew(sourceType, selectorParameter, memberDetalsList)
                : parentExpression.GetSelectNew(sourceType, selectorParameter, memberDetalsList, DestinationType);
        }
    }
}
