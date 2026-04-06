using My_money.Data.Repositories.IRepositories;
using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Threading.Tasks;

namespace My_money.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> AuthUserAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                throw new InvalidOperationException("Wrong email or password.");

            if (PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                return user;
            }
            throw new InvalidOperationException("Wrong email or password.");
        }
    }
}
