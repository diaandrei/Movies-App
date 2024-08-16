using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Application;
using Movies.Application.Models;
using Movies.Application.Services;

namespace Movies.Api.Controllers
{
    [ApiController]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingsController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        [Authorize]
        [HttpGet(ApiEndpoints.Movies.Rate)]
        public async Task<ResponseModel<string>> RateMovie(
            Guid ratingId, Guid movieId, decimal rating, CancellationToken token)
        {
            var response = new ResponseModel<string>
            {
                Title = "Something went wrong.",
                Success = false
            };
            var userId = HttpContext.GetUserId().ToString();
            var isAdmin = HttpContext.CheckAdmin();
            var mapReq = ContractMapping.MapToRatingRequest(ratingId, movieId, rating, userId);
            var result = await _ratingService.RateMovieAsync(mapReq, isAdmin, token: token);

            response.Success = result.Success;
            response.Title = result.Title;
            response.Content = result.Content;


            return response;
        }

        [Authorize]
        [HttpDelete(ApiEndpoints.Movies.DeleteRating)]
        public async Task<ResponseModel<string>> DeleteRating(Guid id, CancellationToken token)
        {
            var response = new ResponseModel<string>
            {
                Title = "Something went wrong.",
                Success = false
            };
            var userId = HttpContext.GetUserId().ToString();
            var result = await _ratingService.DeleteRatingAsync(id, userId, token);
            response.Success = result.Success;
            response.Title = result.Title;
            response.Content = result.Content;
            return response;
        }

        [Authorize]
        [HttpGet(ApiEndpoints.Ratings.GetUserRatings)]
        public async Task<ResponseModel<IEnumerable<MovieRating>>> GetUserRatings(CancellationToken token)
        {
            var response = new ResponseModel<IEnumerable<MovieRating>>
            {
                Success = false,
                Title = "Something went wrong."
            };
            var userId = HttpContext.GetUserId().ToString();
            var ratings = await _ratingService.GetRatingsForUserAsync(userId, token);
            var ratingResponse = ratings.Content.MapToResponse();

            response.Title = ratings.Title;
            response.Success = ratings.Success;
            response.Content = ratings.Content;

            return response;
        }
    }
}
