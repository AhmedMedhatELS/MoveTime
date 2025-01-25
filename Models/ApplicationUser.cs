using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using Utility;

namespace Models
{
    public class ApplicationUser : IdentityUser
    {
        #region Properities
        public string? FullName { get; set; }
        public string? ProfileImage { get; set; }
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public AddWhich AddWhich { get; set; }
        #endregion

        #region Foreign Keys

        #endregion

        #region Relations
        public ICollection<Child> Children { get; set; } = [];
        #endregion
    }
}
