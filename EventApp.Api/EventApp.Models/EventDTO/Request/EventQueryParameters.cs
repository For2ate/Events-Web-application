using EventApp.Models.SharedDTO;

namespace EventApp.Models.EventDTO.Request {

    public class EventQueryParameters {

        private const int MaxPageSize = 50;
        private int _pageSize = 10; 

        public int PageNumber { get; set; } = 1; 

        public int PageSize {
            get => _pageSize;
            set => _pageSize = ( value > MaxPageSize ) ? MaxPageSize : value; 
        }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public string? Place { get; set; } 

        public Guid? CategoryId { get; set; }

        public string? NameContains { get; set; }

        public EventSortByEnum? SortBy { get; set; } = EventSortByEnum.date;

        public SortOrderEnum? SortOrder { get; set; } = SortOrderEnum.asc;

    }

}
