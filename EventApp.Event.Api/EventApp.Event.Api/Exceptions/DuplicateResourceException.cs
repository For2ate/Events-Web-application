namespace EventApp.Api.Exceptions {

    public class DuplicateResourceException : Exception {

        public DuplicateResourceException(string message) : base(message) { }

        public DuplicateResourceException(string resourceName, string resourceIdentifier)
            : base($"Resource '{resourceName}' with identifier '{resourceIdentifier}' already exists.") { }

    }

}
