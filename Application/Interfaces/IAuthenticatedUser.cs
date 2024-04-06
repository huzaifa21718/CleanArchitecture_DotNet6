namespace WebApi.SharedServices
{
    public interface IAuthenticatedUser
    {
        string UserId { get; set; }
    }
}