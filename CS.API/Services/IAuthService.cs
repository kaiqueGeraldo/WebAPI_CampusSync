using CS.Models;

namespace CS.API.Services
{
    public interface IAuthService
    {
        string CreateToken(User user);
    }
}
