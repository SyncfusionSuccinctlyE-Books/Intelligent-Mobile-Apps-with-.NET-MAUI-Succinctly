using Azure;
using Azure.AI.TextAnalytics;
using Azure.AI.Translation.Text;

namespace TravelCompanion;

public partial class NaturalLanguagePage : ContentPage
{
    private const string AILanguageApiKey = 
        "your-ai-language-api-key";
    private const string AILanguageEndpoint = 
        "your-ai-language-endpoint";
    private readonly TextAnalyticsClient _languageClient;
    private const string TranslatorApiKey = 
        "your-ai-translator-api-key";
    private const string TranslatorEndpoint = 
        "your-ai-translator-endpoint";
    private readonly TextTranslationClient _translatorClient;

    private async void OnAnalyzeSentimentClicked(object sender, 
        EventArgs e)
    {
        try
        {
            var text = SentimentInputText.Text;
            if (string.IsNullOrWhiteSpace(text)) return;
            var result = _languageClient.AnalyzeSentiment(text);
            ResultLabel.Text =
                $"Sentiment: {result.Value.Sentiment}";
        }
        catch (RequestFailedException ex)
        {
            await DisplayAlert("Error", ex.ErrorCode, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnTranslateClicked(object sender, 
        EventArgs e)
    {
        try
        {
            var text = TranslatorInputText.Text;
            var targetLanguage =
                LanguagePicker.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(text) ||
                string.IsNullOrWhiteSpace(targetLanguage)) return;
            var response =
                await _translatorClient.
                TranslateAsync(targetLanguage, text);
            TranslatedTextLabel.Text =
                response.Value.FirstOrDefault()?.Translations.
                FirstOrDefault()?.Text ?? "Translation failed";
        }
        catch (RequestFailedException ex)
        {
            await DisplayAlert("Error", ex.ErrorCode, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    public NaturalLanguagePage()
	{
		InitializeComponent();
        _languageClient = 
            new TextAnalyticsClient(new Uri(AILanguageEndpoint), 
            new AzureKeyCredential(AILanguageApiKey));
        _translatorClient = 
            new TextTranslationClient(
                new AzureKeyCredential(TranslatorApiKey), 
            new Uri(TranslatorEndpoint));
    }
}
