using My_money.Data.Repositories;
using My_money.Model;
using System;
using System.Threading.Tasks;

namespace My_money.Services
{
    public class UserFinanceService : IUserFinanceService
    {
        private readonly IUserFinanceRepository _userFinanceRepository;

        public UserFinanceService(IUserFinanceRepository userFinanceRepository)
        {
            _userFinanceRepository = userFinanceRepository;
        }

        public async Task AddUserFinanceAsync(UserFinance userFinance)
        {
            _ = userFinance ?? throw new ArgumentNullException(nameof(userFinance), "User finance cannot be null");

            await _userFinanceRepository.AddAsync(userFinance);
        }

        public async Task<UserFinance> GetUserFinanceAsync()
        {
            return await _userFinanceRepository.GetAsync() ?? await AddDefaultUserFinance();
        }

        public async Task UpdateUserFinanceAsync(float savings, float balance)
        {
            _ = savings >= 0 ? savings : throw new ArgumentOutOfRangeException(nameof(savings), "Savings cannot be negative");

            await _userFinanceRepository.UpdateAsync(new UserFinance
            {
                Savings = savings,
                Balance = balance
            });
        }

        private async Task<UserFinance> AddDefaultUserFinance()
        {
            var defaultFinance = new UserFinance
            {
                Savings = 0,
                Balance = 0
            };

            await _userFinanceRepository.AddAsync(defaultFinance);
            return defaultFinance;

        }
    }
}
