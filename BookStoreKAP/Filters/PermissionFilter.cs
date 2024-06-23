using Microsoft.AspNetCore.Mvc.Filters;

namespace BookStoreKAP.Filters
{
    public class PermissionFilter : ActionFilterAttribute
    {
        public required string Name { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }
    }
}
