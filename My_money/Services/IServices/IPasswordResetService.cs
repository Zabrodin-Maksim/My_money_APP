using My_money.Model;
using System.Threading.Tasks;

namespace My_money.Services.IServices
{
    public interface IPasswordResetService
    {
        Task ResetPasswordWithTemporary(string email);
        Task ChangePassword(User user, string newPassword);
    }
}
