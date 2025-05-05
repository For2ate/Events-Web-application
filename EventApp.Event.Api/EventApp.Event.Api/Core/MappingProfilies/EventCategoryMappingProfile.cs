using AutoMapper;
using EventApp.Data.Entities;
using EventApp.Models.EventCategoriyDTO.Response;
using EventApp.Models.EventCategoryDTO.Request;

namespace EventApp.Api.Core.MappingProfilies {

    public class EventCategoryMappingProfile : Profile {

        public EventCategoryMappingProfile() {

            CreateMap<EventCategoryEntity, EventCategoryFullResponseModel>();


            CreateMap<CreateEventCategoryRequestModel, EventCategoryEntity>();
            CreateMap<UpdateEventCategoryRequestModel, EventCategoryEntity>();

        }

    }

}
