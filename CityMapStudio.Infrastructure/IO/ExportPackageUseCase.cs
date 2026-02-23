using CityMapStudio.Domain.Models;

namespace CityMapStudio.Infrastructure.IO
{
    public interface IExportPackageUseCase
    {
        void Execute(MapProject project, Heightmap16Bit heightmap, string destinationFolder);
    }

    public class ExportPackageUseCase : IExportPackageUseCase
    {
        private readonly IExportPackageService _exportService;

        public ExportPackageUseCase(IExportPackageService exportService)
        {
            _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        }

        public void Execute(MapProject project, Heightmap16Bit heightmap, string destinationFolder)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            if (heightmap == null)
                throw new ArgumentNullException(nameof(heightmap));

            if (string.IsNullOrWhiteSpace(destinationFolder))
                throw new ArgumentException("Destination folder must be specified", nameof(destinationFolder));

            try
            {
                _exportService.ExportMapPackage(project, heightmap, destinationFolder);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro durante exportação: {ex.Message}", ex);
            }
        }
    }
}
