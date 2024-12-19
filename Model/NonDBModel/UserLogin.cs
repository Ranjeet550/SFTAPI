using System.ComponentModel;

namespace SFT.Model.NonDBModel
{
    public class UserLogin
    {
        public string Email { get; set; }
        [PasswordPropertyText]
        public string Password { get; set; }
    }
}
