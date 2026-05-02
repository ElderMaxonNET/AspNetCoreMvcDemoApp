namespace AspNetCoreMvcDemoApp.Core.Web.ViewComponents.Menu
{
    public class MenuService : IMenuService
    {
        private readonly List<MenuItem> _items =
        [
            new MenuItem
            {
                Text = "Users",
                Url = "/Users/Index",
                AllowedRoles = [RoleTypes.Admin]
            },
            new MenuItem
            {
                Text = "Files",
                Url = "/UserFiles/Index",
                AllowedRoles = [RoleTypes.Admin, RoleTypes.Uploader]
            },
            new MenuItem
            {
                Text = "Download",
                Url = "/Download/Index",
                AllowedRoles = [RoleTypes.Admin, RoleTypes.Downloader]
            }
        ];

        public IEnumerable<MenuItem> GetMenuForUser(IUserContext userContext)
        {
            if (!userContext.IsAuthorized)
                return [];

            // Admin can see all menus
            if (userContext.Role == RoleTypes.Admin)
                return _items;

            return _items.Where(i => i.AllowedRoles.Contains(userContext.Role));
        }
    }
}
