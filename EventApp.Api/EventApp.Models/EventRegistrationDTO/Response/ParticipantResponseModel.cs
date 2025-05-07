namespace EventApp.Models.EventRegistrationDTO.Response {
    
    public class ParticipantResponseModel {

        public Guid RegistrationId { get; set; } 

        public Guid UserId { get; set; }

        public string? UserName { get; set; }

        public string? UserEmail { get; set; }

        public DateTime RegistrationDate { get; set; }

    }

}
