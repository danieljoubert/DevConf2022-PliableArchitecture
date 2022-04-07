using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Demo
{
  public static class DataContextExtension
  {
    public static ICollection<T> RetrieveAll<T>(this IDataContext context, Expression<Func<T, bool>> where)
    {
      string whereSql = Expression2WhereSql.Convert(where);
      return context.RetrieveAll(whereSql, null, null, SortDirection.Ascending, typeof(T)).Cast<T>().ToList();
    }

    public static IDataStorable Retrieve(this IDataContext context, object id, Type type)
    {
      Expression<Func<IDataStorable, bool>> expression = v => v.Id == id;
      string where = Expression2WhereSql.Convert(expression);
      return (IDataStorable)context.Retrieve(@where, type);
    }

    public static object Retrieve(this IDataContext context, string where, Type type)
    {
      return context.RetrieveAll(where, null, null, SortDirection.Ascending, type).FirstOrDefault();
    }

    public static ICollection<object> RetrieveAll(this IDataContext context, string where, int? top, string orderBy, SortDirection sortDirection, Type type)
    {
      string query = GetSelectClause(type, top);
      query = AddWhere(query, where);
      query = AddOrderBy(query, orderBy, sortDirection);
      return context.RetrieveQuery(query, type);
    }

    public static void Delete(this IDataContext context, object id, Type type)
    {
      object itemToDelete = context.Retrieve(id, type);
      context.Delete(itemToDelete);
    }

    private static string AddWhere(string query, string where)
    {
      if (!string.IsNullOrEmpty(where))
        where = " where " + where;
      if (query.Contains("/*where*/"))
        return query.Replace("/*where*/", where);
      return query + where;
    }

    private static string AddOrderBy(string query, string @orderby, SortDirection sortDirection)
    {
      if (string.IsNullOrWhiteSpace(orderby))
        return query;
      string direction = sortDirection == SortDirection.Ascending ? string.Empty : "desc";
      return string.Format(CultureInfo.InvariantCulture, "{0} order by {1} {2}", query, orderby, direction);
    }

    private static readonly Regex SetTopInSelect = new Regex(@"^select", RegexOptions.IgnoreCase);

    private static string GetSelectClause(Type type, int? top)
    {
      string select = GetSelectClause(type);
      if (top != null)
        select = SetTopInSelect.Replace(select, string.Format(CultureInfo.InvariantCulture, "select top {0} ", top));
      return select;
    }

    private static string GetSelectClause(Type type)
    {
      //RetrieveQueryAttribute retrieveQuery = type.GetFirstAttribute<RetrieveQueryAttribute>();
      //if (retrieveQuery != null)
      //  return retrieveQuery.Query;
      string tableName = type.Name;
      TableAttribute table = type.GetFirstAttribute<TableAttribute>();
      if (table != null && !string.IsNullOrEmpty(table.Name))
        tableName = table.Name;
      return string.Format(CultureInfo.InvariantCulture, "select * from {0} with(nolock)", tableName);
    }
  }
}