using System.Threading;
using System.Threading.Tasks;

namespace RouteDataCollector.Services.Location
{
    internal interface ILocationService
    {
        Task<GeoCoordinates> GetCurrentLocation(CancellationToken cancellationToken);
    }
}