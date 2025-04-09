using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Microsoft.CognitiveServices.Speech;
using System.Text;
using System.Threading.Tasks;

namespace TravelCompanion;

public partial class ImageAnalysisPage : ContentPage
{
    private const string SubscriptionKey =
        "your-ai-vision-api-key";
    private const string Endpoint =
        "your-ai-vision-endpoint";
    private const string SpeechSubscriptionKey =
        "your-ai-speech-api-key";
    private const string SpeechRegion = "your-region";

    public ImageAnalysisPage()
    {
        InitializeComponent();
    }

    private async Task<bool> RequestPhotoPermissionsAsync()
    {
        var mediaStatus = await Permissions.
            CheckStatusAsync<Permissions.Photos>();
        if (mediaStatus != PermissionStatus.Granted)
        {
            mediaStatus = await Permissions.
                RequestAsync<Permissions.Photos>();
        }

        return mediaStatus == PermissionStatus.Granted;
    }

    private async Task<bool> RequestMicrophonePermissionsAsync()
    {
        var micStatus = await Permissions.
            CheckStatusAsync<Permissions.Microphone>();
        if (micStatus != PermissionStatus.Granted)
        {
            micStatus = await Permissions.
                RequestAsync<Permissions.Microphone>();
        }

        return micStatus == PermissionStatus.Granted;
    }


    private async void OnPickImageClicked(object sender, 
        EventArgs e)
    {
        bool permissionCheck = await RequestPhotoPermissionsAsync();
        if (!permissionCheck)
        {
            await DisplayAlert("Error", 
                "You do not have permissions to access the photo gallery", 
                "OK");
            return;
        }

        var result = await FilePicker.PickAsync(
            new PickOptions
            {
                FileTypes = FilePickerFileType.Images
            });

        if (result != null && !string.
            IsNullOrEmpty(result.FullPath))
        {
            SelectedImage.Source = ImageSource.
                FromFile(result.FullPath);
            ((Button)sender).IsEnabled = true;
        }
    }

    private async void OnAnalyzeImageClicked(object sender, 
        EventArgs e)
    {
        try
        {
            if (SelectedImage.Source == null)
                return;

            var imagePath =
                ((FileImageSource)SelectedImage.Source).File;
            var imageBytes = File.ReadAllBytes(imagePath);

            var credential =
                new AzureKeyCredential(SubscriptionKey);
            var client = new ImageAnalysisClient(
                new Uri(Endpoint), credential);

            using var imageStream = new MemoryStream(imageBytes);
            var binaryData = BinaryData.FromStream(imageStream);
            var result = await client.AnalyzeAsync(binaryData,
                VisualFeatures.Caption | VisualFeatures.Tags |
                VisualFeatures.Objects,
                new ImageAnalysisOptions { Language = "en" });

            string descriptionResult = $"Description: " +
                            $"{result.Value.Caption.Text}\n";
            descriptionResult += "Tags: " + string.Join(", ",
                result.Value.Tags.Values.Select(tag => tag.Name)) + "\n";
            descriptionResult += "Objects:\n";


            AnalysisResult.Text = descriptionResult ??
                "No description available.";
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

    private async void OnOcrAnalyzeImageClicked(object sender,
    EventArgs e)
    {
        try
        {
            if (SelectedImage.Source == null)
                return;

            var imagePath =
                ((FileImageSource)SelectedImage.Source).File;
            var imageBytes = File.ReadAllBytes(imagePath);

            var credential =
                new AzureKeyCredential(SubscriptionKey);
            var client = new ImageAnalysisClient(
                new Uri(Endpoint), credential);

            using var imageStream = new MemoryStream(imageBytes);
            var binaryData = BinaryData.FromStream(imageStream);
            ImageAnalysisResult result = await client.AnalyzeAsync(
                binaryData,
                VisualFeatures.Read);

            var resultStringBuilder = new StringBuilder();

            foreach (DetectedTextBlock block in result.Read.Blocks)
            {
                foreach (DetectedTextLine line in block.Lines)
                {
                    // Only include the text of the line
                    resultStringBuilder.AppendLine(line.Text);

                    // Optionally, you can append each word here if needed
                    // foreach (DetectedTextWord word in line.Words)
                    // {
                    //     resultStringBuilder.
                    //     AppendLine($"Word: '{word.Text}'");
                    // }
                }
            }
            string ocrResult = resultStringBuilder.ToString();

            AnalysisResult.Text = ocrResult ??
                "No description available.";
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

    private async void OnVoiceCommandClicked(object sender, EventArgs e)
    {
        bool permissionCheck = await RequestMicrophonePermissionsAsync();
        if (!permissionCheck)
        {
            await DisplayAlert("Error",
                "You do not have permissions to access the microphone",
                "OK");
            return;
        }

        var speechConfig = SpeechConfig.
            FromSubscription(SpeechSubscriptionKey, SpeechRegion);
        using var recognizer = new SpeechRecognizer(speechConfig);

        var result = await recognizer.RecognizeOnceAsync();

        if (result.Text.Contains("Analyze image", 
            StringComparison.OrdinalIgnoreCase))
        {
            OnAnalyzeImageClicked(sender, e);
        }
    }

    private async void OnReadDescriptionClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(AnalysisResult.Text))
                return;

            var speechConfig = SpeechConfig.
                FromSubscription(SpeechSubscriptionKey, SpeechRegion);
            using var synthesizer = new SpeechSynthesizer(speechConfig);

            await synthesizer.SpeakTextAsync(AnalysisResult.Text);
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
}