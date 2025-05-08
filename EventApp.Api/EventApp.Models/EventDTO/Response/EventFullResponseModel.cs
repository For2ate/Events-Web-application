namespace EventApp.Models.EventDTO.Response {

    public class EventFullResponseModel {

        public Guid Id { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime DateOfEvent { get; set; }

        public string Place { get; set; }

        public int CurrentNumberOfParticipants { get; set; }

        public int MaxNumberOfParticipants { get; set; }

        public string ImageUrl { get; set; }

        public Guid CategoryId { get; set; }

    }

}
