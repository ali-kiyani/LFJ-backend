using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Net.Mail;
using Abp.Zero.Configuration;
using LFJ.Authorization.Accounts.Dto;
using LFJ.Authorization.Users;

namespace LFJ.Authorization.Accounts
{
    public class AccountAppService : LFJAppServiceBase, IAccountAppService
    {
        // from: http://regexlib.com/REDetails.aspx?regexp_id=1923
        public const string PasswordRegex = "(?=^.{8,}$)(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\\s)[0-9a-zA-Z!@#$%^&*()]*$";

        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly IRepository<Agents.Agents> _agentsRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly UserManager _userManager;

        public AccountAppService(
            UserRegistrationManager userRegistrationManager, IRepository<Agents.Agents> agentsRepository, UserManager userManager, IRepository<User, long> userRepository)
        {
            _userRegistrationManager = userRegistrationManager;
            _agentsRepository = agentsRepository;
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public async Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input)
        {
            var tenant = await TenantManager.FindByTenancyNameAsync(input.TenancyName);
            if (tenant == null)
            {
                return new IsTenantAvailableOutput(TenantAvailabilityState.NotFound);
            }

            if (!tenant.IsActive)
            {
                return new IsTenantAvailableOutput(TenantAvailabilityState.InActive);
            }

            return new IsTenantAvailableOutput(TenantAvailabilityState.Available, tenant.Id);
        }

        public async Task<RegisterOutput> Register(RegisterInput input)
        {
            var user = await _userRegistrationManager.RegisterAsync(
                input.Name,
                input.Surname,
                input.EmailAddress,
                input.UserName,
                input.Password,
                true, // Assumed email address is always confirmed. Change this if you want to implement email confirmation.
                (int)UserTypeEnum.AGENT
            );

            int count = (await _agentsRepository.GetAllListAsync()).Count;
            int maxPMcode = 0;
            if (count > 0) 
            {
                maxPMcode = (await _agentsRepository.GetAllListAsync()).Max(x => x.PMCode);
            }

            Agents.Agents agentBankInfo = new Agents.Agents
            {
                AccountName = input.AccountName,
                AccountNumber = input.AccountNumber,
                BankName = input.BankName,
                UserId = user.Id,
                PMCode = ++maxPMcode
            };
            await _agentsRepository.InsertAsync(agentBankInfo);
            var isEmailConfirmationRequiredForLogin = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin);

            return new RegisterOutput
            {
                CanLogin = user.IsActive && (user.IsEmailConfirmed || !isEmailConfirmationRequiredForLogin)
            };
        }

        public async Task<bool> VerifyEmailExist(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return await Task.FromResult(false);
            return await Task.FromResult(true);
        }

        public async Task<bool> VerifyUsernameExist(string username)
        {
            var user = await _userRepository.GetAllListAsync(x => x.UserName == username);
            if (user == null || user.Count == 0)
                return await Task.FromResult(false);
            return await Task.FromResult(true);
        }

        public async Task<RegisterOutput> RegisterInvited(RegisterInvitedInput input)
        {
            var user = await _userRegistrationManager.RegisterAsync(
                input.Name,
                input.Surname,
                input.EmailAddress,
                input.UserName,
                input.Password,
                true, // Assumed email address is always confirmed. Change this if you want to implement email confirmation.
                (int)UserTypeEnum.STAFF
            );

            var isEmailConfirmationRequiredForLogin = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin);

            return new RegisterOutput
            {
                CanLogin = user.IsActive && (user.IsEmailConfirmed || !isEmailConfirmationRequiredForLogin)
            };
        }
    }
}
