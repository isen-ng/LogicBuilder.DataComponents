using System.Collections.Generic;
using System.Linq.Expressions;

namespace LogicBuilder.Expressions.Utils.Selectors
{
    public abstract class SelectorBase
    {
        #region Constants
        protected const string DefaultSelectorParameterName = "a";
        #endregion Constants

        public abstract SelectorTypeEnum SelectorTypeEnum { get; }
        public abstract Expression GetExpression(Expression parentExpression, Dictionary<string, ParameterExpression> parentParameters);

        public string SourceType { get; set; }
        public string SelectorParameterName { get; set; }

        protected string GetSelectorParameterName()
            => string.IsNullOrEmpty(SelectorParameterName)
                    ? DefaultSelectorParameterName
                    : SelectorParameterName;
    }
}
