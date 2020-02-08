using Contoso.Data.Entities;
using LogicBuilder.Expressions.Utils.DataSource;
using LogicBuilder.Expressions.Utils.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace LogicBuilder.Expressions.Utils.Tests
{
    public class SelectorTests
    {
        [Fact]
        public void Build_Where_AverageSelector()
        {
            LambdaExpression ex = QueryExtensions.BuildSelector
            (
                new List<SelectorBase>
                {
                    new WhereSelector
                    {
                        FilterGroup =  new FilterGroup
                        {
                            Logic = "and",
                            Filters = new List<Filter>
                            {
                                new Filter
                                {
                                    Field = "id",
                                    Logic = "and",
                                    Operator = "gt",
                                    Value = "1"
                                },
                                new Filter
                                {
                                    Field = "FirstName",
                                    Operator = "gt",
                                    ValueSourceParameter = "a",
                                    ValueSourceMember = "LastName"
                                }
                            }
                        }
                    },
                    new OrderBySelector
                    {
                        MemberFullName = "LastName",
                        ListSortDirection = Strutures.ListSortDirection.Ascending
                    },
                    new ThenBySelector
                    {
                        MemberFullName = "FirstName",
                        ListSortDirection = Strutures.ListSortDirection.Descending
                    },
                    new SkipSelector
                    {
                        Skip = 1
                    },
                    new TakeSelector
                    {
                        Take = 5
                    },
                    new AverageSelector
                    {
                        MemberFullName = "id"
                    }
                },
                typeof(Student),
                typeof(double)
            );

            Assert.NotNull(ex);
        }

        [Fact]
        public void Build_Agregates_Without_Grouping()
        {
            LambdaExpression ex = QueryExtensions.BuildSelector
            (
                new List<SelectorBase>
                {
                    new GroupBySelector(),
                    new OrderBySelector
                    {
                        MemberFullName = "Key"
                    },
                    new SelectSelector
                    {
                        BodySourceParameterName = "s",
                        Members = new List<SelectorDefinition>
                        {
                            new SelectorDefinition
                            {
                                //SourceType = typeof(IGrouping<int, Department>),
                                ParameterName = "c",
                                MemberName = "Sum_budget",
                                Selectors = new List<SelectorBase>
                                {
                                   new WhereSelector
                                   {
                                       SelectorParameterName = "b",
                                       FilterGroup = new FilterGroup
                                       {
                                           Logic = "and",
                                           Filters = new List<Filter>
                                            {
                                                //new Filter
                                                //{
                                                //    Field = "id",
                                                //    Logic = "and",
                                                //    Operator = "gt",
                                                //    Value = "1"
                                                //},
                                                new Filter
                                                {
                                                    Field = "departmentID",
                                                    Operator = "eq",
                                                    ValueSourceParameter = "s",
                                                    ValueSourceMember = "Count()"
                                                },
                                                //new Filter
                                                //{
                                                //    Field = "Key",
                                                //    Operator = "isnull",
                                                //    //ValueSourceParameter = "a",
                                                //    //ValueSourceMember = "Key"
                                                //}
                                            }
                                       }
                                   },
                                   new ToListSelector()
                                },
                                ResultType = typeof(List<Department>)
                                //ResultType = typeof(List<IGrouping<int, Department>>)
                            }
                        }
                    }
                },
                typeof(Department),
                typeof(IQueryable<object>)
            );
            
            Assert.NotNull(ex);
            //working
            //{s => s.GroupBy(a => 1).OrderBy(a => a.Key).Select(a => new AnonymousType1() {Sum_budget = s.Where(b => (b.DepartmentID == s.Count())).ToList()})}
            //Expression<Func<IQueryable<Department>, object>> exp = s => s.GroupBy(a => 1)
            //                                                                .OrderBy(a => a.Key)
            //                                                                .Select(a => new 
            //                                                                { 
            //                                                                    Sum_budget = s.Where(b => (b.DepartmentID == s.Count()))
            //                                                                                    .ToList() 
            //                                                                });
        }

        [Fact]
        public void Get_Select_New()
        {
            Type queryableType = typeof(IQueryable<Department>);
            ParameterExpression param = Expression.Parameter(queryableType, "q");
            Expression exp = param.GetSelectNew<Department>
            (
                new Dictionary<string, string> { { "Name", "Name" } }
            );
            Assert.NotNull(exp);
        }
    }
}
