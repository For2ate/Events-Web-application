using EventApp.Data.Entities;
using EventApp.Models.SharedDTO;
using EventApp.Models.UserDTO.Requests;
using EventApp.Models.UserDTO.Responses;
using System.Security.Claims;

namespace EventApp.Core.Interfaces {
    
    public interface ITokenService {

        TokensResponse GenerateTokens(UserFullResponseModel user);

        ClaimsPrincipal ValidateRefreshToken(string token);

    }

}
