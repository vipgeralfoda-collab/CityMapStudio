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
        private readonly CameraControllerService _cameraController = new CameraControllerService();

        // Cached resources
        private Dictionary<PreviewResolution, Heightmap16Bit> _previewCache = 
            new Dictionary<PreviewResolution, Heightmap16Bit>();
        private Dictionary<(int step, float seaLevel, float verticalScale), TerrainMeshData> _meshCache =
            new Dictionary<(int, float, float), TerrainMeshData>();

        // Async management
        private CancellationTokenSource _previewCancellationToken;
        private System.Timers.Timer _previewDebounceTimer;
        private System.Timers.Timer _lodDebounceTimer;

        // LOD management
        public TerrainLodController _lodController { get; set; }
        private int _currentLodStep = 1;

        // Camera Controller
        public CameraControllerService CameraController => _cameraController;

        // Event para notificar MainWindow de mudanças que requerem reset de câmera
        public event Action OnHeightmapChanged;

        // Bindable properties
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
        private bool isLoading = false;

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

        [ObservableProperty]
        private string lodIndicator = "LOD: 0 (step=1)";

        public ICommand GenerateTestTerrainCommand { get; }
        
        public ICommand ImportHeightmapCommand { get; set; }
        
        public ICommand ExportPackageCommand { get; set; }
        
        public ICommand ResetCameraCommand { get; set; }

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
            ImportHeightmapCommand = new RelayCommand(() => { });
            ExportPackageCommand = new RelayCommand(() => { });
            ResetCameraCommand = new RelayCommand(ResetCamera);

            // Debounce timers
            _previewDebounceTimer = new System.Timers.Timer(200);
            _previewDebounceTimer.AutoReset = false;
            _previewDebounceTimer.Elapsed += (s, e) => RegeneratePreviewAsync().Wait();

            _lodDebounceTimer = new System.Timers.Timer(150);
            _lodDebounceTimer.AutoReset = false;
            _lodDebounceTimer.Elapsed += (s, e) => ApplyLodChange();
        }

        public void InitializeViewCommands(Action onResetCamera, Action onImportHeightmap, Action onExportPackage)
        {
            ResetCameraCommand = new RelayCommand(onResetCamera);
            ImportHeightmapCommand = new RelayCommand(onImportHeightmap);
            ExportPackageCommand = new RelayCommand(onExportPackage);
        }

        public async void ImportHeightmapFromFile(string filePath)
        {
            try
            {
                IsLoading = true;
                StatusText = "Carregando heightmap...";

                var heightmap = await Task.Run(() => _importHeightmap.ImportFromFile(filePath));
                
                CurrentHeightmapOriginal = heightmap;
                CurrentHeightmap = heightmap.Clone();

                _previewCache.Clear();

                var (minHeight, maxHeight) = TerrainSettings.GetHeightRange(heightmap, VerticalScale);
                SeaLevelMin = minHeight;
                SeaLevelMax = maxHeight;
                SeaLevel = (minHeight + maxHeight) / 2f;

                IsHeightmapLoaded = true;

                StatusText = "Processando malha 3D...";
                
                InitializeLodController();
                
                await RegeneratePreviewAsync();

                HeightmapInfo = $"Full: {CurrentHeightmapOriginal.Width}x{CurrentHeightmapOriginal.Height}";
                StatusText = $"? Heightmap importado: {System.IO.Path.GetFileName(filePath)}";
                
                // Notificar MainWindow para resetar câmera
                OnHeightmapChanged?.Invoke();
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

                CurrentHeightmapOriginal = _heightmapGenerator.Generate(4096, 4096, 1.0f, Environment.TickCount);

                _previewCache.Clear();

                var (minHeight, maxHeight) = TerrainSettings.GetHeightRange(CurrentHeightmapOriginal, VerticalScale);
                SeaLevelMin = minHeight;
                SeaLevelMax = maxHeight;
                SeaLevel = (minHeight + maxHeight) / 2f;

                IsHeightmapLoaded = true;

                StatusText = "Processando malha 3D...";
                
                InitializeLodController();
                
                _ = RegeneratePreviewAsync();

                HeightmapInfo = $"Full: {CurrentHeightmapOriginal.Width}x{CurrentHeightmapOriginal.Height}";
                StatusText = "Test terrain generated";
                
                // Notificar MainWindow para resetar câmera
                OnHeightmapChanged?.Invoke();
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

        public void ExportMapPackage(string destinationFolder)
        {
            if (!IsHeightmapLoaded || CurrentHeightmapOriginal == null)
                throw new InvalidOperationException("Nenhum heightmap carregado");

            StatusText = "Exportando...";

            var heightmapForExport = CurrentHeightmapOriginal.Clone();

            if (ApplySmoothOnExport && SmoothLevel > 0)
            {
                var smoothStrength = (SmoothStrength)(int)SmoothLevel;
                heightmapForExport = _smoothingService.ApplySmooth(heightmapForExport, smoothStrength);
            }

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

            var (isValid, errorMessage) = project.Validate();
            if (!isValid)
                throw new InvalidOperationException(errorMessage);

            _exportPackage.Execute(project, heightmapForExport, destinationFolder);
            
            StatusText = "? Pacote exportado com sucesso!";
        }

        private async Task RegeneratePreviewAsync()
        {
            if (CurrentHeightmapOriginal == null || !IsHeightmapLoaded)
                return;

            IsLoading = true;
            PreviewInfo = $"Generating preview ({(int)SelectedPreviewResolution}x{(int)SelectedPreviewResolution})...";

            try
            {
                _previewCancellationToken?.Cancel();
                _previewCancellationToken = new CancellationTokenSource();
                var token = _previewCancellationToken.Token;

                await Task.Run(async () =>
                {
                    token.ThrowIfCancellationRequested();

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

                    Heightmap16Bit preview;

                    if (_previewCache.ContainsKey(SelectedPreviewResolution))
                    {
                        preview = _previewCache[SelectedPreviewResolution].Clone();
                        PreviewInfo = $"Preview: {(int)SelectedPreviewResolution}x{(int)SelectedPreviewResolution} (cached, Full export: 4096x4096)";
                    }
                    else
                    {
                        preview = _downscaleService.CreatePreview(processed, SelectedPreviewResolution);
                        _previewCache[SelectedPreviewResolution] = preview.Clone();
                        PreviewInfo = $"Preview: {(int)SelectedPreviewResolution}x{(int)SelectedPreviewResolution} (Full export: 4096x4096)";
                    }

                    token.ThrowIfCancellationRequested();

                    CurrentHeightmap = preview;

                    await RegenerateMeshWithLodAsync();
                });
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

        private void SchedulePreviewUpdate()
        {
            _previewDebounceTimer.Stop();
            _previewDebounceTimer.Start();
        }

        public void ScheduleLodUpdate()
        {
            _lodDebounceTimer.Stop();
            _lodDebounceTimer.Start();
        }

        private void ApplyLodChange()
        {
            if (_lodController == null || CurrentHeightmap == null)
                return;

            int desiredStep = _lodController.CurrentStep;

            if (desiredStep == _currentLodStep)
                return;

            _currentLodStep = desiredStep;
            LodIndicator = $"LOD: {_lodController.CurrentLodIndex} (step={_currentLodStep})";

            _ = RegenerateMeshWithLodAsync();
        }

        private async Task RegenerateMeshWithLodAsync()
        {
            if (CurrentHeightmap == null || !IsHeightmapLoaded)
                return;

            await Task.Run(() =>
            {
                var cacheKey = (_currentLodStep, SeaLevel, VerticalScale);

                if (_meshCache.ContainsKey(cacheKey))
                {
                    var cachedData = _meshCache[cacheKey];
                    TerrainMesh = cachedData.TerrainMesh;
                    WaterMesh = cachedData.WaterMesh;
                    return;
                }

                var meshData = _meshGenerator.GenerateTerrainMeshWithSeaLevel(
                    CurrentHeightmap, VerticalScale, SeaLevel, 1.0f, _currentLodStep);

                _meshCache[cacheKey] = meshData;

                TerrainMesh = meshData.TerrainMesh;
                WaterMesh = meshData.WaterMesh;
            });
        }

        public void InitializeLodController()
        {
            if (CurrentHeightmapOriginal == null)
                return;

            float terrainSize = CurrentHeightmapOriginal.Width;
            float center = terrainSize / 2f;

            var profile = new LODProfile();
            _lodController = new TerrainLodController(profile, center, center, terrainSize);
            _currentLodStep = 1;
            _meshCache.Clear();

            LodIndicator = "LOD: 0 (step=1)";
        }

        /// <summary>
        /// Reseta a câmera para orientação correta (vista de cima, horizontal)
        /// Chamado após criar/importar/resetar mapa
        /// </summary>
        public void ResetCameraView()
        {
            // Will be called from MainWindow with HelixViewport3D reference
            // See MainWindow.xaml.cs for implementation
        }

        partial void OnVerticalScaleChanged(float oldValue, float newValue)
        {
            if (CurrentHeightmap != null && IsHeightmapLoaded)
            {
                var (minHeight, maxHeight) = TerrainSettings.GetHeightRange(CurrentHeightmap, newValue);
                SeaLevelMin = minHeight;
                SeaLevelMax = maxHeight;
                SeaLevel = TerrainSettings.ClampSeaLevel(SeaLevel, CurrentHeightmap, newValue);

                _ = RegenerateMeshWithLodAsync();
            }
        }

        partial void OnSeaLevelChanged(float oldValue, float newValue)
        {
            SeaLevelInfo = $"Sea Level: {newValue:F1} m";

            if (CurrentHeightmap != null && IsHeightmapLoaded)
            {
                _ = RegenerateMeshWithLodAsync();
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

    public class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _execute?.Invoke();
    }
}
