using System.Linq;
using Microsoft.EntityFrameworkCore;
using Abp.Configuration;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Net.Mail;

namespace LFJ.EntityFrameworkCore.Seed.Host
{
    public class DefaultSettingsCreator
    {
        private readonly LFJDbContext _context;

        public DefaultSettingsCreator(LFJDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            int? tenantId = null;

            if (LFJConsts.MultiTenancyEnabled == false)
            {
                tenantId = MultiTenancyConsts.DefaultTenantId;
            }

            // Emailing
            AddSettingIfNotExists(EmailSettingNames.DefaultFromAddress, "lfjofficialmail@gmail.com");
            AddSettingIfNotExists(EmailSettingNames.DefaultFromDisplayName, "LFJ Official");
            AddSettingIfNotExists(EmailSettingNames.Smtp.UserName, "lfjofficialmail@gmail.com");
            //AddSettingIfNotExists(EmailSettingNames.Smtp.Domain, "smtp.gmail.com");
            AddSettingIfNotExists(EmailSettingNames.Smtp.EnableSsl, "true");
            AddSettingIfNotExists(EmailSettingNames.Smtp.Host, "smtp.gmail.com");
            AddSettingIfNotExists(EmailSettingNames.Smtp.Port, "587");
            AddSettingIfNotExists(EmailSettingNames.Smtp.Password, "lfj.official");
            AddSettingIfNotExists(EmailSettingNames.Smtp.UseDefaultCredentials, "false");

            // Languages
            AddSettingIfNotExists(LocalizationSettingNames.DefaultLanguage, "en", tenantId);
        }

        private void AddSettingIfNotExists(string name, string value, int? tenantId = null)
        {
            if (_context.Settings.IgnoreQueryFilters().Any(s => s.Name == name && s.TenantId == tenantId && s.UserId == null))
            {
                return;
            }

            _context.Settings.Add(new Setting(tenantId, null, name, value));
            _context.SaveChanges();
        }
    }
}
