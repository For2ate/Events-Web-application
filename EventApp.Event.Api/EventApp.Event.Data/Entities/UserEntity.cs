namespace EventApp.Data.Entities {

    public class UserEntity : BaseEntity{
    
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public DateTime BirthdayDate { get; set; }

        public string Password { get; set; }


        public virtual ICollection<EventRegistrationEntity> Registrations { get; set; } = new List<EventRegistrationEntity>();

    }

}
