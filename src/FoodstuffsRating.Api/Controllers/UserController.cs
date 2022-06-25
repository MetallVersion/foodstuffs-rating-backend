using System.Threading.Tasks;
using FoodstuffsRating.Dto;
using FoodstuffsRating.Services;
using FoodstuffsRating.Services.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodstuffsRating.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route($"{ApiPrefix.Url}/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService,
            ILogger<UserController> logger)
        {
            this._userService = userService;
            this._logger = logger;
        }

        [HttpGet]
        [Route("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = this.User.GetUserIdAsGuid();
            var profile = await this._userService.GetUserProfileAsync(userId);
            if (profile == null)
            {
                return this.Unauthorized();
            }

            return this.Ok(profile);
        }

        [HttpPut]
        [Route("profile")]
        public async Task<IActionResult> UpdateProfile(UserProfileUpdateRequest request)
        {
            var userId = this.User.GetUserIdAsGuid();

            var profile = await this._userService.UpdateUserProfileAsync(userId, request);

            return this.Ok(profile);
        }
    }
}
