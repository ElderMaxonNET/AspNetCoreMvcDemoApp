namespace AspNetCoreMvcDemoApp.Core.Common.Exceptions
{
    public class UserNotAllowedException(string message = "You do not have permission to perform this action.") : Exception(message);
}
