namespace EventApp.Core.Exceptions {

    public class NotFoundException : Exception {
        public string ResourceName { get; }
        public object Identifier { get; }

        public NotFoundException(string resourceName, object identifier)
            : base(FormatMessage(resourceName, identifier)) {
            ResourceName = resourceName;
            Identifier = identifier;
        }

        public NotFoundException(string message) : base(message) {
            ResourceName = "Unknown";
            Identifier = "Unknown";
        }

        public NotFoundException(string message, Exception innerException) : base(message, innerException) {
            ResourceName = "Unknown";
            Identifier = "Unknown";
        }

        private static string FormatMessage(string resourceName, object identifier) {
            return $"Resource '{resourceName}' with identifier '{identifier}' not found.";
        }

    }

}
