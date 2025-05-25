using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace projectone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("public")]
        [AllowAnonymous]
        public ActionResult GetPublic()
        {
            return Ok(new { Message = "Bu public endpoint - token gerekmez" });
        }

        [HttpGet("protected")]
        [Authorize]
        public ActionResult GetProtected()
        {
            var userId = User.FindFirst("userId")?.Value;
            var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            return Ok(new 
            { 
                Message = "Bu protected endpoint - token gerekir",
                IsAuthenticated = User.Identity?.IsAuthenticated,
                UserId = userId,
                NameIdentifier = nameIdentifier,
                AllClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            });
        }

        [HttpGet("debug")]
        [Authorize]
        public ActionResult Debug()
        {
            return Ok(new
            {
                IsAuthenticated = User.Identity?.IsAuthenticated,
                AuthenticationType = User.Identity?.AuthenticationType,
                Name = User.Identity?.Name,
                Claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList(),
                Headers = Request.Headers.Select(h => new { Key = h.Key, Value = h.Value.ToString() }).ToList()
            });
        }
    }
}