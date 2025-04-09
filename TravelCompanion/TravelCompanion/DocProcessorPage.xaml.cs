using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using System.Reflection;

namespace TravelCompanion;

public partial class DocProcessorPage : ContentPage
{
    private readonly DocumentAnalysisClient _client;

    public DocProcessorPage()
    {
        InitializeComponent();

        var endpoint = 
            "you-endpoint-url";
        var credential = 
            new AzureKeyCredential("your-api-key");
        _client = new DocumentAnalysisClient(
            new Uri(endpoint), credential);
    }

    private async Task ExtractInvoiceData(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open);
            AnalyzeDocumentOperation operation =
                await _client.AnalyzeDocumentAsync(
                    WaitUntil.Completed, "prebuilt-invoice", stream);

            AnalyzeResult result = operation.Value;

            string invoiceNumber =
                result.Documents[0].Fields["InvoiceId"].Content;
            string totalAmount =
                result.Documents[0].Fields["AmountDue"].Content;
            string dueDate =
                result.Documents[0].Fields["DueDate"].Content;

            TxtInvoiceData.Text =
                $"Invoice Number: {invoiceNumber}\nTotal Amount: " +
                $"{totalAmount}\nDue Date: {dueDate}";
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

    public Stream GetInvoicePdfStream()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = 
            "TravelCompanion.Resources.Raw.sample-invoice.pdf"; // Adjust namespace

        return assembly.GetManifestResourceStream(resourceName);
    }


    private async void AnalyzeButton_Clicked(object sender, EventArgs e)
    {
        string fileName = 
            Path.Combine(FileSystem.CacheDirectory, 
            "sample_invoice.pdf");

        if (!File.Exists(fileName))
        {
            using Stream resource = 
                await FileSystem.
                OpenAppPackageFileAsync("sample_invoice.pdf");
            using FileStream outputStream = 
                File.Create(fileName);
            await resource.CopyToAsync(outputStream);
        }

        await ExtractInvoiceData(fileName);
    }
}