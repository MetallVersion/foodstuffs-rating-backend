using AutoMapper;
using FoodstuffsRating.Api.Dto;
using FoodstuffsRating.Api.Helpers;
using FoodstuffsRating.Api.Services;
using FoodstuffsRating.Data.Dal;
using FoodstuffsRating.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodstuffsRating.Api.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : ControllerBase
    {
        private readonly IUserManager _userManager;
        private readonly IBackendRepository<User> _userRepository;
        private readonly IMapper _mapper;

        public UserController(IUserManager userManager,
            IBackendRepository<User> userRepository,
            IMapper mapper)
        {
            this._userManager = userManager;
            this._userRepository = userRepository;
            this._mapper = mapper;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterUserRequest request)
        {
            var createdUser = await this._userManager.RegisterAsync(request);
            
            return this.Ok();
        }

        [Authorize]
        [HttpGet]
        [Route("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = this.User.Claims.GetUserId();
            var user = await this._userRepository.GetAsync(x => x.Id == userId);
            if (user == null)
            {
                return this.NotFound();
            }

            var profile = this._mapper.Map<UserProfile>(user);

            return this.Ok(profile);
        }
    }
}
