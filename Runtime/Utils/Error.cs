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
        DownloadFailed, // "Failed to download file"
        TooManyRetries, // "We tried downloading the file too many times",
    }
    
    public class Error
    {
        private ErrorType _type = ErrorType.Unknown;
    
        private string _message = "An unknown error occurred.";
        
        private Exception _cause;
        
        public ErrorType Type => _type;
        
        public string Message => _message;
        
        public Exception Cause => _cause;
        
        public static Error Unknown => new (ErrorType.Unknown);
        
        public static Error NotAuthenticated => new (ErrorType.NotAuthenticated);
        
        public static Error InternetRequired => new (ErrorType.InternetRequired);
        
        public static Error UserHasNoAssignments => new (ErrorType.UserHasNoAssignments);
        
        public static Error FailedToLoadAssignments = new (ErrorType.FailedToLoadAssignments);
        
        public static Error FailedToLoadUserCode => new (ErrorType.FailedToLoadUserCode);
        
        public Error(string message)
        {
            _message = message;
        }

        public Error(string message, Exception cause)
        {
            _message = message;
        }

        public Error(ErrorType type, string message)
        {
            _type = type;
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