using Firebase.Database;
using Firebase.Database.Query;
using QuizGeograficzny.Models;
using System.Reactive.Linq;

namespace QuizGeograficzny.Services
{
    public class ChatService
    {
        private const string FirebaseUrl = "https://quizgeograficzny-default-rtdb.europe-west1.firebasedatabase.app/";
        private const string ChatNode = "chat";
        private readonly FirebaseClient client;

        public ChatService()
        {
            client = new FirebaseClient(FirebaseUrl);
        }

        public async Task SendMessageAsync(ChatMessage message)
        {
            await client.Child(ChatNode)
                        .PostAsync(message);
        }

        public IObservable<ChatMessage> GetMessagesObservable()
        {
            return client.Child(ChatNode)
                         .AsObservable<ChatMessage>()
                         .Select(x => x.Object);
        }
    }
}