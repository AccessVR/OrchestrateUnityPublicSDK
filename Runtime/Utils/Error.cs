using System;

namespace AccessVR.OrchestrateVR.SDK
{
    public enum ErrorType
    {
        Unknown,
        NotAuthenticated,
        InternetRequired, // "Please connect your device to the Internet to continue. (2)"
        UserHasNoAssignments, // "You do not have any assignments."
        FailedToLoadUserCode, // "Failed to request user code"
        FailedToLoadAssignments, // "Failed to load your assignments. (1)"
    }
    
    public class Error
    {
        private ErrorType _type = ErrorType.Unknown;

        public ErrorType Type => _type;
        
        private string _message;
        
        public string Message => _message;
        
        private Exception _cause;
        
        public Exception Cause => _cause;
        
        public static Error Unknown => new Error(ErrorType.Unknown);
        
        public static Error NotAuthenticated => new Error(ErrorType.NotAuthenticated);
        
        public static Error InternetRequired => new Error(ErrorType.InternetRequired);
        
        public static Error UserHasNoAssignments => new Error(ErrorType.UserHasNoAssignments);
        
        public static Error FailedToLoadAssignments = new Error(ErrorType.FailedToLoadAssignments);
        
        public static Error FailedToLoadUserCode => new Error(ErrorType.FailedToLoadUserCode);
        
        public Error(string message)
        {
            _message = message;
        }

        public Error(string message, Exception cause)
        {
            _message = message;
        }
        
        public Error(Exception cause)
        {
            _cause = cause;
        }

        public Error(ErrorType type)
        {
            _type = type;
        }
        
        public Error(ErrorType type, Exception cause)
        {
            _type = type;
            _cause = cause;
        }
    }
}