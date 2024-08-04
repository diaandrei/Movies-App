using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Application;
using Movies.Application.Models;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers
{
    [Authorize]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;
        private readonly IUserWatchlistService _userWatchlistService;
        private readonly IMapper _mapper;

        public MoviesController(IMovieService movieService, IUserWatchlistService userWatchlistService, IMapper mapper)
        {
            _movieService = movieService;
            _userWatchlistService = userWatchlistService;
            _mapper = mapper;
        }

        [HttpPost(ApiEndpoints.Movies.Create)]
        public async Task<ResponseModel<string>> Create([FromBody] CreateMovieRequest request, CancellationToken token)
        {

            var response = new ResponseModel<string>
            {
                Title = "Something went wrong.",
                Success = false
            };

            request.Title = request.Title?.ToUpper();
            var movie = request.MapToMovie();

            var result = await _movieService.CreateAsync(movie, token);
            response.Success = result.Success;
            response.Title = result.Title;

            return response;

        }
        [AllowAnonymous]
        [HttpGet(ApiEndpoints.Movies.Get)]
        public async Task<ResponseModel<MovieResponse>> Get(Guid id, CancellationToken token)
        {
            var response = new ResponseModel<MovieResponse>
            {
                Success = false,
                Title = "Something went wrong."
            };

            try
            {
                bool isAdmin = false;
                string userId = null;
                bool isAuthenticated = HttpContext.IsUserAuthenticated(out var authenticatedUserId);

                if (isAuthenticated)
                {
                    userId = authenticatedUserId.ToString();
                    isAdmin = HttpContext.CheckAdmin();
                }

                var movie = await _movieService.GetByIdAsync(id, isAdmin, userId, token);
                var movieDetail = movie.MapToResponse(userId);

                if (movieDetail != null)
                {
                    response.Content = movieDetail;
                    response.Success = true;
                    response.Title = "Title detail.";
                }
                else
                {
                    response.Title = "Title not found.";
                }
            }
            catch (Exception ex)
            {
                response.Title = ex.Message;
            }

            return response;
        }

        [AllowAnonymous]
        [HttpGet(ApiEndpoints.Movies.GetAll)]
        public async Task<ResponseModel<MoviesResponse>> GetAll([FromQuery] GetAllMoviesRequest request, CancellationToken token)
        {
            var response = new ResponseModel<MoviesResponse>
            {
                Success = false,
                Title = "Something went wrong."
            };

            try
            {
                var options = request.MapToOptions();
                var isUserAuthenticated = HttpContext.IsUserAuthenticated(out var userId);
                var movies = isUserAuthenticated
                    ? await _movieService.GetAllAsync(options, false, false, userId.ToString(), token)
                    : await _movieService.GetAllAsync(options, false, false, null, token);

                if (movies != null)
                {
                    var movieCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, token);
                    var moviesResponse = isUserAuthenticated ? movies.MapToResponse(request.Page, request.PageSize, movieCount, userId.ToString())
                        : movies.MapToResponse(request.Page, request.PageSize, movieCount);

                    response.Success = true;
                    response.Title = "Title list";
                    response.Content = moviesResponse;
                }
                else
                {
                    response.Title = "No titles found.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = ex.Message;
            }

            return response;
        }



        [HttpGet(ApiEndpoints.Movies.GetAdminMovies)]
        public async Task<ResponseModel<MoviesResponse>> GetAllMoviesAdmin([FromQuery] GetAllMoviesRequest request, CancellationToken token)
        {
            ResponseModel<MoviesResponse> response = new ResponseModel<MoviesResponse>
            {
                Success = false,
                Title = "Something went wrong.",
            };

            try
            {
                var userId = HttpContext.GetUserId().ToString();
                var isAdmin = HttpContext.CheckAdmin();
                var options = request.MapToOptions();
                var movies = await _movieService.GetAllAsync(options, false, isAdmin, userId, token);

                if (movies != null)
                {
                    var movieCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, token);
                    var moviesResponse = movies.MapToResponse(request.Page, request.PageSize, movieCount);
                    response.Success = true;
                    response.Title = "Title list";
                    response.Content = moviesResponse;
                }
                else
                {
                    response.Title = "No titles found.";
                }

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = ex.Message;
            }

            return response;
        }

        [AllowAnonymous]
        [HttpGet(ApiEndpoints.Movies.GetTopFavorites)]
        public async Task<ResponseModel<MoviesResponse>> TopFavourite([FromQuery] GetAllMoviesRequest request, CancellationToken token)
        {
            ResponseModel<MoviesResponse> response = new ResponseModel<MoviesResponse>
            {
                Success = false,
                Title = "Something went wrong.",
            };

            try
            {
                var isUserAuthenticated = HttpContext.IsUserAuthenticated(out var userId);
                bool isAdmin = false;

                if (isUserAuthenticated)
                {
                    isAdmin = HttpContext.CheckAdmin();
                }
                var options = request.MapToOptions();
                var result = isUserAuthenticated
                    ? await _movieService.GetAllAsync(options, true, isAdmin, userId.ToString(), token)
                    : await _movieService.GetAllAsync(options, true, false, null, token);

                if (result != null)
                {
                    var movieCount = await _movieService.GetCountAsync(options.Title, options.YearOfRelease, token);
                    var moviesResponse = isUserAuthenticated ? result.MapToResponse(request.Page, request.PageSize, movieCount, userId.ToString())
                        : result.MapToResponse(request.Page, request.PageSize, movieCount);
                    response.Success = true;
                    response.Title = "Titles list";
                    response.Content = moviesResponse;
                }
                else
                {
                    response.Title = "No titles found.";
                }

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = ex.Message;
            }

            return response;
        }

        [HttpPut(ApiEndpoints.Movies.Update)]
        public async Task<ResponseModel<MovieResponse>> Update(UpdateMovieRequest request, CancellationToken token)
        {
            var response = new ResponseModel<MovieResponse>
            {
                Title = "Something went wrong.",
                Success = false
            };
            try
            {
                var userId = HttpContext.GetUserId();
                var movie = request.MapToMovie(request.Id);
                var updatedMovie = await _movieService.UpdateAsync(movie, userId, token);

                if (updatedMovie != null)
                {
                    var movieDetail = updatedMovie.MapToResponse();
                    response.Title = "The title has been successfully updated.";
                    response.Success = true;
                    response.Content = movieDetail;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = ex.Message;
            }


            return response;
        }

        [HttpDelete(ApiEndpoints.Movies.Delete)]
        public async Task<ResponseModel<string>> Delete(Guid id,
            CancellationToken token)
        {
            var response = new ResponseModel<string>
            {
                Title = "Something went wrong.",
                Success = false
            };
            try
            {
                var deleted = await _movieService.DeleteByIdAsync(id, token);
                if (deleted)
                {
                    response.Success = true;
                    response.Title = "Title deleted successfully.";
                }

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = ex.Message;
            }
            return response;
        }
        [AllowAnonymous]
        [HttpGet(ApiEndpoints.Movies.Search)]
        public async Task<ResponseModel<IEnumerable<Movie>>> SearchMovie(string textToSearchMovie)
        {
            var response = new ResponseModel<IEnumerable<Movie>>
            {
                Title = "Something went wrong.",
                Success = false
            };

            try
            {
                var moviesList = await _movieService.GetSearchedMoviesAsync(textToSearchMovie);
                if (moviesList != null)
                {
                    response.Content = moviesList;
                    response.Title = "Successfully fetched titles.";
                    response.Success = true;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = ex.Message;
            }


            return response;
        }

        [HttpGet(ApiEndpoints.Movies.AddToWatchlist)]
        public async Task<ResponseModel<string>> AddMovieWatchList(string movieId)
        {
            var response = new ResponseModel<string>
            {
                Title = "Something went wrong.",
                Success = false
            };
            try
            {
                if (!Guid.TryParse(movieId, out var guidMovieId))
                {
                    response.Title = "Invalid movie ID.";
                }

                var userId = HttpContext.GetUserId();

                var userWatchlistModel = ContractMapping.MapToWatchlist(guidMovieId, userId);
                var result = await _userWatchlistService.AddMovieInWatchlistAsync(userWatchlistModel);
                response.Title = result.Title;
                response.Success = result.Success;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = ex.Message;
            }

            return response;
        }



        [HttpDelete(ApiEndpoints.Movies.RemoveFromWatchlist)]
        public async Task<ResponseModel<string>> DeleteMovieFromWatchList(Guid userWatchlistId)
        {
            var response = new ResponseModel<string>
            {
                Title = "Something went wrong.",
                Success = false
            };

            try
            {
                var userId = HttpContext.GetUserId();
                var result = await _userWatchlistService.DeleteByIdAsync(userWatchlistId);

                if (result != null)
                {
                    response.Success = true;
                    response.Title = "Title successfully removed from your watchlist.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = ex.Message;
            }

            return response;
        }
        [HttpGet(ApiEndpoints.Movies.GetWatchlist)]
        public async Task<ResponseModel<MoviesResponseDto>> AllMovieFromWatchList()
        {
            var response = new ResponseModel<MoviesResponseDto>
            {
                Title = "Something went wrong.",
                Success = false
            };

            try
            {
                var userId = HttpContext.GetUserId();
                var isAdmin = HttpContext.CheckAdmin();
                var allWatchlistData =
                await _userWatchlistService.GetAllAsync(userId.ToString(), isAdmin);
                var result = allWatchlistData.MapToResponse(1, 10, 10);

                if (allWatchlistData != null)
                {
                    response.Title = "User Watchlist";
                    response.Success = true;
                    response.Content = result;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = ex.Message;
            }

            return response;
        }

        [AllowAnonymous]
        [HttpGet(ApiEndpoints.Movies.GetTopMovies)]

        public async Task<ResponseModel<MoviesResponse>> TopMoviesList(CancellationToken token)
        {
            ResponseModel<MoviesResponse> response = new ResponseModel<MoviesResponse>
            {
                Success = false,
                Title = "Something went wrong.",
            };

            try
            {
                var isUserAuthenticated = HttpContext.IsUserAuthenticated(out var userId);
                bool isAdmin = false;

                if (isUserAuthenticated)
                {
                    isAdmin = HttpContext.CheckAdmin();
                }

                var result = isUserAuthenticated
                    ? await _movieService.GetTopMovieAsync(isAdmin, userId.ToString(), token)
                    : await _movieService.GetTopMovieAsync(false, null, token);

                if (result.Success)
                {
                    var movieCount = await _movieService.GetCountAsync("", "", token);
                    var moviesResponse = isUserAuthenticated ? result.Content.MapToResponse(1, 10, movieCount, userId.ToString())
                        : result.Content.MapToResponse(1, 10, movieCount);
                    response.Success = true;
                    response.Title = "Titles list";
                    response.Content = moviesResponse;
                }
                else
                {
                    response.Title = "Titles not found.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = ex.Message;
            }

            return response;
        }


        [HttpPost(ApiEndpoints.Movies.CreateTopMovies)]
        public async Task<ResponseModel<string>> CreateTopMovies(List<Guid> movieIds, CancellationToken token)
        {
            ResponseModel<string> response = new ResponseModel<string>
            {
                Success = false,
                Title = "Something went wrong.",
            };

            try
            {
                var userId = HttpContext.GetUserId().ToString();
                var topMovieModel = ContractMapping.MapTopMovieRequest(movieIds, userId);

                var result = await _movieService.CreateTopMovieAsync(topMovieModel, token);

                if (result.Success)
                {
                    response.Success = result.Success;
                    response.Title = result.Title;
                }
                else
                {
                    response.Title = "Titles not found.";
                }

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = ex.Message;
            }

            return response;
        }

        [AllowAnonymous]
        [HttpGet(ApiEndpoints.Movies.GetRecentMovies)]
        public async Task<ResponseModel<MoviesResponse>> MostRecentMoviesList(CancellationToken token)
        {
            ResponseModel<MoviesResponse> response = new ResponseModel<MoviesResponse>
            {
                Success = false,
                Title = "Something went wrong.",
            };

            try
            {
                bool isAdmin = false;
                string userId = null;
                bool isAuthenticated = HttpContext.IsUserAuthenticated(out var authenticatedUserId);

                if (isAuthenticated)
                {
                    userId = authenticatedUserId.ToString();
                    isAdmin = HttpContext.CheckAdmin();
                }
                var movies = await _movieService.GetMostRecentMovieAsync(isAdmin, userId, token);

                if (movies != null)
                {
                    var movieCount = await _movieService.GetCountAsync("", "", token);
                    var moviesResponse = movies.MapToResponse(1, 10, movieCount);
                    response.Success = true;
                    response.Title = "Most recent Titles list";
                    response.Content = moviesResponse;
                }
                else
                {
                    response.Title = "Titles not found.";
                }

            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = ex.Message;
            }

            return response;
        }
    }
}