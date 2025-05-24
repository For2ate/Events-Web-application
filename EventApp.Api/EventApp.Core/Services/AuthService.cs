using AutoMapper;
using EventApp.Core.Exceptions;
using EventApp.Core.Interfaces;
using EventApp.Core.Methods;
using EventApp.Data.Entities;
using EventApp.Data.Interfaces;
using EventApp.Models.UserDTO.Requests;
using EventApp.Models.UserDTO.Responses;
using Microsoft.Extensions.Logging;

namespace EventApp.Core.Services {
    
    public class AuthService : IAuthService {

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

                user.PasswordHash = Hasher.HashPassword(user.PasswordHash);
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

                if (Hasher.HashPassword(model.Password) != existingUserByEmail.PasswordHash) {
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
