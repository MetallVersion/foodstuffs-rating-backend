using System;
using System.Threading.Tasks;
using AutoMapper;
using FoodstuffsRating.Data.Dal;
using FoodstuffsRating.Data.Models;
using FoodstuffsRating.Dto;
using FoodstuffsRating.Models.Exceptions;

namespace FoodstuffsRating.Services
{
    public interface IUserService
    {
        Task<UserProfile?> GetUserProfileAsync(Guid userId);
        Task<UserProfile> UpdateUserProfileAsync(Guid userId,
            UserProfileUpdateRequest request);
    }

    public class UserService : IUserService
    {
        private readonly IBackendRepository<User> _userRepository;
        private readonly IMapper _mapper;

        public UserService(IBackendRepository<User> userRepository,
            IMapper mapper)
        {
            this._userRepository = userRepository;
            this._mapper = mapper;
        }

        public async Task<UserProfile?> GetUserProfileAsync(Guid userId)
        {
            var user = await this._userRepository.GetAsync(x => x.Id == userId);
            if (user == null)
            {
                return null;
            }

            var profile = this._mapper.Map<UserProfile>(user);

            return profile;
        }

        public async Task<UserProfile> UpdateUserProfileAsync(Guid userId, UserProfileUpdateRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var user = await this._userRepository.GetAsync(x => x.Id == userId, asNoTracking: false);
            if (user == null) throw new ResourceNotFoundException("User", userId.ToString());

            this._mapper.Map(request, user);
            await this._userRepository.UpdateAsync(user);

            return this._mapper.Map<UserProfile>(user);
        }
    }
}
