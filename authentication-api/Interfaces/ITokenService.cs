using authentication_api.Models;

namespace authentication_api.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
