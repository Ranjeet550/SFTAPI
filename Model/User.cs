using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SFT.Model
{
    public class User
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int User_ID { get; set; } 

            
            public string FirstName { get; set; }

            public string MiddleName { get; set; }

            public string LastName { get; set; }

            [Phone]
            public string PhoneNumber { get; set; }

           
            public string EmailAddress { get; set; }

            public string Designation { get; set; }
        public bool Status { get; set; } = true;


        // New property for profile picture path
        public string ProfilePicturePath { get; set; }
        
    }
}
