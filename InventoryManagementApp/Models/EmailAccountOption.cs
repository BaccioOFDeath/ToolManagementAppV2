namespace InventoryManagementApp.Models
{
    public sealed class EmailAccountOption
    {
        public EmailAccountOption(string displayName, string emailAddress, string userName)
        {
            DisplayName = displayName;
            EmailAddress = emailAddress;
            UserName = userName;
        }

        public string DisplayName { get; }
        public string EmailAddress { get; }
        public string UserName { get; }

        public string DisplayText => string.IsNullOrWhiteSpace(DisplayName) || DisplayName == EmailAddress
            ? EmailAddress
            : $"{DisplayName} <{EmailAddress}>";
    }
}
