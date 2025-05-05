using AutoMapper;
using EventApp.Data.Entities;
using EventApp.Models.EventDTO.Request;
using EventApp.Models.EventDTO.Response;

namespace EventApp.Data.MappingProfilies {

    public class EventMappingProfile : Profile {

        public EventMappingProfile() {

            CreateMap<EventEntity, EventFullResponseModel>();

            CreateMap<CreateEventRequestModel, EventEntity>();
            CreateMap<UpdateEventRequestModel, EventEntity>();

        }

    }

}
