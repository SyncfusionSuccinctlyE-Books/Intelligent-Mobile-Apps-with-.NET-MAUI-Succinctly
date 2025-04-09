using Azure;
using Azure.Core;
using Azure.Core.GeoJson;
using Azure.Maps.Routing;
using Azure.Maps.Routing.Models;
using System;
using System.Threading.Tasks;

public class RouteService
{
    private readonly MapsRoutingClient _routingClient;

    public RouteService(string azureMapsKey)
    {
        var credential = new AzureKeyCredential(azureMapsKey);
        _routingClient = new MapsRoutingClient(credential);
    }

    public async Task<RouteDirections> GetOptimizedRouteAsync(double startLat, double startLon, double destLat, double destLon)
    {
        // Define route request options
        var requestOptions = new RouteDirectionOptions
        {
            RouteType = RouteType.Fastest,  // Can be Shortest, Fastest, or Eco
            TravelMode = TravelMode.Car,    // Can be Car, Truck, Pedestrian, etc.
            UseTrafficData = true,
            InstructionsType = RouteInstructionsType.Text
        };

        var startPoint = new GeoPosition(startLon, startLat); // Longitude first!
        var endPoint = new GeoPosition(destLon, destLat);
        var pointCollection = new List<GeoPosition>() { startPoint, endPoint };

        var directionQuery = new RouteDirectionQuery(pointCollection, requestOptions);
        // Call Azure Maps Route Service
        Response<RouteDirections> response = await _routingClient.GetDirectionsAsync(directionQuery);

        return response.Value;
    }
}
