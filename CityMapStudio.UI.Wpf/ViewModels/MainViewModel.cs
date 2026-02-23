using System;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CityMapStudio.Application.Services;
using CityMapStudio.Domain.Models;
using CityMapStudio.Infrastructure.IO;
using CityMapStudio.Rendering3D.Services;

namespace CityMapStudio.UI.Wpf.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IHeightmapGenerator _heightmapGenerator;
        private readonly IImportHeightmapFromPng _importHeightmap;
        private readonly IExportPackageUseCase _exportPackage;
        private readonly TerrainMeshGenerator _meshGenerator;
        private readonly IHeightmapSmoothingService _smoothingService;
        private readonly IHeightmapNormalizationService _normalizationService;
        private readonly IHeightmapDownscaleService _downscaleService;
        private RelayCommand _resetCameraCommand;
        private RelayCommand _importHeightmapCommand;
        private RelayCommand _exportPackageCommand;

        [ObservableProperty]
        private MeshGeometry3D terrainMesh;

        [ObservableProperty]
        private MeshGeometry3D waterMesh;

        [ObservableProperty]
        private float verticalScale = 50f;

        [ObservableProperty]
        private float seaLevel = 0f;

        [ObservableProperty]
        private float seaLevelMin = 0f;

        [ObservableProperty]
        private float seaLevelMax = 100f;

        [ObservableProperty]
        private string seaLevelInfo = "Sea Level: 0.0 m";

        [ObservableProperty]
        private bool isHeightmapLoaded = false;

        [ObservableProperty]
        private string statusText = "Ready";

        [ObservableProperty]
        private string heightmapInfo = "No heightmap loaded";

        [ObservableProperty]
        private Heightmap16Bit currentHeightmap;

        [ObservableProperty]
        private Heightmap16Bit currentHeightmapOriginal;

        [ObservableProperty]
        private float smoothLevel = 0f;

        [ObservableProperty]
        private float targetMaxHeight = 300f;

        [ObservableProperty]
        private bool normalizeForCS2 = false;

        [ObservableProperty]
        private bool applySmoothOnExport = false;

        [ObservableProperty]
        private PreviewResolution selectedPreviewResolution = PreviewResolution.R512;

        [ObservableProperty]
        private string previewInfo = "Preview: Waiting for heightmap";

        private CancellationTokenSource _previewCancellationToken;
        private System.Timers.Timer _previewDebounceTimer;

        [ObservableProperty]
        private float renderResolution = 512f;

        [ObservableProperty]
        private bool isLoading = false;

        public ICommand GenerateTestTerrainCommand { get; }
        
        public ICommand ImportHeightmapCommand
        {
            get => _importHeightmapCommand;
            set => _importHeightmapCommand = value as RelayCommand;
        }

        public ICommand ExportPackageCommand
        {
            get => _exportPackageCommand;
            set => _exportPackageCommand = value as RelayCommand;
        }
        
        public ICommand ResetCameraCommand
        {
            get => _resetCameraCommand;
            set => _resetCameraCommand = value as RelayCommand;
        }

        public MainViewModel()
        {
            _heightmapGenerator = new HeightmapGenerator();
            _meshGenerator = new TerrainMeshGenerator();
            _smoothingService = new HeightmapSmoothingService();
            _normalizationService = new HeightmapNormalizationService();
            _downscaleService = new HeightmapDownscaleService();
            
            var reader = new PngHeightmap16BitReader();
            _importHeightmap = new ImportHeightmapFromPng(reader);

            // Inicializar exportador
            var heightmapExporter = new HeightmapExporter();
            var jsonWriter = new ProjectJsonWriter();
            var readmeGenerator = new ReadmeGenerator();
            var exportService = new ExportPackageService(heightmapExporter, jsonWriter, readmeGenerator);
            _exportPackage = new ExportPackageUseCase(exportService);

            GenerateTestTerrainCommand = new RelayCommand(GenerateTestTerrain);
            _importHeightmapCommand = new RelayCommand(() => { });
            _exportPackageCommand = new RelayCommand(() => { });
            _resetCameraCommand = new RelayCommand(ResetCamera);

            // Inicializar debounce timer para preview
            _previewDebounceTimer = new System.Timers.Timer(200); // 200ms debounce
            _previewDebounceTimer.AutoReset = false;
            _previewDebounceTimer.Elapsed += (s, e) => RegeneratePreviewAsync().Wait();
        }

        /// <summary>
        /// Inicializa os comandos que dependem da View
        /// Deve ser chamado ANTES de setar DataContext
        /// </summary>
        public void InitializeViewCommands(Action onResetCamera, Action onImportHeightmap, Action onExportPackage)
        {
            _resetCameraCommand = new RelayCommand(onResetCamera);
            _importHeightmapCommand = new RelayCommand(onImportHeightmap);
            _exportPackageCommand = new RelayCommand(onExportPackage);
        }

        public void ExportMapPackage(string destinationFolder)
        {
            if (!IsHeightmapLoaded || CurrentHeightmapOriginal == null)
                throw new InvalidOperationException("Nenhum heightmap carregado");

            StatusText = "Exportando...";

            // Preparar heightmap para export
            var heightmapForExport = CurrentHeightmapOriginal.Clone();

            // Aplicar smooth se ativado
            if (ApplySmoothOnExport && SmoothLevel > 0)
            {
                var smoothStrength = (SmoothStrength)(int)SmoothLevel;
                heightmapForExport = _smoothingService.ApplySmooth(heightmapForExport, smoothStrength);
            }

            // Aplicar normalização se ativada
            if (NormalizeForCS2)
            {
                heightmapForExport = _normalizationService.NormalizeForCS2(heightmapForExport, TargetMaxHeight, VerticalScale);
            }

            var project = new MapProject(
                "My_Map",
                heightmapForExport,
                VerticalScale,
                SeaLevel
            );

            // Validar projeto (vai lançar se inválido)
            var (isValid, errorMessage) = project.Validate();
            if (!isValid)
                throw new InvalidOperationException(errorMessage);

            // Executar exportação
            _exportPackage.Execute(project, heightmapForExport, destinationFolder);
            
            StatusText = "✓ Pacote exportado com sucesso!";
        }

        public async void ImportHeightmapFromFile(string filePath)
        {
            try
            {
                IsLoading = true;
                StatusText = "Carregando heightmap...";

                // Importar em thread separada
                var heightmap = await Task.Run(() => _importHeightmap.ImportFromFile(filePath));
                
                // Guardar original e preview
                CurrentHeightmapOriginal = heightmap;
                CurrentHeightmap = heightmap.Clone();

                // Atualizar range de Sea Level baseado no heightmap
                var (minHeight, maxHeight) = TerrainSettings.GetHeightRange(heightmap, VerticalScale);
                SeaLevelMin = minHeight;
                SeaLevelMax = maxHeight;
                SeaLevel = (minHeight + maxHeight) / 2f;

                IsHeightmapLoaded = true;

                // Renderizar com preview pipeline em background
                StatusText = "Processando malha 3D...";
                await RegeneratePreviewAsync();

                HeightmapInfo = $"Full: {CurrentHeightmapOriginal.Width}x{CurrentHeightmapOriginal.Height}";
                StatusText = $"✓ Heightmap importado: {System.IO.Path.GetFileName(filePath)}";
            }
            catch (Exception ex)
            {
                StatusText = $"Import Error: {ex.Message}";
                IsHeightmapLoaded = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void GenerateTestTerrain()
        {
            try
            {
                StatusText = "Generating...";

                // Gerar heightmap procedural FULL 4096x4096
                CurrentHeightmapOriginal = _heightmapGenerator.Generate(4096, 4096, 1.0f, Environment.TickCount);

                // Atualizar range de Sea Level
                var (minHeight, maxHeight) = TerrainSettings.GetHeightRange(CurrentHeightmapOriginal, VerticalScale);
                SeaLevelMin = minHeight;
                SeaLevelMax = maxHeight;
                SeaLevel = (minHeight + maxHeight) / 2f;

                IsHeightmapLoaded = true;

                // Renderizar com preview pipeline
                StatusText = "Processando malha 3D...";
                _ = RegeneratePreviewAsync();

                HeightmapInfo = $"Full: {CurrentHeightmapOriginal.Width}x{CurrentHeightmapOriginal.Height}";
                StatusText = "Test terrain generated";
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
                IsHeightmapLoaded = false;
            }
        }

        private void ResetCamera()
        {
            StatusText = "Camera reset";
        }

        /// <summary>
        /// Regenera preview de forma assíncrona com debounce
        /// </summary>
        private async Task RegeneratePreviewAsync()
        {
            if (CurrentHeightmapOriginal == null || !IsHeightmapLoaded)
                return;

            IsLoading = true;
            PreviewInfo = $"Generating preview ({(int)SelectedPreviewResolution}x{(int)SelectedPreviewResolution})...";

            try
            {
                // Cancelar job anterior se existir
                _previewCancellationToken?.Cancel();
                _previewCancellationToken = new CancellationTokenSource();

                await Task.Run(async () =>
                {
                    var token = _previewCancellationToken.Token;

                    // Aplicar smooth + normalização no original
                    var processed = CurrentHeightmapOriginal.Clone();

                    if (SmoothLevel > 0)
                    {
                        var smoothStrength = (SmoothStrength)(int)SmoothLevel;
                        processed = _smoothingService.ApplySmooth(processed, smoothStrength);
                    }

                    if (NormalizeForCS2)
                    {
                        processed = _normalizationService.NormalizeForCS2(processed, TargetMaxHeight, VerticalScale);
                    }

                    token.ThrowIfCancellationRequested();

                    // Downscale para preview
                    var preview = _downscaleService.CreatePreview(processed, SelectedPreviewResolution);
                    CurrentHeightmap = preview;

                    token.ThrowIfCancellationRequested();

                    // Regenerar mesh
                    await RegenerateMeshAsync();
                });

                PreviewInfo = $"Preview: {(int)SelectedPreviewResolution}x{(int)SelectedPreviewResolution} (Full export: 4096x4096)";
            }
            catch (OperationCanceledException)
            {
                // Ignorar cancelamentos
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Agenda regeneração de preview com debounce
        /// </summary>
        private void SchedulePreviewUpdate()
        {
            _previewDebounceTimer.Stop();
            _previewDebounceTimer.Start();
        }

        /// <summary>
        /// Regenera malha 3D com reamostragem baseada em RenderResolution
        /// Simples, sem cache, sem overhead
        /// </summary>
        public async Task RegenerateMeshAsync()
        {
            if (CurrentHeightmap == null || !IsHeightmapLoaded)
                return;

            IsLoading = true;

            try
            {
                await Task.Run(() =>
                {
                    // Downsample para resolução de renderização
                    int targetSize = (int)RenderResolution;
                    var renderHeightmap = CurrentHeightmap.Downsample(targetSize, targetSize);

                    // Gerar malha com a versão redimensionada
                    var meshData = _meshGenerator.GenerateTerrainMeshWithSeaLevel(
                        renderHeightmap, VerticalScale, SeaLevel, 1.0f);

                    // Atualizar no thread principal
                    TerrainMesh = meshData.TerrainMesh;
                    WaterMesh = meshData.WaterMesh;
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnVerticalScaleChanged(float oldValue, float newValue)
        {
            if (CurrentHeightmap != null && IsHeightmapLoaded)
            {
                var (minHeight, maxHeight) = TerrainSettings.GetHeightRange(CurrentHeightmap, newValue);
                SeaLevelMin = minHeight;
                SeaLevelMax = maxHeight;
                SeaLevel = TerrainSettings.ClampSeaLevel(SeaLevel, CurrentHeightmap, newValue);

                _ = RegenerateMeshAsync();
            }
        }

        partial void OnSeaLevelChanged(float oldValue, float newValue)
        {
            SeaLevelInfo = $"Sea Level: {newValue:F1} m";

            if (CurrentHeightmap != null && IsHeightmapLoaded)
            {
                _ = RegenerateMeshAsync();
            }
        }

        partial void OnRenderResolutionChanged(float oldValue, float newValue)
        {
            if (CurrentHeightmap != null && IsHeightmapLoaded)
            {
                _ = RegenerateMeshAsync();
            }
        }

        partial void OnSmoothLevelChanged(float oldValue, float newValue)
        {
            if (CurrentHeightmapOriginal != null && IsHeightmapLoaded)
            {
                SchedulePreviewUpdate();
            }
        }

        partial void OnTargetMaxHeightChanged(float oldValue, float newValue)
        {
            if (CurrentHeightmapOriginal != null && IsHeightmapLoaded && NormalizeForCS2)
            {
                SchedulePreviewUpdate();
            }
        }

        partial void OnNormalizeForCS2Changed(bool oldValue, bool newValue)
        {
            if (CurrentHeightmapOriginal != null && IsHeightmapLoaded)
            {
                SchedulePreviewUpdate();
            }
        }

        partial void OnSelectedPreviewResolutionChanged(PreviewResolution oldValue, PreviewResolution newValue)
        {
            if (CurrentHeightmapOriginal != null && IsHeightmapLoaded)
            {
                SchedulePreviewUpdate();
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _execute();
    }
}
