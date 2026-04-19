using My_money.Data.Repositories.IRepositories;
using My_money.Enums;
using My_money.Model;
using My_money.Services.IServices;
using My_money.Constants;
using My_money.Templates;
using My_money.Utilities;
using System;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace My_money.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IHouseholdRepository _householdRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHouseholdMemberRepository _householdMemberRepository;
        private readonly IHouseholdFinanceRepository _householdFinanceRepository;
        private readonly IUserFinanceRepository _userFinanceRepository;
        private readonly IUserSessionService _userSessionService;
        private readonly IBudgetCategoryRepository _budgetCategoryRepository;

        public RegistrationService(
            IHouseholdRepository householdRepository,
            IUserRepository userRepository,
            IHouseholdMemberRepository householdMemberRepository,
            IHouseholdFinanceRepository householdFinanceRepository,
            IUserFinanceRepository userFinanceRepository,
            IUserSessionService userSessionService,
            IBudgetCategoryRepository budgetCategoryRepository)
        {
            _householdRepository = householdRepository;
            _userRepository = userRepository;
            _householdMemberRepository = householdMemberRepository;
            _householdFinanceRepository = householdFinanceRepository;
            _userFinanceRepository = userFinanceRepository;
            _userSessionService = userSessionService;
            _budgetCategoryRepository = budgetCategoryRepository;
        }

        public async Task RegisterAdminAndHouseholdAsync(string username, string email, Household household)
        {
            if (await _userRepository.GetByEmailAsync(email) != null)
                throw new InvalidOperationException("Email is already registered. You can reset your password.");

            var password = PasswordGenerator.Generate();
            var passwordHash = PasswordHasher.HashPassword(password);

            using var connection = new SQLiteConnection(_userRepository.ConnectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                await EmailService.SendAsync(email, "Password Reset", EmailTemplates.NewUser(password));

                var userId = await _userRepository.AddAsync(
                    new User
                    {
                        DisplayName = username,
                        Email = email,
                        PasswordHash = passwordHash,
                        IsActive = 0
                    },
                    connection,
                    transaction);

                await _userFinanceRepository.AddAsync(new UserFinance { UserId = userId }, connection, transaction);

                household.CreatedByUserId = userId;
                var householdId = await _householdRepository.AddAsync(household, connection, transaction);

                await _householdMemberRepository.AddAsync(
                    new HouseholdMember
                    {
                        HouseholdId = householdId,
                        UserId = userId,
                        Role = nameof(HouseholdMemberRole.Admin),
                        CanManageBudget = 1,
                        CanManageMembers = 1
                    },
                    connection,
                    transaction);

                await _householdFinanceRepository.AddAsync(new HouseholdFinance { HouseholdId = householdId }, connection, transaction);
                await CreateDefaultHouseholdOtherCategoryAsync(householdId, userId, connection, transaction);
                await CreateDefaultPersonalOtherCategoryAsync(householdId, userId, connection, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task RegisterUserAsync(string? passwordHash, string username, string email, HouseholdMemberRole role)
        {
            if (!_userSessionService.IsAuthenticated)
                throw new InvalidOperationException("User is not authenticated.");

            if (_userSessionService.CurrentHouseholdMember!.Role != nameof(HouseholdMemberRole.Admin))
                throw new InvalidOperationException("Only household admins can register new users.");

            if (await _userRepository.GetByEmailAsync(email) != null)
                throw new InvalidOperationException("Email is already registered.");

            if (role == HouseholdMemberRole.Child && string.IsNullOrWhiteSpace(passwordHash))
                throw new InvalidOperationException("Admin must set the initial password for child accounts.");

            string? passwordToEmail = null;
            var householdId = _userSessionService.CurrentHouseholdMember.HouseholdId;

            using var connection = new SQLiteConnection(_userRepository.ConnectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                int userId;

                if (passwordHash != null && role == HouseholdMemberRole.Child)
                {
                    userId = await _userRepository.AddAsync(
                        new User
                        {
                            DisplayName = username,
                            Email = email,
                            PasswordHash = passwordHash,
                            IsActive = 1
                        },
                        connection,
                        transaction);
                }
                else
                {
                    passwordToEmail = PasswordGenerator.Generate();
                    userId = await _userRepository.AddAsync(
                        new User
                        {
                            DisplayName = username,
                            Email = email,
                            PasswordHash = PasswordHasher.HashPassword(passwordToEmail),
                            IsActive = 0
                        },
                        connection,
                        transaction);
                }

                await _userFinanceRepository.AddAsync(new UserFinance { UserId = userId }, connection, transaction);

                await _householdMemberRepository.AddAsync(
                    new HouseholdMember
                    {
                        HouseholdId = householdId,
                        UserId = userId,
                        Role = nameof(role),
                        CanManageBudget = role == HouseholdMemberRole.Child ? 0 : 1,
                        CanManageMembers = role == HouseholdMemberRole.Admin ? 1 : 0
                    },
                    connection,
                    transaction);

                if (role != HouseholdMemberRole.Child)
                {
                    await CreateDefaultPersonalOtherCategoryAsync(householdId, userId, connection, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            if (passwordToEmail is not null)
            {
                await EmailService.SendAsync(email, "Password Reset", EmailTemplates.NewUser(passwordToEmail));
            }
        }

        private async Task CreateDefaultHouseholdOtherCategoryAsync(int householdId, int createdByUserId, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            if (await _budgetCategoryRepository.GetByNameAsync("Other", householdId, null, RecordConstants.Scopes.Shared) is not null)
            {
                return;
            }

            await _budgetCategoryRepository.AddAsync(
                new BudgetCategory
                {
                    Name = "Other",
                    Plan = 0m,
                    HouseholdId = householdId,
                    OwnerUserId = null,
                    Scope = RecordConstants.Scopes.Shared,
                    CreatedByUserId = createdByUserId
                },
                connection,
                transaction);
        }

        private async Task CreateDefaultPersonalOtherCategoryAsync(int householdId, int userId, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            if (await _budgetCategoryRepository.GetByNameAsync("Other", householdId, userId, RecordConstants.Scopes.Personal) is not null)
            {
                return;
            }

            await _budgetCategoryRepository.AddAsync(
                new BudgetCategory
                {
                    Name = "Other",
                    Plan = 0m,
                    HouseholdId = householdId,
                    OwnerUserId = userId,
                    Scope = RecordConstants.Scopes.Personal,
                    CreatedByUserId = userId
                },
                connection,
                transaction);
        }
    }
}
