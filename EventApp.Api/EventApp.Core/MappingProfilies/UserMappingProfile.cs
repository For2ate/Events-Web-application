using AutoMapper;
using EventApp.Data.Entities;
using EventApp.Models.UserDTO.Requests;
using EventApp.Models.UserDTO.Responses;

namespace EventApp.Core.MappingProfilies {
   
    public class UserMappingProfile : Profile
    {

        public UserMappingProfile() {

            CreateMap<UserEntity, UserFullResponseModel>();


            CreateMap<UserRegisterRequestModel, UserEntity>();

        }

    }

}
