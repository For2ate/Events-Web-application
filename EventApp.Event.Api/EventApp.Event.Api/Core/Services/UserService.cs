using AutoMapper;
using EventApp.Api.Core.Interfaces;
using EventApp.Data.Interfaces;
using EventApp.Models.UserDTO.Responses;

namespace EventApp.Api.Core.Services {
    
    public class UserService : IUserService {

        private readonly IUserRepository _userRepository;
        private readonly IMapper _userMapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IMapper userMapper, ILogger<UserService> logger) {

            _userMapper = userMapper;
            _userRepository = userRepository;
            _logger = logger;

        }

        public async Task<UserFullResponseModel> GetUserByIdAsync(Guid userId) {

            try {

                var user = await _userRepository.GetByIdAsync(userId);

                return _userMapper.Map<UserFullResponseModel>(user);

            } catch (Exception ex) {

                _logger.LogError(ex, "Error get user with {userId}", userId);
                throw;

            }

        }

    }

}
