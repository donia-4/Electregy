using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PeakWise.Application.DTOs.Auth;
using PeakWise.Application.Interfaces;
using PeakWise.Domain.Entities;
using PeakWise.Shared.Responses;

namespace PeakWise.Application.Features
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;
        private readonly ResponseHandler _responseHandler;
        private readonly ITokenStoreService _tokenStoreService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<AppUser> userManager,
            AppDbContext context,
            ResponseHandler responseHandler,
            ITokenStoreService tokenStoreService,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _context = context;
            _responseHandler = responseHandler;
            _tokenStoreService = tokenStoreService;
            _logger = logger;
        }

        public async Task<Response<RegisterResponse>> RegisterAsync(RegisterRequest request)
        {
            if (request == null)
            {
                return _responseHandler.BadRequest<RegisterResponse>("Invalid request.");
            }

            _logger.LogInformation("RegisterAsync started for Email: {Email}", request.Email);

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed: Email is already registered: {Email}", request.Email);
                return _responseHandler.BadRequest<RegisterResponse>("Email is already registered.");
            }

            try
            {
                var user = new AppUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.FullName,
                    EmailConfirmed = true // <-- Verified by default
                };

                var createUserResult = await _userManager.CreateAsync(user, request.Password);
                if (!createUserResult.Succeeded)
                {
                    var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("User creation failed for Email: {Email}. Errors: {Errors}", request.Email, errors);
                    return _responseHandler.BadRequest<RegisterResponse>(errors);
                }

                await _userManager.AddToRoleAsync(user, "Consumer");

                var tokens = await _tokenStoreService.GenerateAndStoreTokensAsync(user.Id, user);

                var response = new RegisterResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    IsEmailConfirmed = true, // <-- Return true in response
                    AccessToken = tokens?.AccessToken,
                    RefreshToken = tokens?.RefreshToken
                };

                _logger.LogInformation("User registered successfully: {Email}", request.Email);
                return _responseHandler.Created(response, "User registered successfully and is ready to use.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during RegisterAsync for Email: {Email}", request?.Email);
                return _responseHandler.InternalServerError<RegisterResponse>("An error occurred during registration.");
            }
        }

        public async Task<Response<LoginResponse>> LoginAsync(LoginRequest loginRequest)
        {
            if (loginRequest == null)
                return _responseHandler.BadRequest<LoginResponse>("Invalid request.");

            try
            {
                var user = await _userManager.FindByEmailAsync(loginRequest.Email);
                if (user == null)
                    return _responseHandler.NotFound<LoginResponse>("User not found.");

                if (!await _userManager.CheckPasswordAsync(user, loginRequest.Password))
                    return _responseHandler.BadRequest<LoginResponse>("Incorrect email or password.");

                // This check will pass now because EmailConfirmed is true by default
                if (!user.EmailConfirmed)
                    return _responseHandler.BadRequest<LoginResponse>("Email is not verified.");

                var roles = await _userManager.GetRolesAsync(user);
                var tokens = await _tokenStoreService.GenerateAndStoreTokensAsync(user.Id, user);

                var response = new LoginResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "USER",
                    IsEmailConfirmed = user.EmailConfirmed,
                    AccessToken = tokens?.AccessToken,
                    RefreshToken = tokens?.RefreshToken,
                    DisplayName = user.UserName
                };

                return _responseHandler.Success(response, "Login successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during LoginAsync for Email: {Email}", loginRequest?.Email);
                return _responseHandler.InternalServerError<LoginResponse>("An error occurred during login.");
            }
        }

        public async Task<Response<string>> LogoutAsync(ClaimsPrincipal userClaims)
        {
            try
            {
                var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return _responseHandler.Unauthorized<string>("User not authenticated");
                }

                await _tokenStoreService.InvalidateOldTokensAsync(userId);
                return _responseHandler.Success<string>(null, "Logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during logout.");
                return _responseHandler.InternalServerError<string>($"An error occurred during logout: {ex.Message}");
            }
        }
    }
}