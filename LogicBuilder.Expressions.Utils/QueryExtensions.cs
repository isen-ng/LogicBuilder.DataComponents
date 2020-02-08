using LogicBuilder.Expressions.Utils.DataSource;
using LogicBuilder.Expressions.Utils.Selectors;
using LogicBuilder.Expressions.Utils.Strutures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace LogicBuilder.Expressions.Utils
{
    public static class QueryExtensions
    {
        /// <summary>
        /// Creates an OrderBy expression from a SortCollection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sortCollection"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static Expression<Func<IQueryable<T>, IQueryable<T>>> BuildOrderByExpression<T>(this SortCollection sortCollection) where T : class
        {
            if (sortCollection == null)
                return null;

            ParameterExpression param = Expression.Parameter(typeof(IQueryable<T>), "q");
            MethodCallExpression mce = param.GetOrderBy<T>(sortCollection);
            return Expression.Lambda<Func<IQueryable<T>, IQueryable<T>>>(mce, param);
        }

        /// <summary>
        /// Creates an order by method call expression to be invoked on an expression e.g. (parameter, member, method call) of type IQueryable<T>.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="expression"></param>
        /// <param name="sorts"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static MethodCallExpression GetOrderBy<TSource>(this Expression expression, SortCollection sorts)
        {
            MethodCallExpression resultExp = sorts.SortDescriptions.Aggregate(null, (MethodCallExpression mce, SortDescription description) =>
            {
                return mce == null
                    ? expression.GetOrderByCall(description.PropertyName, description.SortDirection)
                    : mce.GetThenByCall(description.PropertyName, description.SortDirection);
            });

            return resultExp.GetSkipCall(sorts.Skip).GetTakeCall(sorts.Take);
        }

        public static MethodCallExpression GetSkipCall(this Expression expression, int skip) 
            => Expression.Call
            (
                typeof(Queryable), 
                "Skip", 
                new[] { expression.GetUnderlyingElementType() }, 
                expression, 
                Expression.Constant(skip)
            );

        public static MethodCallExpression GetTakeCall(this Expression expression, int take)
            => Expression.Call
            (
                typeof(Queryable),
                "Take",
                new[] { expression.GetUnderlyingElementType() },
                expression,
                Expression.Constant(take)
            );

        public static MethodCallExpression GetOrderByCall(this Expression expression, string memberFullName, ListSortDirection sortDirection, string selectorParameterName = "a")
        {
            return GetCall(expression.GetUnderlyingElementType());
            MethodCallExpression GetCall(Type sourceType) 
                => Expression.Call
                (
                    typeof(Queryable),
                    sortDirection == ListSortDirection.Ascending ? "OrderBy" : "OrderByDescending",
                    new Type[] 
                    { 
                        sourceType, 
                        sourceType.GetMemberInfoFromFullName(memberFullName).GetMemberType() 
                    },
                    expression,
                    memberFullName.GetTypedSelector(sourceType, selectorParameterName)
                );
        }

        public static MethodCallExpression GetThenByCall(this Expression expression, string memberFullName, ListSortDirection sortDirection, string selectorParameterName = "a")
        {
            return GetCall(expression.GetUnderlyingElementType());
            MethodCallExpression GetCall(Type sourceType) 
                => Expression.Call
                (
                    typeof(Queryable),
                    sortDirection == ListSortDirection.Ascending ? "ThenBy" : "ThenByDescending",
                    new Type[] 
                    { 
                        sourceType, 
                        sourceType.GetMemberInfoFromFullName(memberFullName).GetMemberType() 
                    },
                    expression,
                    memberFullName.GetTypedSelector(sourceType, selectorParameterName)
                );
        }

        /// <summary>
        /// Creates an GroupBy expression from a property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="group"></param>
        /// <returns></returns>
        public static Expression<Func<IQueryable<T>, IQueryable<IGrouping<object, T>>>> BuildGroupByExpression<T>(this string group) where T : class
        {
            if (group == null)
                return null;

            ParameterExpression param = Expression.Parameter(typeof(IQueryable<T>), "q");
            MethodCallExpression mce = param.GetGroupBy<T>(group);

            return Expression.Lambda<Func<IQueryable<T>, IQueryable<IGrouping<object, T>>>>(mce, param);
        }

        /// <summary>
        /// Creates a group by method call expression to be invoked on an expression e.g. (parameter, member, method call) of type IQueryable<T>.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="expression"></param>
        /// <param name="groupByProperty"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static MethodCallExpression GetGroupBy<TSource>(this Expression expression, string groupByProperty, string parameterName = "a") 
            => expression.GetGroupBy(typeof(TSource), groupByProperty, parameterName);

        public static MethodCallExpression GetGroupBy(this Expression expression, Type sourceType, string groupByProperty, string parameterName = "a")
        {
            LambdaExpression selectorExpression = groupByProperty.GetTypedSelector(sourceType, parameterName);

            return Expression.Call
            (
                typeof(Queryable), 
                "GroupBy",
                new Type[] { sourceType, selectorExpression.ReturnType }, 
                expression,
                groupByProperty.GetObjectSelector(sourceType, parameterName)
            );
        }

        public static MethodCallExpression GetGroupBy(this Expression expression, string groupByProperty, string parameterName = "a")
        {
            Type sourceType = expression.GetUnderlyingElementType();
            LambdaExpression selectorExpression = groupByProperty.GetTypedSelector(sourceType, parameterName);

            return Expression.Call
            (
                typeof(Queryable),
                "GroupBy",
                new Type[] { sourceType, selectorExpression.ReturnType },
                expression,
                groupByProperty.GetObjectSelector(sourceType, parameterName)
            );
        }

        public static MethodCallExpression GetGroupBy(this Expression expression, string parameterName = "a")
        {
            Type sourceType = expression.GetUnderlyingElementType();
            return Expression.Call
            (
                typeof(Queryable), 
                "GroupBy",
                new Type[] { sourceType, typeof(int) }, 
                expression, 
                Expression.Lambda(Expression.Constant(1), Expression.Parameter(sourceType, parameterName))
            );
        }

        /// <summary>
        /// Creates a Where lambda expression from a filter group
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="group"></param>
        /// <returns></returns>
        public static Expression<Func<IQueryable<T>, IQueryable<T>>> BuildWhereExpression<T>(this DataSource.FilterGroup group) where T : class
        {
            if (group == null)
                return null;

            ParameterExpression param = Expression.Parameter(typeof(IQueryable<T>), "q");
            MethodCallExpression mce = param.GetWhere<T>(group);

            return Expression.Lambda<Func<IQueryable<T>, IQueryable<T>>>(mce, param);
        }

        /// <summary>
        /// Creates a Where method call expression to be invoked on an expression e.g. (parameter, member, method call) of type IQueryable<T>.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="expression"></param>
        /// <param name="filterGroup"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static MethodCallExpression GetWhere<TSource>(this Expression expression, DataSource.FilterGroup filterGroup) where TSource : class
        {
            LambdaExpression filterExpression = filterGroup.GetFilterExpression<TSource>();
            Type[] genericArgumentsForMethod = new Type[] { typeof(TSource) };

            return Expression.Call(typeof(Queryable), "Where", genericArgumentsForMethod, expression, filterExpression);
        }

        /// <summary>
        /// Creates a Where lambda expression from a filter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Expression<Func<IQueryable<T>, IQueryable<T>>> BuildWhereExpression<T>(this DataSource.Filter filter) where T : class
        {
            if (filter == null)
                return null;

            ParameterExpression param = Expression.Parameter(typeof(IQueryable<T>), "q");
            MethodCallExpression mce = param.GetWhere<T>(filter);

            return Expression.Lambda<Func<IQueryable<T>, IQueryable<T>>>(mce, param);
        }

        /// <summary>
        /// Creates a Where method call expression to be invoked on an expression e.g. (parameter, member, method call) of type IQueryable<T>.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="expression"></param>
        /// <param name="filter"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static MethodCallExpression GetWhere<TSource>(this Expression expression, DataSource.Filter filter) where TSource : class
        {
            LambdaExpression filterExpression = filter.GetFilterExpression<TSource>();
            Type[] genericArgumentsForMethod = new Type[] { typeof(TSource) };

            return Expression.Call(typeof(Queryable), "Where", genericArgumentsForMethod, expression, filterExpression);
        }

        public static MethodCallExpression GetWhere(this Expression expression, FilterGroup filterGroup, string parameterName = "a")
        {
            Type parentType = expression.GetUnderlyingElementType();
            LambdaExpression filterExpression = filterGroup.GetFilterExpression(parentType, parameterName, expression);
            Type[] genericArgumentsForMethod = new Type[] { parentType };

            return Expression.Call(typeof(Queryable), "Where", genericArgumentsForMethod, expression, filterExpression);
        }

        /// <summary>
        /// Function to create a lambda expression from a diverse group of method call expressions.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDest"></typeparam>
        /// <param name="param"></param>
        /// <param name="methodFunc"></param>
        /// <returns></returns>
        public static Expression<Func<TSource, TDest>> BuildLambdaExpression<TSource, TDest>(this ParameterExpression param, Func<ParameterExpression, Expression> methodFunc)
            where TSource : class
            where TDest : class 
            => Expression.Lambda<Func<TSource, TDest>>(methodFunc(param), param);

        /// <summary>
        /// Create select new anonymous type using a dynamically created class called "AnonymousType" i.e. q => q.Select(p => new { ID = p.ID, FullName = p.FullName });
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyFullNames"></param>
        /// <returns></returns>
        public static Expression<Func<IQueryable<T>, IQueryable<dynamic>>> BuildSelectNewExpression<T>(this ICollection<string> propertyFullNames) where T : class
        {
            if (propertyFullNames == null)
                return null;

            ParameterExpression param = Expression.Parameter(typeof(IQueryable<T>), "q");
            MethodCallExpression mce = param.GetSelectNew<T>(propertyFullNames);
            return Expression.Lambda<Func<IQueryable<T>, IQueryable<dynamic>>>(mce, param);
        }

        public static MethodCallExpression GetSelectNew<TSource>(this Expression expression, ICollection<string> propertyFullNames, string parameterName = "a") where TSource : class
        {
            ParameterExpression selectorParameter = Expression.Parameter(typeof(TSource), parameterName);
            
            return GetSelectNew<TSource>
            (
                expression,
                selectorParameter,
                GetMemberDetails<TSource>(propertyFullNames, selectorParameter)
            );
        }

        public static MethodCallExpression GetSelectNew<TSource>(this Expression expression, ParameterExpression selectorParameter, List<MemberDetails> memberDetails) where TSource : class
        {
            return expression.GetSelectMethodExpression<TSource>
            (
                memberDetails,
                selectorParameter,
                AnonymousTypeFactory.CreateAnonymousType(memberDetails)
            );
        }

        public static MethodCallExpression GetSelectNew(this Expression expression, Type sourceType, ParameterExpression selectorParameter, List<MemberDetails> memberDetails)
        {
            return expression.GetSelectMethodExpression
            (
                sourceType,
                memberDetails,
                selectorParameter,
                AnonymousTypeFactory.CreateAnonymousType(memberDetails)
            );
        }

        public static MethodCallExpression GetSelectNew(this Expression expression, Type sourceType, ParameterExpression selectorParameter, List<MemberDetails> memberDetails, Type newType)
        {
            return expression.GetSelectMethodExpression
            (
                sourceType,
                memberDetails,
                selectorParameter,
                newType
            );
        }

        /// <summary>
        /// Creates Distinct method call expression to run against a queryable
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MethodCallExpression GetDistinct(this Expression expression) => expression.GetMethodCall("Distinct");

        /// <summary>
        /// Creates Single method call expression to run against a queryable
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MethodCallExpression GetSingle(this Expression expression) => expression.GetMethodCall("Single");

        /// <summary>
        /// Creates Single method call expression to run against a queryable
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static MethodCallExpression GetSingle(this Expression expression, FilterGroup filterGroup, string filterParameterName = "a") 
            => expression.GetMethodCall
            (
                "Single",
                filterGroup.GetFilterExpression
                (
                    expression.GetUnderlyingElementType(),
                    filterParameterName
                )
            );

        /// <summary>
        /// Creates SingleOrDefault method call expression to run against a queryable
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MethodCallExpression GetSingleOrDefault(this Expression expression) => expression.GetMethodCall("SingleOrDefault");

        /// <summary>
        /// Creates SingleOrDefault method call expression to run against a queryable
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static MethodCallExpression GetSingleOrDefault(this Expression expression, FilterGroup filterGroup, string filterParameterName = "a") 
            => expression.GetMethodCall
            (
                "SingleOrDefault",
                filterGroup.GetFilterExpression
                (
                    expression.GetUnderlyingElementType(),
                    filterParameterName
                )
            );

        /// <summary>
        /// Creates First method call expression to run against a queryable
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MethodCallExpression GetFirst(this Expression expression) => expression.GetMethodCall("First");

        /// <summary>
        /// reates First method call expression to run against a queryable
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static MethodCallExpression GetFirst(this Expression expression, FilterGroup filterGroup, string filterParameterName = "a") 
            => expression.GetMethodCall
            (
                "First", 
                filterGroup.GetFilterExpression
                (
                    expression.GetUnderlyingElementType(),
                    filterParameterName
                )
            );

        /// <summary>
        /// Creates FirstOrDefault method call expression to run against a queryable
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static MethodCallExpression GetFirstOrDefault(this Expression expression) => expression.GetMethodCall("FirstOrDefault");

        /// <summary>
        /// Creates FirstOrDefault method call expression to run against a queryable
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static MethodCallExpression GetFirstOrDefault(this Expression expression, FilterGroup filterGroup, string filterParameterName = "a")
            => expression.GetMethodCall
            (
                "FirstOrDefault",
                filterGroup.GetFilterExpression
                (
                    expression.GetUnderlyingElementType(),
                    filterParameterName
                )
            );

        public static MethodCallExpression GetLast(this Expression expression) => expression.GetMethodCall("Last");

        public static MethodCallExpression GetLast(this Expression expression, FilterGroup filterGroup, string filterParameterName = "a")
            => expression.GetMethodCall
            (
                "Last",
                filterGroup.GetFilterExpression
                (
                    expression.GetUnderlyingElementType(),
                    filterParameterName
                )
            );

        public static MethodCallExpression GetLastOrDefault(this Expression expression) => expression.GetMethodCall("LastOrDefault");

        public static MethodCallExpression GetLastOrDefault(this Expression expression, FilterGroup filterGroup, string filterParameterName = "a")
            => expression.GetMethodCall
            (
                "LastOrDefault",
                filterGroup.GetFilterExpression
                (
                    expression.GetUnderlyingElementType(),
                    filterParameterName
                )
            );

        public static MethodCallExpression GetAny(this Expression expression) => expression.GetMethodCall("Any");

        public static MethodCallExpression GetAny(this Expression expression, FilterGroup filterGroup, string filterParameterName = "a") 
            => expression.GetMethodCall
            (
                "Any",
                filterGroup.GetFilterExpression
                (
                    expression.GetUnderlyingElementType(),
                    filterParameterName
                )
            );

        public static MethodCallExpression GetAll(this Expression expression, FilterGroup filterGroup, string filterParameterName = "a") 
            => expression.GetMethodCall
            (
                "All",
                filterGroup.GetFilterExpression
                (
                    expression.GetUnderlyingElementType(),
                    filterParameterName
                )
            );

        public static MethodCallExpression GetToList(this Expression parentExpression)
            => Expression.Call
            (
                typeof(Enumerable), 
                "ToList", 
                new Type[] { parentExpression.GetUnderlyingElementType() }, 
                parentExpression
            );

        public static MethodCallExpression GetAsQueryable(this Expression parentExpression)
            => Expression.Call(typeof(Queryable), "AsQueryable", new Type[] { parentExpression.GetUnderlyingElementType() }, parentExpression);

        internal static MethodCallExpression GetMethodCall(this Expression expression, string methodName, params Expression[] args)
            => Expression.Call
            (
                typeof(Queryable), 
                methodName, 
                new Type[] { expression.GetUnderlyingElementType() },
                new Expression[] { expression }.Concat(args).ToArray()
            );

        /// <summary>
        /// Create select new anonymous type using a dynamically created class called "AnonymousType" i.e. q => q.Select(p => new { ID = p.ID, FullName = p.FullName });
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyFullNames"></param>
        /// <returns></returns>
        public static Expression<Func<IQueryable<T>, IQueryable<dynamic>>> BuildSelectNewExpression<T>(this IDictionary<string, string> propertyFullNames) where T : class
        {
            if (propertyFullNames == null)
                return null;

            ParameterExpression param = Expression.Parameter(typeof(IQueryable<T>), "q");
            MethodCallExpression mce = param.GetSelectNew<T>(propertyFullNames);
            return Expression.Lambda<Func<IQueryable<T>, IQueryable<dynamic>>>(mce, param);
        }

        public static MethodCallExpression GetSelectNew<TSource>(this Expression expression, IDictionary<string, string> propertyFullNames, string parameterName = "a") where TSource : class
        {
            ParameterExpression selectorParameter = Expression.Parameter(typeof(TSource), parameterName);
            
            return GetSelectNew<TSource>
            (
                expression,
                selectorParameter,
                GetMemberDetails<TSource>(propertyFullNames, selectorParameter)
            );
        }

        private static MethodCallExpression GetSelectMethodExpression<TSource>(this Expression expression, List<MemberDetails> memberDetails, ParameterExpression param, Type newType) 
            => expression.GetSelectMethodExpression(typeof(TSource), memberDetails, param, newType);

        private static MethodCallExpression GetSelectMethodExpression(this Expression expression, Type sourceType, List<MemberDetails> memberDetails, ParameterExpression param, Type newType)
        {
            //Func<TSource, anonymous> s => new AnonymousType { Member = s.Member }
            LambdaExpression selectorExpression = Expression.Lambda
            (
                typeof(Func<,>).MakeGenericType(new Type[] { sourceType, newType }),
                GetInitExpression(memberDetails, newType),
                param
            );

            //IQueryable<anonymousType> Select<TSource, anonymousType>(this IQueryable<TSource> source, Expression<Func<TSource, anonymousType>> selector);
            return Expression.Call(typeof(Queryable), "Select", new Type[] { sourceType, newType }, expression, selectorExpression);
        }

        private static Expression GetInitExpression(List<MemberDetails> memberDetails, Type sourceType)
        {
            //Bind anonymous type's member to TSource's selector.
            IEnumerable<MemberBinding> bindings = memberDetails.Select
            (
                nameType => 
                {
                    Type memberType = sourceType.GetProperty(nameType.MemberName).PropertyType;
                    Type selectorType = nameType.Selector.Type;
                    return Expression.Bind(sourceType.GetProperty(nameType.MemberName), nameType.Selector);
                }
                //nameType => Expression.Bind
                //(
                //    //PropertyInfo for the anonymous type's member
                //    sourceType.GetProperty(nameType.MemberName),
                //    //Selector expression for the TSource member
                //    nameType.Selector
                //)
            );

            return Expression.MemberInit(Expression.New(sourceType), bindings);
        }

        private static List<MemberDetails> GetMemberDetails<TSource>(IDictionary<string, string> propertyFullNames, ParameterExpression selectorParameter)
            => propertyFullNames.Aggregate(new List<MemberDetails>(), (list, next) =>
            {
                Type t = typeof(TSource);
                List<string> fullNameList = next.Value.Split('.').Aggregate(new List<string>(), (lst, n) =>
                {
                    MemberInfo p = t.GetMemberInfo(n);
                    t = p.GetMemberType();
                    lst.Add(p.Name);
                    return lst;
                });

                list.Add(new MemberDetails
                {
                    Selector = fullNameList.Aggregate
                    (
                        (Expression)selectorParameter, (param, n) => Expression.MakeMemberAccess
                        (
                            param,
                            param.Type.GetMemberInfo(n)
                        )
                    ),
                    MemberName = next.Key,
                    Type = t
                });
                return list;
            });

        private static List<MemberDetails> GetMemberDetails<TSource>(ICollection<string> propertyFullNames, ParameterExpression selectorParameter)
            => propertyFullNames.Aggregate(new List<MemberDetails>(), (list, next) =>
            {
                Type t = typeof(TSource);
                List<string> fullNameList = next.Split('.').Aggregate(new List<string>(), (lst, n) =>
                {
                    MemberInfo p = t.GetMemberInfo(n);
                    t = p.GetMemberType();
                    lst.Add(p.Name);
                    return lst;
                });

                list.Add(new MemberDetails
                {
                    Selector = fullNameList.Aggregate
                    (
                        (Expression)selectorParameter, (param, n) => Expression.MakeMemberAccess
                        (
                            param, 
                            param.Type.GetMemberInfo(n)
                        )
                    ),
                    MemberName = string.Join("", fullNameList),
                    Type = t
                });
                return list;
            });

        /// <summary>
        /// Create a dictionary select from a list of properties in lieu of select new anonymous type.   New requires IL code.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyFullNames"></param>
        /// <returns></returns>
        public static Expression<Func<IQueryable<T>, IQueryable<Dictionary<string, object>>>> BuildSelectDictionaryExpression<T>(this ICollection<string> propertyFullNames) where T : class
        {
            if (propertyFullNames == null)
                return null;

            ParameterExpression param = Expression.Parameter(typeof(IQueryable<T>), "q");
            MethodCallExpression mce = param.GetSelectDictionary<T>(propertyFullNames);
            return Expression.Lambda<Func<IQueryable<T>, IQueryable<Dictionary<string, object>>>>(mce, param);
        }

        /// <summary>
        /// Creates a select dictionary method call expression to be invoked on an expression e.g. (parameter, member, method call) of type IQueryable<T> in lieu of select new anonymous type.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="expression"></param>
        /// <param name="propertyFullNames"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static MethodCallExpression GetSelectDictionary<TSource>(this Expression expression, ICollection<string> propertyFullNames, string parameterName = "a")
        {
            List<LambdaExpression> selectors = propertyFullNames.Aggregate(new List<LambdaExpression>(), (mems, next) => {
                mems.Add(next.GetTypedSelector<TSource>(parameterName));
                return mems;
            });

            ParameterExpression param = Expression.Parameter(typeof(TSource), parameterName);

            List<KeyValuePair<string, Expression>> dictionaryInitializers = propertyFullNames.Aggregate(new List<KeyValuePair<string, Expression>>(), (mems, next) => {
                string[] parts = next.Split('.');
                Expression parent = parts.Aggregate((Expression)param, (p, n) => Expression.MakeMemberAccess(p, p.Type.GetMemberInfo(n)));
                if (parent.Type.GetTypeInfo().IsValueType)//Convert value type expressions to object expressions otherwise
                    parent = Expression.Convert(parent, typeof(object));//Expression.Lambda below will throw an exception for value types

                mems.Add(new KeyValuePair<string, Expression>(next, parent));
                return mems;
            });

            //Dictionary<string, object>.Add
            MethodInfo addMethod = typeof(Dictionary<string, object>).GetMethod(
                "Add", new[] { typeof(string), typeof(object) });
            //Create a Dictionary here. Each entry is a single propperty and lambda expression to create the value
            ListInitExpression createDictionaryEntrySelector = Expression.ListInit(
                    Expression.New(typeof(Dictionary<string, object>)),
                    dictionaryInitializers.Select(kvp => Expression.ElementInit(addMethod, new Expression[] { Expression.Constant(kvp.Key), kvp.Value })));

            //Func<TSource, Dictionary<string, object>>
            LambdaExpression selectorExpression = Expression.Lambda<Func<TSource, Dictionary<string, object>>>(
                createDictionaryEntrySelector,
                param);

            Type[] genericArgumentsForSelectMethod = new Type[] { typeof(TSource), typeof(Dictionary<string, object>) };

            return Expression.Call(typeof(Queryable), "Select", genericArgumentsForSelectMethod, expression, selectorExpression);
        }

        /// <summary>
        /// Creates a list of navigation expressions from the list of period delimited navigation properties.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="includes"></param>
        /// <returns></returns>
        public static IEnumerable<Expression<Func<TSource, object>>> BuildIncludes<TSource>(this IEnumerable<string> includes)
            where TSource : class
            => includes.Select(include => BuildSelectorExpression<TSource>(include)).ToList();

        /// <summary>
        /// Build Selector Expression
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="fullName"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static Expression<Func<TSource, object>> BuildSelectorExpression<TSource>(string fullName, string parameterName = "i")
            => (Expression<Func<TSource, object>>)BuildSelectorExpression(typeof(TSource), fullName, parameterName);

        /// <summary>
        /// Build Selector Expression
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fullName"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static LambdaExpression BuildSelectorExpression(Type type, string fullName, string parameterName = "i")
        {
            ParameterExpression param = Expression.Parameter(type, parameterName);
            string[] parts = fullName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            Type parentType = type;
            Expression parent = param;

            for (int i = 0; i < parts.Length; i++)
            {
                if (parentType.IsList())
                {
                    parent = GetSelectExpression(parts.Skip(i), parent, parentType.GetUnderlyingElementType(), parameterName);//parentType is the underlying type of the member since it is an IEnumerable<T>
                    return Expression.Lambda
                    (
                        typeof(Func<,>).MakeGenericType(new[] { type, typeof(object) }),
                        parent,
                        param
                    );
                }
                else
                {
                    MemberInfo mInfo = parentType.GetMemberInfo(parts[i]);
                    parent = Expression.MakeMemberAccess(parent, mInfo);

                    parentType = mInfo.GetMemberType();
                }
            }

            if (parent.Type.IsValueType)//Convert value type expressions to object expressions otherwise
                parent = Expression.Convert(parent, typeof(object));//Expression.Lambda below will throw an exception for value types

            return Expression.Lambda
            (
                typeof(Func<,>).MakeGenericType(new[] { type, typeof(object) }),
                parent,
                param
            );
        }

        private static string ChildParameterName(this string currentParameterName)
        {
            string lastChar = currentParameterName.Substring(currentParameterName.Length - 1);
            if (short.TryParse(lastChar, out short lastCharShort))
            {
                return string.Concat(currentParameterName.Substring(0, currentParameterName.Length - 1), (lastCharShort++).ToString(CultureInfo.CurrentCulture));
            }
            else
            {
                return currentParameterName += "0";
            }
        }

        private static Expression GetSelectExpression(IEnumerable<string> parts, Expression parent, Type underlyingType, string parameterName)//underlying type because paranet is a collection
            => Expression.Call
            (
                typeof(Enumerable),//This is an Enumerable (not Queryable) select.  We are selecting includes for a member who is a collection
                "Select",
                new Type[] { underlyingType, typeof(object) },
                parent,
                BuildSelectorExpression(underlyingType, string.Join(".", parts), parameterName.ChildParameterName())//Join the remaining parts to create a full name
            );

        public static LambdaExpression BuildSelector(ICollection<SelectorBase> selectors, Type sourceType, Type resultType, string parameterName = "s")
        {
            Type queryableType = typeof(IQueryable<>).MakeGenericType(sourceType);
            ParameterExpression param = Expression.Parameter(queryableType, parameterName);
            Dictionary<string, ParameterExpression> parentParameters = new Dictionary<string, ParameterExpression> { { parameterName, param } };
            return Expression.Lambda
            (
                typeof(Func<,>).MakeGenericType(new[] { queryableType, resultType }),
                selectors.BuildBody(param, parentParameters),
                param
            );
        }

        public static Expression BuildBody(this ICollection<SelectorBase> selectors, ParameterExpression param, Dictionary<string, ParameterExpression> parentParameters) 
            => selectors.Aggregate((Expression)param, (ex, next) => next.GetExpression(ex, parentParameters));
    }

    public class MemberDetails
    {
        public Expression Selector { get; set; }
        public string MemberName { get; set; }
        public Type Type { get; set; }
    }

    public class SelectorDefinition
    {
        public string MemberName { get; set; }
        public ICollection<SelectorBase> Selectors { get; set; }
        public Type SourceType { get; set; }
        public Type ResultType { get; set; }
        public string ParameterName { get; set; }
    }

    public static class AnonymousTypeFactory
    {
        private static int classCount;

        public static Type CreateAnonymousType(IEnumerable<MemberDetails> memberDetails)
        {
            AssemblyName dynamicAssemblyName = new AssemblyName("TempAssembly");
            AssemblyBuilder dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder dynamicModule = dynamicAssembly.DefineDynamicModule("TempAssembly");
            TypeBuilder typeBuilder = dynamicModule.DefineType(GetAnonymousTypeName(), TypeAttributes.Public);
            MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            var builders = memberDetails.Select(info => new
            {
                FieldBuilder = typeBuilder.DefineField(string.Concat("_", info.MemberName), info.Type, FieldAttributes.Private),
                PropertyBuilder = typeBuilder.DefineProperty(info.MemberName, PropertyAttributes.HasDefault, info.Type, null),
                GetMethodBuilder = typeBuilder.DefineMethod(string.Concat("get_", info.MemberName), getSetAttr, info.Type, Type.EmptyTypes),
                SetMethodBuilder = typeBuilder.DefineMethod(string.Concat("set_", info.MemberName), getSetAttr, null, new Type[] { info.Type })
            });

            builders.ToList().ForEach(builder =>
            {
                ILGenerator getMethodIL = builder.GetMethodBuilder.GetILGenerator();
                getMethodIL.Emit(OpCodes.Ldarg_0);
                getMethodIL.Emit(OpCodes.Ldfld, builder.FieldBuilder);
                getMethodIL.Emit(OpCodes.Ret);

                ILGenerator setMethodIL = builder.SetMethodBuilder.GetILGenerator();
                setMethodIL.Emit(OpCodes.Ldarg_0);
                setMethodIL.Emit(OpCodes.Ldarg_1);
                setMethodIL.Emit(OpCodes.Stfld, builder.FieldBuilder);
                setMethodIL.Emit(OpCodes.Ret);

                builder.PropertyBuilder.SetGetMethod(builder.GetMethodBuilder);
                builder.PropertyBuilder.SetSetMethod(builder.SetMethodBuilder);
            });

            return typeBuilder.CreateTypeInfo().AsType();
        }

        private static string GetAnonymousTypeName()
            => "AnonymousType" + ++classCount;
    }
}
