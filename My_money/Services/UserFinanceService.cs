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

        public async Task<int> AddUserFinanceAsync(UserFinance userFinance)
        {
            _ = userFinance ?? throw new ArgumentNullException(nameof(userFinance), "User finance cannot be null");

            return await _userFinanceRepository.AddAsync(userFinance);
        }

        public async Task<UserFinance> GetUserFinanceAsync()
        {
            return await _userFinanceRepository.GetAsync() ?? await AddDefaultUserFinance();
        }

        public async Task UpdateUserFinanceAsync(decimal savings, decimal balance)
        {
            _ = savings >= 0 ? savings : throw new ArgumentOutOfRangeException(nameof(savings), "Savings cannot be negative");
            
            var actualUserFinance = await GetUserFinanceAsync();
            await _userFinanceRepository.UpdateAsync(new UserFinance
            {
                Id = actualUserFinance.Id,
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
            return await _userFinanceRepository.GetAsync() ?? throw new Exception();
        }
    }
}
