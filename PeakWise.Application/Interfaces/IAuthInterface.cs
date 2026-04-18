using System.Security.Claims;
using PeakWise.Application.DTOs.Auth;
using PeakWise.Shared.Responses;

namespace PeakWise.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Response<RegisterResponse>> RegisterAsync(RegisterRequest request);
        Task<Response<LoginResponse>> LoginAsync(LoginRequest loginRequest);
        Task<Response<string>> LogoutAsync(ClaimsPrincipal userClaims);
    }
}