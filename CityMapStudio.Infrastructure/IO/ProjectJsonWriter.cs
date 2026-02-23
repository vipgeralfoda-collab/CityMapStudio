using System.Text.Json;
using CityMapStudio.Domain.Models;

namespace CityMapStudio.Infrastructure.IO
{
    public interface IProjectJsonWriter
    {
        void WriteProjectMetadata(MapProject project, string outputPath);
    }

    public class ProjectJsonWriter : IProjectJsonWriter
    {
        public void WriteProjectMetadata(MapProject project, string outputPath)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be empty", nameof(outputPath));

            var metadata = new
            {
                version = "1.0",
                mapName = project.MapName,
                width = project.Width,
                height = project.Height,
                bitDepth = project.BitDepth,
                verticalScaleMeters = project.VerticalScaleMeters,
                seaLevelMeters = project.SeaLevelMeters,
                createdAt = project.CreatedAt.ToString("O"), // ISO 8601
                format = "Cities: Skylines II Terrain Package"
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(metadata, options);

            File.WriteAllText(outputPath, json);
        }
    }
}
