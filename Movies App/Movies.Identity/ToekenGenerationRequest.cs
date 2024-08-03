namespace Movies.Identity
{
    public class TokenGenerationRequest
    {
        public string UserId { get; set; }

        public string Email { get; set; }
        public string RoleId { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsTrustedMember { get; set; }

        public Dictionary<string, object> CustomClaims { get; set; } = new();
    }
}