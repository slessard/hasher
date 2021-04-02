using System;

namespace com.pigdawg.Hasher
{
    [Serializable]
    public class ArgumentParsingException : ArgumentException
    {
        private readonly int m_errorCode;
        
        public ArgumentParsingException (string message, string paramName, int errorCode)
            : this(message, paramName, errorCode, null/*innerException*/)
        {
        }
        
        public ArgumentParsingException (string message, string paramName, int errorCode, Exception innerException)
            : base(message, paramName, innerException)
        {
            m_errorCode = errorCode;
        }
        
        public int ErrorCode
        {
            get
            {
                return m_errorCode;
            }
        }
    }
}

