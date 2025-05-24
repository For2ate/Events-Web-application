using EventApp.Models.UserDTO.Responses;

namespace EventApp.Core.Interfaces {

    public interface IUserService {

        Task<UserFullResponseModel> GetUserByIdAsync(Guid id);

    }

}
