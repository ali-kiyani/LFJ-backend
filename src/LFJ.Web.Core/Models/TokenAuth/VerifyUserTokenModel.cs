
using System.ComponentModel.DataAnnotations;

namespace LFJ.Models.TokenAuth
{
    public class VerifyUserTokenModel
    {
        [Required]
        public long UserId { get; set; }

        [Required]
        public string ResetToken { get; set; }
    }
}
