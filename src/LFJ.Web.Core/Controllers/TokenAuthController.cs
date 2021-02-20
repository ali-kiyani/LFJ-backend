using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using Abp.UI;
using LFJ.Authentication.External;
using LFJ.Authentication.JwtBearer;
using LFJ.Authorization;
using LFJ.Authorization.Users;
using LFJ.Models.TokenAuth;
using LFJ.MultiTenancy;
using Abp.Domain.Repositories;
using System.Web;
using Microsoft.Extensions.Configuration;
using Abp.Net.Mail;
using LFJ.Users.Dto;
using LFJ.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace LFJ.Controllers
{
    [Route("api/[controller]/[action]")]
    public class TokenAuthController : LFJControllerBase
    {
        private readonly LogInManager _logInManager;
        private readonly ITenantCache _tenantCache;
        private readonly UserManager _userManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Staff.Staff, int> _staffRepository;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly TokenAuthConfiguration _configuration;
        private readonly IExternalAuthConfiguration _externalAuthConfiguration;
        private readonly IExternalAuthManager _externalAuthManager;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IEmailSender _emailSender;
        private readonly UserRegistrationManager _userRegistrationManager;

        public TokenAuthController(
            LogInManager logInManager,
            ITenantCache tenantCache,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            TokenAuthConfiguration configuration,
            IExternalAuthConfiguration externalAuthConfiguration,
            IExternalAuthManager externalAuthManager,
            IWebHostEnvironment environment,
            UserManager userManager,
            IEmailSender emailSender,
            IRepository<User, long> userRepository,
            IRepository<Staff.Staff, int> staffRepository,
            UserRegistrationManager userRegistrationManager)
        {
            _logInManager = logInManager;
            _tenantCache = tenantCache;
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _userManager = userManager;
            _appConfiguration = environment.GetAppConfiguration();
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _configuration = configuration;
            _externalAuthConfiguration = externalAuthConfiguration;
            _externalAuthManager = externalAuthManager;
            _emailSender = emailSender;
            _userRegistrationManager = userRegistrationManager;
        }

        [HttpPost]
        public async Task ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.UserNameOrEmailAddress);
            if (user == null)
                throw new UserFriendlyException(L("InvalidRequest"), L("InvalidRequest"));
            if (!await CheckBackEndUserAsync(model.UserNameOrEmailAddress))
                throw new UserFriendlyException(L("InvalidRequest"), L("InvalidUserNameOrPassword"));
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = "http://" + _appConfiguration["LFJUrl:domainUrl"] + "/account/reset-password?id=" + user.Id.ToString() + "&token=" + HttpUtility.UrlEncode(token);
            await ForgotPasswordEmail(user, link);
            // TO do email
        }

        private async Task ForgotPasswordEmail(User user, string link)
        {
            //Get tenant of current login user
            var emailContent = System.IO.File.ReadAllText("EmailTemplates/ForgotPassword.html");
            emailContent = emailContent.Replace("##Name##", user.Name)
                                       .Replace("##Url##", link)
                                       .Replace("##CurrentYear##", DateTime.Now.Year.ToString());

            await _emailSender.SendAsync(user.EmailAddress, "Resetting your password", emailContent, true);
        }

        private async Task<bool> CheckBackEndUserAsync(string userNameOrEmailAddress)
        {
            var user = await _userRepository.FirstOrDefaultAsync(x =>
                (x.UserName == userNameOrEmailAddress || x.EmailAddress == userNameOrEmailAddress) && x.IsActive && !x.IsDeleted);
            if (user == null)
                return false;

            return true;
        }

        [HttpPost]
        public async Task<UserDto> VerifyResetPasswordToken([FromBody] VerifyUserTokenModel model)
        {
            await _userManager.InitializeOptionsAsync(AbpSession.TenantId);
            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            var result = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", model.ResetToken);
            if (!result)
            {
                throw new UserFriendlyException(L("InvalidRequest"), L("InvalidRequest"));
            }
            return ObjectMapper.Map<UserDto>(user);
        }

        [HttpPost]
        public async Task<bool> VerifyInvitationEmail([FromBody] VerifyInvitedUserModel model)
        {
            var user = await _staffRepository.GetAsync(model.id);
            if (user == null || !user.Email.Equals(model.email))
            {
                throw new UserFriendlyException(L("InvalidRequest"), L("InvalidRequest"));
            }
            return true;
        }

        [HttpPost]
        public async Task ResetPassword([FromBody] ResetPasswordModel model)
        {
            await _userManager.InitializeOptionsAsync(AbpSession.TenantId);
            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            CheckErrors(await _userManager.ResetPasswordAsync(user, model.ResetToken, model.Password));
        }


        [HttpPost]
        public async Task<AuthenticateResultModel> Authenticate([FromBody] AuthenticateModel model)
        {
            var loginResult = await GetLoginResultAsync(
                model.UserNameOrEmailAddress,
                model.Password,
                GetTenancyNameOrNull()
            );

            var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity));

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = loginResult.User.Id
            };
        }

        [HttpGet]
        public List<ExternalLoginProviderInfoModel> GetExternalAuthenticationProviders()
        {
            return ObjectMapper.Map<List<ExternalLoginProviderInfoModel>>(_externalAuthConfiguration.Providers);
        }

        [HttpPost]
        public async Task<ExternalAuthenticateResultModel> ExternalAuthenticate([FromBody] ExternalAuthenticateModel model)
        {
            var externalUser = await GetExternalUserInfo(model);

            var loginResult = await _logInManager.LoginAsync(new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider), GetTenancyNameOrNull());

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    {
                        var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity));
                        return new ExternalAuthenticateResultModel
                        {
                            AccessToken = accessToken,
                            EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                            ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds
                        };
                    }
                case AbpLoginResultType.UnknownExternalLogin:
                    {
                        var newUser = await RegisterExternalUserAsync(externalUser);
                        if (!newUser.IsActive)
                        {
                            return new ExternalAuthenticateResultModel
                            {
                                WaitingForActivation = true
                            };
                        }

                        // Try to login again with newly registered user!
                        loginResult = await _logInManager.LoginAsync(new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider), GetTenancyNameOrNull());
                        if (loginResult.Result != AbpLoginResultType.Success)
                        {
                            throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                                loginResult.Result,
                                model.ProviderKey,
                                GetTenancyNameOrNull()
                            );
                        }

                        return new ExternalAuthenticateResultModel
                        {
                            AccessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity)),
                            ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds
                        };
                    }
                default:
                    {
                        throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                            loginResult.Result,
                            model.ProviderKey,
                            GetTenancyNameOrNull()
                        );
                    }
            }
        }

        private async Task<User> RegisterExternalUserAsync(ExternalAuthUserInfo externalUser)
        {
            var user = await _userRegistrationManager.RegisterAsync(
                externalUser.Name,
                externalUser.Surname,
                externalUser.EmailAddress,
                externalUser.EmailAddress,
                Authorization.Users.User.CreateRandomPassword(),
                true,
                (int)UserTypeEnum.AGENT
            );

            user.Logins = new List<UserLogin>
            {
                new UserLogin
                {
                    LoginProvider = externalUser.Provider,
                    ProviderKey = externalUser.ProviderKey,
                    TenantId = user.TenantId
                }
            };

            await CurrentUnitOfWork.SaveChangesAsync();

            return user;
        }

        private async Task<ExternalAuthUserInfo> GetExternalUserInfo(ExternalAuthenticateModel model)
        {
            var userInfo = await _externalAuthManager.GetUserInfo(model.AuthProvider, model.ProviderAccessCode);
            if (userInfo.ProviderKey != model.ProviderKey)
            {
                throw new UserFriendlyException(L("CouldNotValidateExternalUser"));
            }

            return userInfo;
        }

        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
        {
            var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
            }
        }

        private string CreateAccessToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(expiration ?? _configuration.Expiration),
                signingCredentials: _configuration.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        private static List<Claim> CreateJwtClaims(ClaimsIdentity identity)
        {
            var claims = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            // Specifically add the jti (random nonce), iat (issued timestamp), and sub (subject/user) claims.
            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            });

            return claims;
        }

        private string GetEncryptedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken, AppConsts.DefaultPassPhrase);
        }
    }
}
