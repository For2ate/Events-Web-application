using EventApp.Models.SharedDTO;

namespace EventApp.Models.EventCategoriyDTO.Request {

    public class EventCategoryPagedQueryParametrs : BasePagedQueryParametrs{

        public string? NameContains { get; set; }

        public EventCategorySortByEnum SortBy { get; set; } = EventCategorySortByEnum.Name;

        public SortOrderEnum SortOrder { get; set; } = SortOrderEnum.asc;

    }

}
