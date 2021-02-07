using System.ComponentModel.DataAnnotations;

namespace LFJ.Users.Dto
{
    public class ChangeUserLanguageDto
    {
        [Required]
        public string LanguageName { get; set; }
    }
}