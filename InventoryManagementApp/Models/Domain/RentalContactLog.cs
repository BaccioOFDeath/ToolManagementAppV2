using CommunityToolkit.Mvvm.ComponentModel;

namespace InventoryManagementApp.Models.Domain
{
    public class RentalContactLog : ObservableObject
    {
        private int _contactLogID;
        public int ContactLogID
        {
            get => _contactLogID;
            set => SetProperty(ref _contactLogID, value);
        }

        private int _rentalID;
        public int RentalID
        {
            get => _rentalID;
            set => SetProperty(ref _rentalID, value);
        }

        private string _channel = string.Empty;
        public string Channel
        {
            get => _channel;
            set => SetProperty(ref _channel, value);
        }

        private string _direction = string.Empty;
        public string Direction
        {
            get => _direction;
            set => SetProperty(ref _direction, value);
        }

        private string _recipient = string.Empty;
        public string Recipient
        {
            get => _recipient;
            set => SetProperty(ref _recipient, value);
        }

        private string _subject = string.Empty;
        public string Subject
        {
            get => _subject;
            set => SetProperty(ref _subject, value);
        }

        private string _message = string.Empty;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private string _createdBy = string.Empty;
        public string CreatedBy
        {
            get => _createdBy;
            set => SetProperty(ref _createdBy, value);
        }

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        public string Summary => string.IsNullOrWhiteSpace(Subject) ? Message : Subject;
    }
}
