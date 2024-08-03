using Movies.Contracts.Requests;

namespace Movies.Api
{
    public static class ApiEndpoints
    {
        private const string ApiBase = "api";

        public static class Movies
        {
            private const string Base = $"{ApiBase}/movies";

            public const string Create = Base;
            public const string Get = $"{Base}/{{id:guid}}";
            public const string GetAll = Base;
            public const string GetAdminMovies = $"{Base}/admin";
            public const string GetTopFavorites = $"{Base}/favorites";
            public const string Update = $"{Base}/{{id:guid}}";
            public const string Delete = $"{Base}/{{id:guid}}";

            public const string Rate = $"{Base}/{{id:guid}}/ratings";
            public const string DeleteRating = $"{Base}/{{id:guid}}/ratings";

            public const string Search = $"{Base}/search";
            public const string AddToWatchlist = $"{Base}/watchlist";
            public const string RemoveFromWatchlist = $"{Base}/watchlist/{{id:guid}}";
            public const string GetWatchlist = $"{Base}/watchlist";
            public const string GetTopMovies = $"{Base}/top";
            public const string CreateTopMovies = $"{Base}/top";
            public const string GetRecentMovies = $"{Base}/recent";
        }

        public static class Ratings
        {
            private const string Base = $"{ApiBase}/movies";
            public const string GetUserRatings = $"{Base}/me";
        }
    }
}