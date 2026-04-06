using My_money.Data.Repositories.IRepositories;
using My_money.Enums;
using My_money.Model;
using My_money.Services.IServices;
using System;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Services
{
    public class UserFinanceService : IUserFinanceService
    {
        private readonly IUserFinanceRepository _userFinanceRepository;
        private readonly IUserSessionService _userSessionService;

        public UserFinanceService(IUserFinanceRepository userFinanceRepository, IUserSessionService userSessionService)
        {
            _userFinanceRepository = userFinanceRepository;
            _userSessionService = userSessionService;
        }

        private int GetAuthenticatedUserId()
        {
            if (!_userSessionService.IsAuthenticated)
                throw new InvalidOperationException("User is not authenticated.");

            return _userSessionService.CurrentUser!.Id;
        }

        public async Task<UserFinance?> GetByUserIdAsync(int userId)
        {
            return await _userFinanceRepository.GetByUserIdAsync(userId);
        }

        public async Task<int> AddUserFinanceAsync(UserFinance userFinance)
        {
            _ = userFinance ?? throw new ArgumentNullException(nameof(userFinance), "User finance cannot be null");
            return await _userFinanceRepository.AddAsync(userFinance);
        }

        public async Task ApplyExpenseAsync(decimal amount, IncomeTarget target)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");

            var actualUserFinance = await GetByUserIdAsync(GetAuthenticatedUserId()) ?? throw new InvalidOperationException("User finance not found.");
            switch (target)
            {
                case IncomeTarget.Balance:
                    actualUserFinance.Balance -= amount;
                    break;
                case IncomeTarget.Savings:
                    actualUserFinance.Savings -= amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), "Invalid income target.");
            }

            if (actualUserFinance.Savings < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Savings cannot be negative.");

            await _userFinanceRepository.UpdateAsync(actualUserFinance);
        }

        public async Task ApplyExpenseAsync(decimal amount, IncomeTarget target, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");

            var actualUserFinance = await _userFinanceRepository.GetByUserIdAsync(GetAuthenticatedUserId(), connection, transaction)
                ?? throw new InvalidOperationException("User finance not found.");

            switch (target)
            {
                case IncomeTarget.Balance:
                    actualUserFinance.Balance -= amount;
                    break;
                case IncomeTarget.Savings:
                    actualUserFinance.Savings -= amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), "Invalid income target.");
            }

            if (actualUserFinance.Savings < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Savings cannot be negative.");

            await _userFinanceRepository.UpdateAsync(actualUserFinance, connection, transaction);
        }

        public async Task ApplyIncomeAsync(decimal amount, IncomeTarget target)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");

            var actualUserFinance = await GetByUserIdAsync(GetAuthenticatedUserId()) ?? throw new InvalidOperationException("User finance not found.");
            switch (target)
            {
                case IncomeTarget.Balance:
                    actualUserFinance.Balance += amount;
                    break;
                case IncomeTarget.Savings:
                    actualUserFinance.Savings += amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), "Invalid income target.");
            }

            await _userFinanceRepository.UpdateAsync(actualUserFinance);
        }

        public async Task ApplyIncomeAsync(decimal amount, IncomeTarget target, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");

            var actualUserFinance = await _userFinanceRepository.GetByUserIdAsync(GetAuthenticatedUserId(), connection, transaction)
                ?? throw new InvalidOperationException("User finance not found.");

            switch (target)
            {
                case IncomeTarget.Balance:
                    actualUserFinance.Balance += amount;
                    break;
                case IncomeTarget.Savings:
                    actualUserFinance.Savings += amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), "Invalid income target.");
            }

            await _userFinanceRepository.UpdateAsync(actualUserFinance, connection, transaction);
        }

        public async Task UpdateUserFinanceAsync(decimal? savings, decimal? balance)
        {
            if (savings is < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(savings), "Savings cannot be negative");
            }

            var actualUserFinance = await GetByUserIdAsync(GetAuthenticatedUserId()) ?? throw new InvalidOperationException("User finance not found.");
            await _userFinanceRepository.UpdateAsync(new UserFinance
            {
                Id = actualUserFinance.Id,
                UserId = actualUserFinance.UserId,
                Savings = savings ?? actualUserFinance.Savings,
                Balance = balance ?? actualUserFinance.Balance
            });
        }

        public async Task DeleteUserFinance(int id)
        {
            await _userFinanceRepository.DeleteAsync(id);
        }
    }
}
