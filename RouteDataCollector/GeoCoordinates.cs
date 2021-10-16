namespace RouteDataCollector
{
    internal class GeoCoordinates
    {
        public GeoCoordinates()
        {
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Accuracy { get; internal set; }
    }
}