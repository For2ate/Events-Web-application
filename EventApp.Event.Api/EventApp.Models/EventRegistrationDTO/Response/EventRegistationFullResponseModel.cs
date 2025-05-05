namespace EventApp.Models.EventRegistrationDTO.Response {

    public class EventRegistationFullResponseModel {

        public Guid Id { get; set; } 

        public DateTime RegistrationDate { get; set; }


        public Guid UserId { get; set; }
        
        public string? UserName { get; set; }
        
        public string? UserEmail { get; set; }


        public Guid EventId { get; set; }

        public string? EventName { get; set; }

    }

}
