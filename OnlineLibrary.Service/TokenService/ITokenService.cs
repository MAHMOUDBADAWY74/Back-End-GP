using OnlineLibrary.Data.Entities;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.TokenService
{
    public interface ITokenService
    {
        Task<string> GenerateJwtToken(ApplicationUser user);
    }
}