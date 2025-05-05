namespace EventApp.Models.EventCategoryDTO.Request {

    public class UpdateEventCategoryRequestModel {

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

    }

}