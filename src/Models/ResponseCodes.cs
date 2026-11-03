namespace Common.Models;

public static class ResponseStatus
{
    public const string InvalidArgument = "InvalidArgument";
    public const string BadRequest = "BadRequest";
    public const string NotFound = "NotFound";
    public const string Unauthorized = "Unauthorized";
    public const string Forbidden = "Forbidden";
    public const string MethodNotAllowed = "MethodNotAllowed";
    public const string Error = "Error";
}

public static class ErrorCodes
{
    public static class Business
    {
        public const string Validation = "BUSINESS:Validation";
        public const string BadRequest = "BUSINESS:BadRequest";
        public const string NotFound = "BUSINESS:NotFound";
        public const string Unauthorized = "BUSINESS:Unauthorized";
        public const string Forbidden = "BUSINESS:Forbidden";
        public const string MethodNotAllowed = "BUSINESS:MethodNotAllowed";
    }

    public static class Unknown
    {
        public const string Unhandled = "UNKNOWN:Unhandled";
    }
}
