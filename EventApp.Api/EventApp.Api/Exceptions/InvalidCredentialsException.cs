namespace EventApp.Api.Exceptions {

    public class InvalidCredentialsException : Exception {

        public InvalidCredentialsException()
            : base("Неверный email или пароль.") { }

        public InvalidCredentialsException(string message)
            : base(message) { }

        public InvalidCredentialsException(string message, Exception innerException)
            : base(message, innerException) { }

    }

}
