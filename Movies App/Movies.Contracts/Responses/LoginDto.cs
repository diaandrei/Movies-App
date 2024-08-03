namespace Movies.Contracts.Responses
{
    public class LoginDto
    {
        public string Token { get; set; }
        public bool IsAdmin { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
