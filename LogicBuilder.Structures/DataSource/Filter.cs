﻿using System.Collections.Generic;

namespace LogicBuilder.Expressions.Utils.DataSource
{
    public class Filter
    {
        public Filter
            (
                string Field,
                string Oper,
                string Value = null,
                string ValueSourceMember = null,
                string ValueSourceType = null,
                string ValueSourceParameter = null
            )
        {
            this.Field = Field;
            this.Operator = Oper;
            this.Value = Value;
            this.ValueSourceMember = ValueSourceMember;
            this.ValueSourceType = ValueSourceType;
            this.ValueSourceParameter = ValueSourceParameter;
        }

        public Filter() { }

        public string Field { get; set; }
        public string Operator { get; set; }
        public object Value { get; set; }
        public string ValueSourceType { get; set; }
        public string ValueSourceParameter { get; set; }
        public string ValueSourceMember { get; set; }
        public string Logic { get; set; }
        public IEnumerable<Filter> Filters { get; set; }
    }
}
