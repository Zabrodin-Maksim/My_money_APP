using My_money.Model;
using My_money.Data.Repositories.IRepositories;
using My_money.Services.IServices;
using System.Threading.Tasks;
using My_money.Templates;
using My_money.Utilities;

namespace My_money.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IUserRepository _userRepository;

        public PasswordResetService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task ChangePassword(User user, string newPassword)
        {
            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            user.IsActive = 1;

            await _userRepository.UpdateAsync(user);
        }

        public async Task ResetPasswordWithTemporary(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                return;
            }

            string tempPassword = PasswordGenerator.Generate();
            user.PasswordHash = PasswordHasher.HashPassword(tempPassword);
            user.IsActive = 0;

            await _userRepository.UpdateAsync(user);

            await EmailService.SendAsync(user.Email, "Password Reset", EmailTemplates.PasswordReset(tempPassword));
        }
    }
}
