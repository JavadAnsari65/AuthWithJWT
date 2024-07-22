using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace sample3.Filter
{
    public class PermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permission;
        public PermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.HasClaim(c => c.Type == "permisons" && c.Value.Contains(_permission)))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
