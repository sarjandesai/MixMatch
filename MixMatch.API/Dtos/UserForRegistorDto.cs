using System.ComponentModel.DataAnnotations;

namespace MixMatch.API.Dtos
{
    public class UserForRegistorDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(8,MinimumLength=4,ErrorMessage="You must specify password between 4 and 8 charactors.")]
        public string Password { get; set; }
    }
}