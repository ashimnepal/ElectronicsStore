using System.ComponentModel.DataAnnotations;

namespace ElectronicsStore.ViewModels
{
    public class ProfileViewModel
    {
        public string UserId { get; set; }
        
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Username")]
        public string UserName { get; set; }
        
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        
        [Display(Name = "Address")]
        public string Address { get; set; }
        
        [Display(Name = "City")]
        public string City { get; set; }
        
        [Display(Name = "State")]
        public string State { get; set; }
        
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
        
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        
        [Display(Name = "Roles")]
        public IList<string> Roles { get; set; }
        
        // Full name property
        public string FullName => $"{FirstName} {LastName}";
    }
} 