namespace AspNetCoreMvcDemoApp.Core.Common.Exceptions
{
    public class UserNotFoundException(string message = "User not found.") : Exception(message);
}
