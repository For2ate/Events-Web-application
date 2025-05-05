using EventApp.Models.UserDTO.Responses;

namespace EventApp.Api.Core.Interfaces {

    public interface IUserService {

        Task<UserFullResponseModel> GetUserByIdAsync(Guid id);

    }

}
