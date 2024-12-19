using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SFT.Model
{
    public class UserAuth
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int User_Auth_Id { get; set; }

        [Required]
        public int User_ID { get; set; }

        [Required]
        [PasswordPropertyText]
        public string Password { get; set; }

        public bool AutoGenPass { get; set; }
    }
}
