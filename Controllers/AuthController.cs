using Microsoft.AspNetCore.Mvc;
using projectone.Services;
using projectone.Dtos;

namespace projectonet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // 📌 LOGIN → Access + Refresh Token döner
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto.UserName, loginDto.Password);

            if (result == null)
                return Unauthorized("Kullanıcı adı veya şifre hatalı");

            return Ok(new
            {
                accessToken = result.Value.accessToken,
                refreshToken = result.Value.refreshToken
            });
        }

        // 📌 REFRESH TOKEN → Yeni Access + Refresh üretir
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto refreshDto)
        {
            var result = await _authService.RefreshTokenAsync(refreshDto.RefreshToken);

            if (result == null)
                return Unauthorized("Refresh token geçersiz veya süresi dolmuş");

            return Ok(new
            {
                accessToken = result.Value.accessToken,
                refreshToken = result.Value.refreshToken
            });
        }
    }
}
