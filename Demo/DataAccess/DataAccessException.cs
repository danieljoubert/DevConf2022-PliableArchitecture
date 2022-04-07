using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Demo
{
  [Serializable]
  public class DataAccessException : Exception
  {
    public DataAccessException()
    {

    }

    public DataAccessException(string message)
      : base(message)
    {
    }

    public DataAccessException(string message, Exception ex)
      : base(message, ex)
    {
    }

    public DataAccessException(Exception ex, string message, params object[] args)
      : base(string.Format(CultureInfo.InvariantCulture, message, args), ex)
    {
    }

    protected DataAccessException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
