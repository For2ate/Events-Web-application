using AutoMapper;
using EventApp.Api.Core.Interfaces;
using EventApp.Api.Core.Methods;
using EventApp.Api.Exceptions;
using EventApp.Data.Entities;
using EventApp.Data.Interfaces;
using EventApp.Models.UserDTO.Requests;
using EventApp.Models.UserDTO.Responses;

namespace EventApp.Api.Core.Services {
    
    public class AuthService : IAuthService{

        private readonly IUserRepository _userRepository;
        private readonly IMapper _userMapper;

        private readonly ILogger<AuthService> _logger;
    
        public AuthService( 
                IUserRepository userRepository, IMapper userMapper, ILogger<AuthService> logger 
            ) {

            _userRepository = userRepository;
            _userMapper = userMapper;
            _logger = logger;

        }

        public async Task<UserFullResponseModel> Register(UserRegisterRequestModel model) {

            try {

                var existingUserByEmail = await _userRepository.GetUserByEmailAsync(model.Email);
                if (existingUserByEmail != null) {
                    throw new DuplicateResourceException("User", model.Email);
                }

                var user = _userMapper.Map<UserEntity>(model);

                user.Password = Hasher.HashPassword(user.Password);
                user.BirthdayDate = model.BirthdayDate.ToUniversalTime();

                await _userRepository.AddAsync(user);

                return _userMapper.Map<UserFullResponseModel>(user);

            } catch (Exception ex) {

                _logger.LogError(ex, "Error creating user {model}", model);

                throw;

            }

        }

        public async Task<UserFullResponseModel> Login(UserLoginRequestModel model) {

            try {

                var existingUserByEmail = await _userRepository.GetUserByEmailAsync(model.Email);
                if (existingUserByEmail == null) {
                    throw new InvalidCredentialsException();
                }

                if (Hasher.HashPassword(model.Password) != existingUserByEmail.Password) {
                    throw new InvalidCredentialsException();
                }

                return _userMapper.Map<UserFullResponseModel>(existingUserByEmail);

            } catch (Exception ex) {

                _logger.LogError(ex, "Error login user {model}", model);

                throw;

            }

        }


    }

}
