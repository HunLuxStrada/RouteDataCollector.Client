using Microsoft.Maui.Essentials;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RouteDataCollector.Services.Location
{
    internal class LocationService
    {
        public static async Task<GeoCoordinates> GetCurrentLocation(CancellationToken cancellationToken, int timeout = 100)
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(timeout));
                var location = await Geolocation.GetLocationAsync(request, cancellationToken);
                if (location != null)
                {
                    Log.Verbose("Latitude, Longitude: {Latitude}, {Longitude}; Accuracy: {Accuracy}", location.Latitude, location.Longitude, location.Accuracy);
                    return new()
                    {
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        Accuracy = location.Accuracy,
                    };
                }
            }
            catch (FeatureNotEnabledException ex)
            {
                Log.Debug($"{ex}");
            }
            catch (FeatureNotSupportedException ex)
            {
                Log.Debug($"{ex}");
            }
            catch (PermissionException ex)
            {
                Log.Debug($"{ex}");
            }
            catch (Exception ex)
            {
                Log.Debug($"{ex}");
            }
            return null;
        }
    }
}
