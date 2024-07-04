using AuthApi.Auth;
using AuthApi.Program;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AuthApi.Admin.Filters;

public class AdminAuthorizeFilter : IAuthorizationFilter {
    public void OnAuthorization(AuthorizationFilterContext context) {
        var path = context.HttpContext.Request.Path.Value;
        if (path is null or "") return;
        if (!path.StartsWith($"/{Routs.Admin}", StringComparison.OrdinalIgnoreCase)) return;

        var user = context.HttpContext.User;
        if (user.Identity is null || !user.Identity.IsAuthenticated) {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!user.IsInRole(Roles.Administrator)) {
            context.Result = new ForbidResult();
        }
    }
}