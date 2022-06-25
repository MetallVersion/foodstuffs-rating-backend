using System.Threading.Tasks;
using AutoMapper;
using FoodstuffsRating.Dto;
using FoodstuffsRating.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FoodstuffsRating.Api.Controllers
{
    [ApiController]
    [Route($"{ApiPrefix.Url}/user")]
    public class UserRegistrationController : ControllerBase
    {
        private readonly IUserRegistrationService _userRegistrationService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserRegistrationController> _logger;

        public UserRegistrationController(IUserRegistrationService userRegistrationService,
            IMapper mapper,
            ILogger<UserRegistrationController> logger)
        {
            this._userRegistrationService = userRegistrationService;
            this._mapper = mapper;
            this._logger = logger;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(UserRegistrationRequest registrationRequest)
        {
            var createdUser = await this._userRegistrationService.RegisterAsync(registrationRequest);

            var profile = this._mapper.Map<UserProfile>(createdUser);

            // TODO: handle such routes without code duplications
            return this.Created($"{ApiPrefix.Url}/user/profile", profile);
        }
    }
}
