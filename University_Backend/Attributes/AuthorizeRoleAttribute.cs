using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Claims;

public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _role;

    public AuthorizeRoleAttribute(string role)
    {
        _role = role;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity.IsAuthenticated ||
            !user.Claims.Any(c => c.Type == "Role" && c.Value == _role))
        {
            context.Result = new ForbidResult();
        }
    }
}
