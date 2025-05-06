using AutoMapper;
using EventApp.Data.Entities;
using EventApp.Models.EventRegistrationDTO.Response;

namespace EventApp.Api.Core.MappingProfilies {

    public class EventRegistationMappingProfile : Profile {
    
        public EventRegistationMappingProfile() {

            CreateMap<EventRegistrationEntity,EventRegistrationFullResponseModel>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FirstName : null)) 
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null)) 
                .ForMember(dest => dest.EventName, opt => opt.MapFrom(src => src.Event != null ? src.Event.Name : null));

            CreateMap<EventRegistrationEntity, ParticipantResponseModel>()
                .ForMember(dest => dest.RegistrationId, opt => opt.MapFrom(src => src.Id)) 
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FirstName : null))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null));

        }
    
    }

}
