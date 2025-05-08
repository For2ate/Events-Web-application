namespace EventApp.Data.Entities {

    public class EventRegistrationEntity : BaseEntity {

        public DateTime RegistrationDate { get; set; }

        public Guid UserId { get; set; }
        public virtual UserEntity User { get; set; }

        public Guid EventId { get; set; }
        public virtual EventEntity Event { get; set; }

    }

}
