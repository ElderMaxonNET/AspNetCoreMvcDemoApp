using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreMvcDemoApp.Core.Web.ViewComponents.Menu
{
    public class MenuViewComponent(IMenuService menuService, IUserContext userContext) : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menuItems = menuService.GetMenuForUser(userContext);
            return View(menuItems);
        }
    }
}
