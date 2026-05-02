namespace AspNetCoreMvcDemoApp.Core.Web.ViewComponents.Menu
{
    public class MenuItem
    {
        public string Text { get; set; } = null!;
        public string Url { get; set; } = null!;
        public List<RoleTypes> AllowedRoles { get; set; } = [];
    }
}
