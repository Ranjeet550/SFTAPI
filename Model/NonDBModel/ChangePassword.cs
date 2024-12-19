using System.ComponentModel;

namespace SFT.Model.NonDBModel
{
    public class ChangePassword
    {
        [PasswordPropertyText]
        public string OldPassword { get; set; }

        [PasswordPropertyText]
        public string NewPassword { get; set; }
    }
}
