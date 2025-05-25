using projectone.Models;
using projectone.Dtos;

namespace projectone.Services
{
    public class AuthService
    {
        private readonly UsersService _userService;
        private readonly UserPasswordService _userPasswordService;
        private readonly JwtService _jwtService;
        private readonly RefreshTokenService _refreshTokenService;

        public AuthService(
            UsersService userService,
            UserPasswordService userPasswordService,
            JwtService jwtService,
            RefreshTokenService refreshTokenService)
        {
            _userService = userService;
            _userPasswordService = userPasswordService;
            _jwtService = jwtService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<(string accessToken, string refreshToken)?> LoginAsync(string username, string password)
        {
            var user = (await _userService.GetAllAsync()).FirstOrDefault(u => u.UserName == username);
            if (user == null) return null;

            var isPasswordCorrect = await _userPasswordService.CheckPasswordAsync(user.Id, password);
            if (!isPasswordCorrect) return null;

            var accessToken = _jwtService.GenerateAccessToken(user.Id);
            var refreshToken = _jwtService.GenerateRefreshToken();

            await _refreshTokenService.CreateRefreshTokenAsync(user.Id, refreshToken);

            return (accessToken, refreshToken);
        }

        public async Task<(string accessToken, string refreshToken)?> RefreshTokenAsync(string oldRefreshToken)
        {
            var tokenData = await _refreshTokenService.GetByTokenAsync(oldRefreshToken);

            if (tokenData == null || !tokenData.IsActive)
                return null;

            // önceki refresh token’ı geçersiz kıl
            await _refreshTokenService.RevokeTokenAsync(oldRefreshToken);

            // yeni token’lar üret
            var newAccessToken = _jwtService.GenerateAccessToken(tokenData.UserId);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            await _refreshTokenService.CreateRefreshTokenAsync(tokenData.UserId, newRefreshToken);

            return (newAccessToken, newRefreshToken);
        }
    }
}
