using My_money.Model;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface IAuthService
    {
        Task<User> AuthUserAsync(string email, string password);
    }
}
