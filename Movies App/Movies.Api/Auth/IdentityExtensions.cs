namespace Movies.Api.Auth
{
    public static class IdentityExtensions
    {
        public static bool HasAdminPrivileges(this HttpContext context)
        {
            var isAdminClaim = context.User.Claims.SingleOrDefault(x => x.Type == "isAdmin");
            if (isAdminClaim != null && isAdminClaim.Value == "True")
            {
                return true;
            }
            return false;
        }


        public static Guid GetUserId(this HttpContext context)
        {
            var userIdClaim = context.User.Claims.SingleOrDefault(x => x.Type == "userId");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new InvalidOperationException("User ID is not valid or not found.");
            }

            return userId;
        }

        public static bool TryGetAuthenticatedUserId(this HttpContext context, out Guid? userId)
        {
            userId = null;

            if (context.User.Identity.IsAuthenticated)
            {
                var userIdClaim = context.User.Claims.SingleOrDefault(x => x.Type == "userId");
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedUserId))
                {
                    userId = parsedUserId;
                    return true;
                }
            }

            return false;
        }
    }
}
