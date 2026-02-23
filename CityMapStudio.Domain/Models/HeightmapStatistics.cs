namespace CityMapStudio.Domain.Models
{
    /// <summary>
    /// Estatísticas do heightmap
    /// </summary>
    public class HeightmapStatistics
    {
        public ushort MinHeight { get; set; }
        public ushort MaxHeight { get; set; }
        public double AverageHeight { get; set; }
        public double StandardDeviation { get; set; }

        public HeightmapStatistics(ushort min, ushort max, double avg, double stdDev)
        {
            MinHeight = min;
            MaxHeight = max;
            AverageHeight = avg;
            StandardDeviation = stdDev;
        }

        public float GetRangeMeters(float verticalScale)
        {
            return ((MaxHeight - MinHeight) / 65535f) * verticalScale;
        }
    }
}
