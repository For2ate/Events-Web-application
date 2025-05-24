using AutoMapper;
using EventApp.Core.Resolvers;
using EventApp.Data.Entities;
using EventApp.Models.EventDTO.Request;
using EventApp.Models.EventDTO.Response;

namespace EventApp.Core.MappingProfilies {

    public class EventMappingProfile : Profile {

        public EventMappingProfile() {

            CreateMap<EventEntity, EventFullResponseModel>()
                .ForMember(
                    destination => destination.ImageUrl, 
                    options => options.MapFrom<ImageUrlResolver>() 
                );

            CreateMap<CreateEventRequestModel, EventEntity>();
            CreateMap<UpdateEventRequestModel, EventEntity>();

        }

    }

}
