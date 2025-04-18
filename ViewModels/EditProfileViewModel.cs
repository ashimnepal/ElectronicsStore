using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.ViewModels
{
    public class EditProfileViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }
        
        [Required]
        [Display(Name = "City")]
        public string City { get; set; }
        
        [Required]
        [Display(Name = "State")]
        public string State { get; set; }
        
        [Required]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
        
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }
} 