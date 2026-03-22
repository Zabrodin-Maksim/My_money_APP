using My_money.Data.Repositories.IRepositories;
using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        public async Task AddToSavingsAsync(decimal amount)
        {
            var actualUserFinance = await GetUserFinanceAsync();
            await UpdateUserFinanceAsync(actualUserFinance.Savings + amount, null);
        }

        public async Task AddToBalanceAsync(decimal amount)
        {
            var actualUserFinance = await GetUserFinanceAsync();
            await UpdateUserFinanceAsync(null, actualUserFinance.Balance + amount);
        }

        /// <summary>
        /// Applies an expense to the user's balance.
        /// </summary>
        /// <param name="cost">
        /// The expense amount to deduct from the current balance.
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="cost"/> is negative.
        /// </exception>
        public async Task ApplyExpenseAsync(decimal amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Cost cannot be negative.");
            }

            var actualUserFinance = await GetUserFinanceAsync();
            await UpdateUserFinanceAsync(null, actualUserFinance.Balance - amount);
        }

        /// <summary>
        /// Updates the user's financial information.
        /// </summary>
        /// <param name="savings">
        /// The new savings amount. If null, the current value is preserved.
        /// </param>
        /// <param name="balance">
        /// The new balance amount. If null, the current value is preserved.
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">savings < 0</exception>
        public async Task UpdateUserFinanceAsync(decimal? savings, decimal? balance)
        {
            if (savings is < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(savings), "Savings cannot be negative");
            }

            var actualUserFinance = await GetUserFinanceAsync();
            await _userFinanceRepository.UpdateAsync(new UserFinance
            {
                Id = actualUserFinance.Id,
                Savings = savings ?? actualUserFinance.Savings,
                Balance = balance ?? actualUserFinance.Balance
            });
        }

        public async Task<UserFinance> AddDefaultUserFinance()
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
