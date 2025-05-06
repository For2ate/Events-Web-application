using System;
namespace EventApp.Data.Entities {

    public class EventEntity : BaseEntity {

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime DateOfEvent { get; set; }

        public int CurrentNumberOfParticipants { get; set; }

        public int MaxNumberOfParticipants { get; set; }

        public string ImageUrl { get; set; }

        public string Place { get; set; }

        public Guid CategoryId { get; set; }
        public virtual EventCategoryEntity Category { get; set; }


        public virtual ICollection<EventRegistrationEntity> Registrations { get; set; } = new List<EventRegistrationEntity>();

    }

}
