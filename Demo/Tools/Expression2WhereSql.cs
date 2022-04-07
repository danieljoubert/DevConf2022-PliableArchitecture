using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Demo
{
	public class Expression2WhereSql
	{
		public Expression2WhereSql(Expression expression)
		{
			_query = ExpandExpression(expression);
		}

		private readonly string _query;
		public string Query { get { return _query; } }

		public static string Convert(Expression expression)
		{
			return new Expression2WhereSql(expression).Query;
		}

		private string ExpandExpression(Expression expression)
		{
			switch (expression.NodeType)
			{
				case ExpressionType.Equal:
					return ExpandBinary(expression, "=", "is");
				case ExpressionType.AndAlso:
					return ExpandBinary(expression, "and");
				case ExpressionType.OrElse:
					return ExpandBinary(expression, "or");
				case ExpressionType.NotEqual:
					return ExpandBinary(expression, "<>", "is not");
				case ExpressionType.Or:
					return ExpandBinary(expression, "|");
				case ExpressionType.And:
					return ExpandBinary(expression, "&");
				case ExpressionType.GreaterThan:
					return ExpandBinary(expression, ">");
				case ExpressionType.GreaterThanOrEqual:
					return ExpandBinary(expression, ">=");
				case ExpressionType.LessThan:
					return ExpandBinary(expression, "<");
				case ExpressionType.LessThanOrEqual:
					return ExpandBinary(expression, "<=");
				case ExpressionType.Constant:
					return ExpandConstant(expression);
				case ExpressionType.MemberAccess:
					return ExpandMember(expression);
				case ExpressionType.Call:
					return ExpandConstant(expression);
				case ExpressionType.Parameter:
					return ((ParameterExpression)expression).Name;
				case ExpressionType.Lambda:
					return ExpandLambda(expression);
				case ExpressionType.Convert:
					return ExpandConvert(expression);
				case ExpressionType.Not:
					return ExpandNot(expression);
				default:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture,
						"Expression not supported. NodeType: {0}", expression.NodeType));
			}
		}
		private string ExpandNot(Expression expression)
		{
			var ue = (UnaryExpression)expression;
			return "not" + ExpandExpression(ue.Operand);
		}

		private string ExpandConvert(Expression expression)
		{
			var uex = (UnaryExpression)expression;
			string result = ExpandExpression(uex.Operand);
			return result == "null" ? result : string.Format(CultureInfo.InvariantCulture, "({0})", result);
		}

		private string ExpandLambda(Expression expression)
		{
			var lbd = (LambdaExpression)expression;
			return ExpandExpression(lbd.Body);
		}

		private string ExpandMember(Expression expression)
		{
			var me = (MemberExpression)expression;
			if (me.Expression == null && me.NodeType == ExpressionType.MemberAccess)
				return ExpandConstant(expression);
			switch (me.Expression.NodeType)
			{
				case ExpressionType.Constant:
					return ExpandConstant(expression);
				case ExpressionType.Parameter:
					return ExpandParameter(me);
				case ExpressionType.Convert:
					return me.Member.Name;
				case ExpressionType.MemberAccess:
					return ExpandConstant(expression);
				default:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture,
						"ExpandMember: Node type '{0}' is not supported",
						me.Expression.NodeType));
			}
		}

		private static string ExpandParameter(MemberExpression me)
		{
			PropertyInfo info = (PropertyInfo)me.Member;
			if (info.PropertyType == typeof(bool))
				return string.Format(CultureInfo.InvariantCulture, "([{0}] = 1)", me.Member.Name);
			return string.Format(CultureInfo.InvariantCulture, "[{0}]", me.Member.Name);
		}

		private string ExpandConstant(Expression expression)
		{
			switch (expression.NodeType)
			{
				case ExpressionType.Constant:
					var c = (ConstantExpression)expression;
					return WriteConstant(c.Value);
				case ExpressionType.Call:
				case ExpressionType.MemberAccess:
					{
						MethodCallExpression mce = expression as MethodCallExpression;
						if (mce != null)
							return ExpandMethodCall(mce);
						return ExpandMethodCall(expression);
					}
				default:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture,
						"ExpandConstant: Node type '{0}' is not supported",
						expression.NodeType));
			}
		}

		private string ExpandMethodCall(MethodCallExpression expression)
		{
			if (expression.Method.Name == "Contains")
			{
				Expression arg0 = expression.Arguments[0];
				if (arg0.Type == typeof(string))
					return ExpandLike(expression, arg0);
				return ExpandValueIn(expression, arg0);
			}
			return ExpandMethodCall((Expression)expression);
		}

		private string ExpandLike(MethodCallExpression expression, Expression arg0)
		{
			string column = ExpandExpression(expression.Object);
			var value = ExpandExpression(arg0).Trim('\'');
			return string.Format(CultureInfo.InvariantCulture, "{0} like '%{1}%'", column, value);
		}

		private string ExpandValueIn(MethodCallExpression expression, Expression arg0)
		{
			Expression arg1 = expression.Arguments[1];
			string values = ExpandExpression(arg0);
			string column = ExpandExpression(arg1);
			return string.Format(CultureInfo.InvariantCulture, "{0} in ({1})", column, values);
		}

		private static string ExpandMethodCall(Expression expression)
		{
			Delegate compiled = Expression.Lambda(expression).Compile();
			object value = compiled.DynamicInvoke();
			if (expression.Type.IsEnum)
				value = (int)value;
			return WriteConstant(value);
		}

		private static string WriteConstant(object value)
		{
			if (value == null)
				return "null";
			if (value is Guid)
				return string.Format(CultureInfo.InvariantCulture, "'{0}'", value);
			if (value is DateTime)
				return string.Format(CultureInfo.InvariantCulture, "'{0:yyyy-MM-dd HH:mm:ss.fff}'", value);
			string str = value as string;
			if (str != null)
				return string.Format(CultureInfo.InvariantCulture, "'{0}'", str.Replace("'", "''"));
			if (value is bool)
				return (true.Equals(value) ? 1 : 0).ToString(CultureInfo.InvariantCulture);
			Type type = value.GetType();
			if (type.IsArray)
			{
				IEnumerable e = value as IEnumerable;
				return string.Join(", ", e.Cast<object>().Select(WriteConstant));
			}
			if (type.IsEnum)
			{
				var enumUnderlyingType = type.GetEnumUnderlyingType();
				if (enumUnderlyingType == typeof(long))
					return string.Format(CultureInfo.InvariantCulture, "{0}", (long)value);
				if (enumUnderlyingType == typeof(int))
					return string.Format(CultureInfo.InvariantCulture, "{0}", (int)value);
				if (enumUnderlyingType == typeof(byte))
					return string.Format(CultureInfo.InvariantCulture, "{0}", (byte)value);
			}
			return value.ToString();
		}

		private string ExpandBinary(Expression expression, string op, string opRightNull = null)
		{
			var be = (BinaryExpression)expression;
			return ConcatBinary(be.Left, be.Right, op, opRightNull);
		}

		private string ConcatBinary(Expression left, Expression right, string op, string opRightNull)
		{
			string leftExpression = ExpandExpression(left);
			string rightExpression = ExpandExpression(right);
			string operation = rightExpression == "null" ? opRightNull : op;
			return string.Format(CultureInfo.InvariantCulture, "({0} {1} {2})", leftExpression, operation, rightExpression);
		}

	}
}
