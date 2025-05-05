namespace EventApp.Api.Exceptions {

    public class NotFoundException : Exception {

        public NotFoundException(string resourceName, object resourceIdentifier)
           : base($"Resource '{resourceName}' with identifier '{resourceIdentifier}' was not found.") { }

    }

}
