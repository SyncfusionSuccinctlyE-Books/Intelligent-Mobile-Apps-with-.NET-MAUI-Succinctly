using Azure;
using Azure.Search.Documents;
using System.Collections.ObjectModel;

namespace TravelCompanion;

public partial class SearchPage : ContentPage
{
    private const string AdminKey = "your-admin-key";
    private const string Endpoint = "your-endpoint-url";
    private const string IndexName =
        "travel-destinations";

    private readonly SearchClient searchClient;
    public ObservableCollection<TravelDestination>
        SearchResults
    { get; set; } = new();

    public SearchPage()
    {
        InitializeComponent();
        var credential = new AzureKeyCredential(AdminKey);
        searchClient =
            new SearchClient(new Uri(Endpoint),
            IndexName, credential);
        this.BindingContext = SearchResults;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        bool destinationsUploaded =
            Preferences.Get("destinations", false);
        if (!destinationsUploaded)
            await UploadSampleDataAsync();
    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        var query = SearchEntry.Text;
        if (string.IsNullOrWhiteSpace(query)) return;
        await PerformSearch(query);
    }

    private async Task PerformSearch(string query)
    {
        try
        {
            var options = new SearchOptions
            { IncludeTotalCount = true };
            var response =
                await searchClient.
                SearchAsync<TravelDestination>(query, options);

            await foreach (var result in
                response.Value.GetResultsAsync())
            {
                SearchResults.Add(result.Document);
            }
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

    public async Task UploadSampleDataAsync()
    {
        try
        {
            var destinations = new List<TravelDestination>
        {
            new TravelDestination { Id = "1",
                Name = "Paris",
                Description = "The city of love.",
                Rating = 4.8 },
            new TravelDestination { Id = "2",
                Name = "Rome",
                Description = "The Eternal City.",
                Rating = 4.7 }
        };

            await searchClient.UploadDocumentsAsync(destinations);
            Preferences.Set("destinations", true);
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

    public async Task<List<TravelDestination>> GetRecommendedDestinationsAsync()
    {
        try
        {
            var options = new SearchOptions
            {
                Filter = "Rating ge 4.7",
                OrderBy = { "Rating desc" }
            };

            var searchResults =
                await searchClient.SearchAsync<TravelDestination>("", options);
            var recommendedList =
                new List<TravelDestination>();

            await foreach (var result in
                searchResults.Value.GetResultsAsync())
            {
                recommendedList.Add(result.Document);
            }

            return recommendedList;
        }
        catch (RequestFailedException ex)
        {
            await DisplayAlert("Error", ex.ErrorCode, "OK");
            return null;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
            return null;
        }
    }
}