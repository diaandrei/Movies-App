using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Movies.Application.Models;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;
using Movies.Identity;

namespace Movies.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
        }

        [HttpPost(ApiEndpoints.Movies.Register)]
        public async Task<ResponseModel<string>> Register([FromBody] RegisterRequest model)
        {
            ResponseModel<string> response = new ResponseModel<string>();
            response.Title = "Something went wrong. Please try again later";

            try
            {
                if (ModelState.IsValid)
                {
                    var user = new ApplicationUser { FirstName = model.FirstName, LastName = model.LastName, UserName = model.Email, Email = model.Email };
                    user.IsTrustedMember = false;
                    user.IsAdmin = false;
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        var roleExists = await _roleManager.RoleExistsAsync("User");

                        if (!roleExists)
                        {
                            var roleResult = await _roleManager.CreateAsync(new IdentityRole("User"));

                            if (!roleResult.Succeeded)
                            {
                                response.Title = "Failed to create user role. Please try again later.";
                                return response;
                            }
                        }

                        // Assign the role to the user
                        var addToRoleResult = await _userManager.AddToRoleAsync(user, "User");

                        if (addToRoleResult.Succeeded)
                        {
                            response.Title = "Account created successfully! Thank you for joining us.";
                            response.Success = true;
                            return response;
                        }
                        else
                        {
                            response.Title = "Failed to assign user role. Please try again later.";
                            return response;
                        }
                    }
                    else
                    {
                        response.Title = string.Join(", ", result.Errors.Select(e => e.Description));
                        return response;
                    }
                }
                else
                {
                    response.Title = "The provided data is invalid. Please review and try again.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Title = ex.Message;
            }

            return response;
        }

        [HttpPost(ApiEndpoints.Movies.Login)]
        public async Task<ResponseModel<LoginDto>> Login([FromBody] LoginRequest model)
        {
            ResponseModel<LoginDto> response = new ResponseModel<LoginDto>();
            response.Title = "Oops! Something went wrong. Please retry in a moment.";
            response.Success = false;

            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);

                        if (result.Succeeded)
                        {
                            // Get the roles of the user
                            var roles = await _userManager.GetRolesAsync(user);
                            var role = roles.FirstOrDefault();
                            var roleId = string.Empty;

                            if (!string.IsNullOrEmpty(role))
                            {
                                var roleObj = await _roleManager.FindByNameAsync(role);
                                roleId = roleObj?.Id;
                            }

                            TokenGenerationRequest jwtReq = new TokenGenerationRequest
                            {
                                UserId = user.Id,
                                Email = model.Email,
                                RoleId = roleId,
                                IsTrustedMember = user.IsTrustedMember,
                                IsAdmin = user.IsAdmin,
                            };

                            var token = TokenGenerator.GenerateToken(jwtReq);
                            LoginDto login = new LoginDto
                            {
                                Token = token,
                                IsAdmin = user.IsAdmin,
                                Name = user.FirstName + " " + user.LastName,
                                Email = user.Email
                            };
                            response.Content = login;
                            response.Success = true;
                            response.Title = "Successfully logged in! Welcome back.";
                        }
                        else
                        {
                            response.Title = "Invalid email or password. Please try again.";
                        }
                    }
                    else
                    {
                        response.Title = "User not found. Please check your details and try again.";
                    }
                }
                else
                {
                    response.Title = "The input data is invalid. Please review the errors and try again";
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
