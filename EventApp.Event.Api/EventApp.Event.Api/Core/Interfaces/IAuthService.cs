using EventApp.Models.UserDTO.Requests;
using EventApp.Models.UserDTO.Responses;

namespace EventApp.Api.Core.Interfaces {
    
    public interface IAuthService {

        Task<UserFullResponseModel> Register(UserRegisterRequestModel model);

        Task<UserFullResponseModel> Login(UserLoginRequestModel model);

    }
    
}
