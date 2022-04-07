using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Demo
{
  [Serializable]
  public class ValidationException : Exception
  {
    public ValidationException()
    {
    }

    public ValidationException(string message)
      : base(message)
    {
    }

    public ValidationException(string message, Exception ex)
      : base(message, ex)
    {
    }

    public ValidationException(string message, params object[] args)
      : base(string.Format(CultureInfo.InvariantCulture, message, args))
    {
    }

    public ValidationException(Exception inner, string message, params object[] args)
      : base(string.Format(CultureInfo.InvariantCulture, message, args), inner)
    {
    }

    protected ValidationException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
    }
  }
}