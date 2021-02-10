using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Domain.Repositories;
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

        public AccountAppService(
            UserRegistrationManager userRegistrationManager, IRepository<Agents.Agents> agentsRepository)
        {
            _userRegistrationManager = userRegistrationManager;
            _agentsRepository = agentsRepository;
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
                true // Assumed email address is always confirmed. Change this if you want to implement email confirmation.
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

    }
}
