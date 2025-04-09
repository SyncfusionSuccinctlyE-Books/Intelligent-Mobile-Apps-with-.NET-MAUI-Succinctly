using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Collections.ObjectModel;

namespace TravelCompanion;

public partial class ChatbotPage : ContentPage
{
    private const string ApiKey = 
        "your-api-key";
    private readonly AzureOpenAIClient openAiClient;
    private ChatClient chatClient;
    public ObservableCollection<string> Messages { get; set; } = new();

    public ChatbotPage()
    {
        InitializeComponent();

        var endpoint =
            new Uri("your-endpoint");
        var credential = new AzureKeyCredential(ApiKey);

        openAiClient = new AzureOpenAIClient(
            endpoint,
            credential);

        ChatMessages.ItemsSource = Messages;
    }

    private async void OnSendMessageClicked(object sender, EventArgs e)
    {
        var userInput = MessageEntry.Text;
        if (string.IsNullOrWhiteSpace(userInput)) return;

        Messages.Add($"You: {userInput}");
        MessageEntry.Text = string.Empty;
        await GetChatResponse(userInput);
    }

    private async Task GetChatResponse(string userInput)
    {
        try
        {
            chatClient = openAiClient.GetChatClient("gpt-35-turbo");

            var message = new UserChatMessage(userInput);

            var response = await chatClient.CompleteChatAsync(message);
            Messages.Add($"Assistant: {response.Value.Content[0].Text}");
        }
        catch (Exception ex)
        {
            Messages.Add($"Bot: An error occurred - {ex.Message}");
        }
    }
}