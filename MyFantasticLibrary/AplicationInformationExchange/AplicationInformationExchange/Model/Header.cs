using System.Runtime.Serialization;

namespace AplicationInformationExchange.Model
{
    public class Header
    {
        public Header(int statusCode, int operationCode)
        {
            StatusCode = statusCode;
            OperationCode = operationCode;
        }
        public int StatusCode { get; private set; }
        public int OperationCode { get; private set; }
    }
}