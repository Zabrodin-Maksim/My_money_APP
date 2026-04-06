using My_money.Data.Repositories.IRepositories;
using My_money.Enums;
using My_money.Model;
using My_money.Services.IServices;
using My_money.Templates;
using System;
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

        public RegistrationService(IHouseholdRepository householdRepository, IUserRepository userRepository, IHouseholdMemberRepository householdMemberRepository, 
            IHouseholdFinanceRepository householdFinanceRepository, IUserFinanceRepository userFinanceRepository, IUserSessionService userSessionService)
        {
            _householdRepository = householdRepository;
            _userRepository = userRepository;
            _householdMemberRepository = householdMemberRepository;
            _householdFinanceRepository = householdFinanceRepository;
            _userFinanceRepository = userFinanceRepository;
            _userSessionService = userSessionService;
        }

        public async Task RegisterAdminAndHouseholdAsync(string username, string email, Household household)
        {
            if (await _userRepository.GetByEmailAsync(email) != null)
                throw new InvalidOperationException("Email is already registered. You can reset your password.");

            var password = PasswordGenerator.Generate();
            var passwordHash = PasswordHasher.HashPassword(password);

            await _userRepository.AddAsync(new User { DisplayName = username, Email = email , PasswordHash = passwordHash, IsActive = 0 });
            var user = await _userRepository.GetByEmailAsync(email);
            await _userFinanceRepository.AddAsync(new UserFinance { UserId = (user!).Id });

            await _householdRepository.AddAsync(household);
            await _householdMemberRepository.AddAsync(new HouseholdMember
            {
                HouseholdId = household.Id,
                UserId = (user!).Id,
                Role = nameof(HouseholdMemberRole.Admin),
                CanManageBudget = 1,
                CanManageMembers = 1
            });
            await _householdFinanceRepository.AddAsync(new HouseholdFinance { HouseholdId = household.Id });


            await EmailService.SendAsync(email, "Password Reset", EmailTemplates.NewUser(password));
        }

        public async Task RegisterUserAsync(string? passwordHash, string username, string email, HouseholdMemberRole role)
        {
            if(!_userSessionService.IsAuthenticated)
                throw new InvalidOperationException("User is not authenticated.");

            if(_userSessionService.CurrentHouseholdMember!.Role != nameof(HouseholdMemberRole.Admin))
                throw new InvalidOperationException("Only household admins can register new users.");

            if (await _userRepository.GetByEmailAsync(email) != null)
                throw new InvalidOperationException("Email is already registered.");

            var password = PasswordGenerator.Generate();

            if (passwordHash != null && role == HouseholdMemberRole.Child)
            {
                await _userRepository.AddAsync(new User { DisplayName = username, Email = email, PasswordHash = passwordHash, IsActive = 0 });
                await EmailService.SendAsync(email, "Password Reset", EmailTemplates.NewUser(password));
            }
            else
            {
                await _userRepository.AddAsync(new User { DisplayName = username, Email = email, PasswordHash = PasswordHasher.HashPassword(password), IsActive = 0 });
            }

            var user = await _userRepository.GetByEmailAsync(email);
            await _userFinanceRepository.AddAsync(new UserFinance { UserId = (user!).Id });

            switch (role)
            {
                case HouseholdMemberRole.Admin:
                    await _householdMemberRepository.AddAsync(new HouseholdMember
                    {
                        HouseholdId = _userSessionService.CurrentHouseholdMember!.HouseholdId,
                        UserId = (user!).Id,
                        Role = nameof(role),
                        CanManageBudget = 1,
                        CanManageMembers = 1
                    });
                    break;

                case HouseholdMemberRole.Partner:
                    await _householdMemberRepository.AddAsync(new HouseholdMember
                    {
                        HouseholdId = _userSessionService.CurrentHouseholdMember!.HouseholdId,
                        UserId = (user!).Id,
                        Role = nameof(role),
                        CanManageBudget = 1,
                        CanManageMembers = 0
                    });
                    break;

                case HouseholdMemberRole.Child:
                    await _householdMemberRepository.AddAsync(new HouseholdMember
                    {
                        HouseholdId = _userSessionService.CurrentHouseholdMember!.HouseholdId,
                        UserId = (user!).Id,
                        Role = nameof(role),
                        CanManageBudget = 0,
                        CanManageMembers = 0
                    });
                    break;
            }
            
        }
    }
}
