using System.ComponentModel.DataAnnotations;
using TaskAuthenticationAuthorization.Models;


namespace TaskAuthenticationAuthorization.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage ="Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage ="Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }  

        [Required(ErrorMessage ="Confirm Password is required")]  
        [Compare("Password", ErrorMessage ="Passwords do not match")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set;}

        [Required]
        public string FirstName { get; set; }
          [Required]
        public string LastName { get; set;}
          [Required]
        public string Address { get; set; }
        public Discount? Discount { get; set; }
        
        
    }
}