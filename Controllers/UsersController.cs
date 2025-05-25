using Microsoft.AspNetCore.Mvc;
using projectone.Services;
using projectone.Dtos;
using projectone.Models;
using Microsoft.AspNetCore.Authorization;

namespace projectone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _userService;
        private readonly UserPasswordService _userPasswordService;

        public UsersController(UsersService userService, UserPasswordService userPasswordService)
        {
            _userService = userService;
            _userPasswordService = userPasswordService;
        }

        // 📌 Kullanıcı Kaydı (Register)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UsersRegisterDto dto)
        {
            var user = new Users
            {
                Name = dto.Name,
                LastName = dto.LastName,
                UserName = dto.UserName,
                InsertTime = DateTime.UtcNow
            };

            await _userService.CreateAsync(user);

            // Şifre hashlenip ayrı koleksiyona kaydediliyor
            var passwordDto = new UsersPasswordDto
            {
                UserId = user.Id,
                Password = dto.Password
            };

            await _userPasswordService.CreateHashedPasswordAsync(passwordDto);

            return Ok("Kayıt başarılı");
        }

        // 📌 Tüm kullanıcıları getir
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UsersUpdateDto dto)
        {
            var existingUser = await _userService.GetByIdAsync(id);
            if (existingUser == null)
                return NotFound("Kullanıcı bulunamadı");

            existingUser.Name = dto.Name;
            existingUser.LastName = dto.LastName;
            existingUser.UserName = dto.UserName;
            existingUser.UpdateTime = DateTime.UtcNow;

            await _userService.UpdateAsync(id, existingUser);

            return Ok("Kullanıcı güncellendi");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı");

            user.DeleteTime = DateTime.UtcNow;
            await _userService.UpdateAsync(id, user);

            return Ok("Kullanıcı silindi (soft delete)");
        }
        [Authorize]
        [HttpGet("secret")]
        public IActionResult Secret()
        {
            var userId = User.FindFirst("userId")?.Value;
            return Ok($"Sadece giriş yapan kullanıcılar görebilir. Giriş yapan ID: {userId}");
        }


    }
}
