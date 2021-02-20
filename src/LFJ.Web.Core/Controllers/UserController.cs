using Abp.Domain.Repositories;
using Abp.Net.Mail;
using Abp.UI;
using LFJ.Authorization.Users;
using LFJ.Configuration;
using LFJ.Users;
using LFJ.Users.Dto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LFJ.Controllers
{
    [Route("api/[controller]/[action]")]
    public class UserController : LFJControllerBase
    {
        private readonly IUserAppService _userAppService;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IEmailSender _emailSender;
        private readonly UserManager _userManager;

        public UserController(
            IUserAppService userAppService,
            IEmailSender emailSender,
            UserManager userManager,
            IWebHostEnvironment environment
            )
        {
            _userAppService = userAppService;
            _emailSender = emailSender;
            _userManager = userManager;
            _appConfiguration = environment.GetAppConfiguration();
        }

        [HttpPost]
        public async Task<string> InviteNewUser([FromBody]InviteUserDto inviteDto)
        {
            var userExists = await _userManager.FindByNameOrEmailAsync(inviteDto.Email);
            if (userExists != null)
            {
                return await Task.FromResult("DuplicateEmail");
            }
            var id = await _userAppService.InviteUser(inviteDto);
            var link = "http://" + _appConfiguration["LFJUrl:domainUrl"] + "/account/user-invitation?id=" + id +"&email=" + HttpUtility.UrlEncode(inviteDto.Email);
            await InviteUserEmail(inviteDto, link);
            return await Task.FromResult("Invited");
        }

        private async Task InviteUserEmail(InviteUserDto inviteDto, string link)
        {
            //Get tenant of current login user
            var emailContent = System.IO.File.ReadAllText("EmailTemplates/InviteUser.html");
            emailContent = emailContent.Replace("##Name##", inviteDto.Name)
                                       .Replace("##Url##", link)
                                       .Replace("##CurrentYear##", DateTime.Now.Year.ToString());

            await _emailSender.SendAsync(inviteDto.Email, "You are Invited to LFJ", emailContent, true);
        }
    }
}
