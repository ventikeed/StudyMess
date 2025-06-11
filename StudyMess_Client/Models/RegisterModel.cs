using Microsoft.AspNetCore.Http;

namespace StudyMess_Client.Models
{
    public class RegisterModel
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public int GroupId { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}