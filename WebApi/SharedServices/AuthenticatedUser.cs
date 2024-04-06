using System.Security.Claims;

namespace WebApi.SharedServices
{
    public class AuthenticatedUser : IAuthenticatedUser
    {
        public AuthenticatedUser(IHttpContextAccessor httpContextAccessor)
        {
            UserId = httpContextAccessor.HttpContext.User.FindFirstValue("uid");
        }

        public string UserId { get; set; }
    }
}
