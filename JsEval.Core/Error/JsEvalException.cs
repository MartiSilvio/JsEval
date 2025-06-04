namespace JsEval.Core.Error
{
    public class JsEvalException : Exception
    {
        public JsEvalException()
        {
        }

        public JsEvalException(string message)
            : base(message)
        {
        }

        public JsEvalException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}