using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeakWise.Application.DTOs.Tokens;

namespace PeakWise.Application.Interfaces
{
    public interface ITokenStoreService
    {
        Task<string> CreateAccessTokenAsync(AppUser appUser);
        string GenerateRefreshToken();
        Task SaveRefreshTokenAsync(string userId, string refreshToken);
        Task InvalidateOldTokensAsync(string userId);
        Task<bool> IsValidAsync(string refreshToken);

        Task<TokenResponse> GenerateAndStoreTokensAsync(string userId, AppUser user);


    }
}
