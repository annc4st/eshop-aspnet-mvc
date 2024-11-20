using System;
using System.ComponentModel.DataAnnotations;

namespace TaskAuthenticationAuthorization.Models
{

    public enum UserRole
{ Admin, Buyer }


public enum BuyerType
{
    None, Regular, Golden, Wholesale
}


    public class User
    {
        public int UserId { get; set; }
 
        [EmailAddress]
        public string Email { get; set; }
     
        public string PasswordHash { get; set; } // storing hashed password
        public string Salt { get; set; }
        // role permissions (Admin, buyer)
        public UserRole Role { get; set; }
        // Only relevant if the user is a Buyer
        public BuyerType? BuyerType { get; set; } = null;

         // Navigation property to Customer
        public Customer Customer { get; set; }
    }
}