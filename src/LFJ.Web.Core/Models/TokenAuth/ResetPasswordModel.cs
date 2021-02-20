using Abp.Auditing;
using Abp.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LFJ.Models.TokenAuth
{
    public class ResetPasswordModel
    {
        [Required]
        public long UserId { get; set; }

        [Required]
        public string ResetToken { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        [DisableAuditing]
        public string Password { get; set; }
    }
}
