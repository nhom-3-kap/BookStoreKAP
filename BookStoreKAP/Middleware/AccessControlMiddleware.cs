using BookStoreKAP.Data;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BookStoreKAP.Common.Constants;
using System.Text.Encodings.Web;

namespace BookStoreKAP.Middleware
{
    public class AccessControlMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;

        public AccessControlMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<BookStoreKAPDBContext>();

            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var controllerDescriptor = endpoint.Metadata.OfType<ControllerActionDescriptor>().FirstOrDefault();
                var controllerName = controllerDescriptor?.ControllerName;
                var actionName = controllerDescriptor?.ActionName;
                var areaName = endpoint.Metadata.OfType<AreaAttribute>()?.FirstOrDefault()?.RouteValue;

                if (controllerName != null && actionName != null)
                {
                    var accessController = await _context.AccessControllers
                        .Include(ac => ac.Domains)
                        .ThenInclude(d => d.Role)
                        .FirstOrDefaultAsync(ac => ac.Name == controllerName && ac.AreaName == areaName && ac.Status == AccessControllerStatusConstant.PRIVATE);

                    if (accessController != null)
                    {
                        if (accessController.Status == AccessControllerStatusConstant.PRIVATE)
                        {
                            if (context.User.Identity != null && !context.User.Identity.IsAuthenticated)
                            {
                                string returnUrl = Uri.EscapeDataString(context.Request.Path.Value);
                                context.Response.Redirect($"{RouteConstant.LOGIN}?returnUrl={returnUrl}");
                                return;
                            }
                            else
                            {
                                var user = context.User;
                                if (user.IsInRole("Root"))
                                {
                                    await _next(context);
                                    return;
                                }
                                else
                                {
                                    var roles = accessController.Domains.Select(d => d.Role).ToList();
                                    if (roles.Any())
                                    {
                                        var hasRole = false;

                                        foreach (var role in roles)
                                        {
                                            if (user.IsInRole(role.Name))
                                            {
                                                hasRole = true;
                                                break;
                                            }
                                        }

                                        if (hasRole)
                                        {
                                            var hasAccess = false;
                                            foreach (var role in roles)
                                            {
                                                var claims = await _context.RoleClaims
                                                    .Where(rc => rc.RoleId == role.Id && rc.ControllerName == controllerName && rc.ActionName == actionName)
                                                    .ToListAsync();

                                                foreach (var claim in claims)
                                                {
                                                    if (user.HasClaim("Permission", claim.ClaimValue))
                                                    {
                                                        hasAccess = true;
                                                        break;
                                                    }
                                                }

                                                if (hasAccess)
                                                {
                                                    break;
                                                }
                                            }

                                            if (!hasAccess)
                                            {
                                                context.Response.Redirect("/Auth/AccessDenied");
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            context.Response.Redirect("/Auth/AccessDenied");
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        context.Response.Redirect("/Auth/AccessDenied");
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            await _next(context);
        }
    }
}
