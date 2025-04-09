using Azure;
using Microsoft.Maui.Controls.Maps;

namespace TravelCompanion;

public partial class MapsPage : ContentPage
{
    public MapsPage()
    {
        InitializeComponent();
    }

    private Location selectedDestination;

    private void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        selectedDestination = new Location(e.Location.Latitude,
            e.Location.Longitude);

        Pin destinationPin = new Pin
        {
            Label = "Destination",
            Type = PinType.Place,
            Location = new Location(selectedDestination.Latitude,
             selectedDestination.Longitude)
        };
        mapControl.Pins.Clear();
        mapControl.Pins.Add(destinationPin);
    }

    private async void OnGetRouteClicked(object sender,
        EventArgs e)
    {
        if (selectedDestination != null)
        {
            CalculateRoute(selectedDestination.Latitude,
                selectedDestination.Longitude);
        }
        else
        {
            await DisplayAlert("Error",
                "Please select a destination on the map", "OK");
        }
    }


    private async Task<bool> RequestLocationPermissionsAsync()
    {
        var locationStatus = await Permissions.
            CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (locationStatus != PermissionStatus.Granted)
        {
            locationStatus = await Permissions.
                RequestAsync<Permissions.LocationWhenInUse>();
        }

        return locationStatus == PermissionStatus.Granted;
    }

    public async void CalculateRoute(double destinationLat, double destinationLon)
    {
        bool permissions = await RequestLocationPermissionsAsync();
        if (!permissions) return;
        try
        {

            var routeService = new RouteService("api-key");

            GeolocationRequest request =
                new GeolocationRequest(GeolocationAccuracy.Low,
                TimeSpan.FromSeconds(10));

            var _cancelTokenSource = new CancellationTokenSource();

            Location currentLocation =
                await Geolocation.Default.
                GetLocationAsync(request, _cancelTokenSource.Token);

            if (currentLocation == null)
            {
                await DisplayAlert("Error", "Unable to get current location", "OK");
                return;
            }

            var route = await routeService.GetOptimizedRouteAsync(
                currentLocation.Latitude, currentLocation.Longitude,
                destinationLat, destinationLon
            );

            if (route.Routes.Count > 0)
            {
                var singleRoute = route.Routes[0];

                var polyline = new Polyline
                {
                    StrokeColor = Colors.Blue, // Set polyline color
                    StrokeWidth = 5
                };

                foreach (var point in singleRoute.Legs[0].Points)
                {
                    polyline.Geopath.Add(
                        new Location(point.Latitude, point.Longitude));
                }

                mapControl.MapElements.Clear();
                mapControl.MapElements.Add(polyline);

                var directions = singleRoute.Guidance.
                    Instructions.Select(i => i.Message).ToList();

                string formattedDirections = string.Join("\n", directions);
                DirectionsLabel.Text = formattedDirections;
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

}