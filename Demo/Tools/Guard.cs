using System;
using System.Globalization;
using System.Linq.Expressions;

namespace Demo
{
  public static class Guard
  {
    public static void Argument(bool argumentValid, string argumentName, string format, params object[] args)
    {
      if (argumentValid)
        return;
      string message = string.Format(CultureInfo.InvariantCulture, format, args);
      throw new ArgumentException(message, argumentName);
    }

    public static void ArgumentNotNull([ValidatedNotNull] object argumentValue, string argumentName)
    {
      if (argumentValue == null)
        throw new ArgumentNullException(argumentName);
    }

    public static void ArgumentNotNullOrEmpty(string argumentValue, string argumentName)
    {
      if (argumentValue == null)
        throw new ArgumentNullException(argumentName);
      if (string.IsNullOrEmpty(argumentValue))
        throw new ArgumentException("String cannot be empty.", argumentName);
    }
    public static void ArgumentIsNotNullOrWhiteSpace(string argumentValue, string argumentName)
    {
      if (argumentValue == null)
        throw new ArgumentNullException(argumentName);
      if (string.IsNullOrWhiteSpace(argumentValue))
        throw new ArgumentException("String cannot be empty.", argumentName);
    }

    public static void IsNotNull(object value, string format, params object[] args)
    {
      Validate(value != null, format, args);
    }

    public static void IsNotNull<T>(object value, string format, params object[] args)
      where T : Exception
    {
      Validate<T>(value != null, format, args);
    }

    public static void Validate(bool valid, string format, params object[] args)
    {
      if (valid)
        return;
      string message = string.Format(CultureInfo.InvariantCulture, format, args);
      throw new ValidationException(message);
    }

    public static void Validate<T>(bool valid, string format, params object[] args)
      where T : Exception
    {
      if (valid)
        return;
      string message = string.Format(CultureInfo.InvariantCulture, format, args);
      throw (Exception)Activator.CreateInstance(typeof(T), new object[] { message });
    }

    public static void ArgumentNotNull(Expression<Func<object>> expression)
    {
      ArgumentNotNull(expression, "expression");
      Func<object> func = expression.Compile();
      object value = func();
      if (value == null)
      {
        string name = GetVariableName(expression.Body);
        throw new ArgumentNullException(name);
      }
    }

    public static void ArgumentNotNullOrEmpty(Expression<Func<string>> expression)
    {
      ArgumentNotNull(expression, "expression");
      Func<string> func = expression.Compile();
      string value = func();
      if (string.IsNullOrEmpty(value))
      {
        string name = GetVariableName(expression.Body);
        if (value == null)
          throw new ArgumentNullException(name);
        throw new ArgumentException(value);
      }
    }

    private static string GetVariableName(Expression expression)
    {
      MemberExpression memberExpression = expression as MemberExpression;
      if (memberExpression == null)
        throw new ArgumentException("The body of the lambda expression can only contain one variable");
      return memberExpression.Member.Name;
    }

  }
}