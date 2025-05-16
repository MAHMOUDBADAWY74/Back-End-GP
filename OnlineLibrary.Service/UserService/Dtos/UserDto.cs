namespace OnlineLibrary.Service.UserService.Dtos
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; } 
        public string? UserName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; } 
        public int? Age { get; set; }
        public string Token { get; set; } 
    }
}