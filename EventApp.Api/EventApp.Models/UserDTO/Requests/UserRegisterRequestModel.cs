namespace EventApp.Models.UserDTO.Requests {

    public class UserRegisterRequestModel {

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public DateTime BirthdayDate { get; set; }

    }

}
