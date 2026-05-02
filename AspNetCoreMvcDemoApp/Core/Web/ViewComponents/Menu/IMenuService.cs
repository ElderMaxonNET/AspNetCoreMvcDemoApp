namespace AspNetCoreMvcDemoApp.Core.Web.ViewComponents.Menu
{
    public interface IMenuService
    {
        IEnumerable<MenuItem> GetMenuForUser(IUserContext userContext);
    }
}
