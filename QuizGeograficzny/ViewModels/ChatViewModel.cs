using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using QuizGeograficzny.Models;
using QuizGeograficzny.Services;

namespace QuizGeograficzny.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        private readonly ChatService _chatService;
        private string _messageText = string.Empty;
        private string _currentUser = "Gość";

        public ObservableCollection<ChatMessage> Messages { get; set; } = new ObservableCollection<ChatMessage>();

        public string MessageText
        {
            get => _messageText;
            set { _messageText = value; OnPropertyChanged(); }
        }

        public ICommand SendCommand { get; }

        public ChatViewModel(ChatService chatService)
        {
            _chatService = chatService;
            SendCommand = new Command(async () => await SendMessage());

            InitializeChat();
        }

        public async Task LoadCurrentUserAsync()
        {
            string profileId = Preferences.Get("LocalProfileId", string.Empty);

            if (!string.IsNullOrEmpty(profileId))
            {
                var profile = await RankingService.GetProfileAsync(profileId);

                if (profile != null && !string.IsNullOrEmpty(profile.PlayerName))
                {
                    _currentUser = profile.PlayerName;
                }
            }
        }

        private void InitializeChat()
        {
            var observable = _chatService.GetMessagesObservable();
            observable.Subscribe(msg =>
            {
                if (msg != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Messages.Add(msg);
                    });
                }
            });
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageText)) return;

            var msg = new ChatMessage
            {
                UserName = _currentUser,
                MessageBody = MessageText,
                Timestamp = DateTime.UtcNow
            };

            await _chatService.SendMessageAsync(msg);
            MessageText = string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}