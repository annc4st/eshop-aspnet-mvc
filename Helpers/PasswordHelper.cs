using System;
using System.Security.Cryptography;
using System.Text;

namespace TaskAuthenticationAuthorization.Helpers
{

    public class PasswordHelper
    {
        private const int SaltSize = 16; // Salt size in bytes
        private const int KeySize = 32;  // Hash size in bytes
        private const int Iterations = 10000; // Number of iterations
     
     public static (string Hash, string Salt) HashPassword(string password)
     {
         // Generate a salt using RandomNumberGenerator
        byte[] saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
        string salt = Convert.ToBase64String(saltBytes);

        // Derive the hash

        // Create the hash with the salt
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
        {
            byte[] hashBytes = pbkdf2.GetBytes(KeySize);
            string hash = Convert.ToBase64String(hashBytes);
            return (hash, salt);
        }
    }

    public static bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt)
    {
         // Convert the stored salt from Base64 string to byte array
        byte[] saltBytes = Convert.FromBase64String(storedSalt);

        // Derive the hash using the entered password and the stored salt
        using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, saltBytes, Iterations, HashAlgorithmName.SHA256))
        {
            // Generate the hash based on the entered password
            byte[] hashBytes = pbkdf2.GetBytes(KeySize);
             // Convert the generated hash to Base64 string to compare
            string enteredHash = Convert.ToBase64String(hashBytes);

// Compare the generated hash with the stored hash
            return storedHash == enteredHash;
        }
    }
}
}