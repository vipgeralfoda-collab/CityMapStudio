using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using CityMapStudio.UI.Wpf.ViewModels;
using CityMapStudio.Domain.Models;
using CityMapStudio.Application.Services;
using HelixToolkit.Wpf;

namespace CityMapStudio.UI.Wpf
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private EditorState _editorState;
        private TerrainBrushMode _currentTerrainMode = TerrainBrushMode.Raise;
        private Point? _lastMousePos;
        private bool _isDrawing = false;
        private HelixViewport3D _viewport3D;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = new MainViewModel();
            _editorState = new EditorState();

            _viewport3D = FindName("Viewport3D") as HelixViewport3D;

            if (_viewport3D != null)
            {
                // Mouse interaction para edição
                _viewport3D.MouseDown += Viewport_MouseDown;
                _viewport3D.MouseMove += Viewport_MouseMove;
                _viewport3D.MouseUp += Viewport_MouseUp;

                Action onResetCamera = () =>
                {
                    _viewport3D.ResetCamera();
                    ResetCameraToTopView(_viewport3D);
                };

                Action onImportHeightmap = () =>
                {
                    var dialog = new OpenFileDialog
                    {
                        Filter = "PNG 16-bit Grayscale (*.png)|*.png|All files (*.*)|*.*",
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        Title = "Import Heightmap (PNG 16-bit Grayscale)"
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        _viewModel.ImportHeightmapFromFile(dialog.FileName);
                    }
                };

                Action onExportPackage = () =>
                {
                    if (!_viewModel.IsHeightmapLoaded)
                    {
                        MessageBox.Show(
                            "Erro: Nenhum heightmap carregado.\n\nImporte um PNG 16-bit antes de exportar.",
                            "Export Package",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    var folderDialog = new System.Windows.Forms.FolderBrowserDialog
                    {
                        Description = "Selecione a pasta para exportar o pacote de mapa"
                    };

                    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            _viewModel.ExportMapPackage(folderDialog.SelectedPath);
                            MessageBox.Show(
                                "Pacote exportado com sucesso!",
                                "Export Package",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(
                                $"Erro ao exportar: {ex.Message}",
                                "Export Package",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                };

                _viewModel.InitializeViewCommands(onResetCamera, onImportHeightmap, onExportPackage);

                // Monitorar mudanças de câmera para LOD
                _viewport3D.CameraChanged += (s, e) =>
                {
                    if (_viewModel?.IsHeightmapLoaded == true && _viewModel?._lodController != null)
                    {
                        var cameraPos = _viewport3D.Camera.Position;
                        if (_viewModel._lodController.UpdateLod(cameraPos))
                        {
                            _viewModel.ScheduleLodUpdate();
                        }
                    }
                };
            }

            DataContext = _viewModel;
            
            // Assinar evento de mudança de heightmap
            _viewModel.OnHeightmapChanged += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (_viewport3D != null)
                    {
                        ResetCameraToTopView(_viewport3D);
                    }
                });
            };
            
            // Inicializar inspector com Terrain selecionado
            ShowTerrainInspector();
            
            // Inicializar inspector
            ShowTerrainInspector();
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is string category)
            {
                switch (category)
                {
                    case "Terrain":
                        ShowTerrainInspector();
                        break;
                    case "Water":
                        ShowWaterInspector();
                        break;
                    case "Resources":
                        ShowResourcesInspector();
                        break;
                    case "Utilities":
                        ShowUtilitiesInspector();
                        break;
                }
            }
        }

        private void ShowTerrainInspector()
        {
            var panel = FindName("InspectorPanel") as StackPanel;
            if (panel == null) return;

            panel.Children.Clear();

            var title = new TextBlock
            {
                Text = (string)FindResource("TerrainBrush"),
                Foreground = System.Windows.Media.Brushes.LimeGreen,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(10, 10, 10, 8)
            };
            panel.Children.Add(title);

            AddSection(panel, (string)FindResource("BrushMode"));
            var modeOptions = new[] 
            { 
                ((string)FindResource("Raise"), TerrainBrushMode.Raise),
                ((string)FindResource("Lower"), TerrainBrushMode.Lower),
                ((string)FindResource("Smooth"), TerrainBrushMode.Smooth),
                ((string)FindResource("Flatten"), TerrainBrushMode.Flatten),
                ((string)FindResource("SetHeight"), TerrainBrushMode.SetHeight),
                ((string)FindResource("Noise"), TerrainBrushMode.Noise)
            };
            
            var modePanel = new StackPanel { Margin = new Thickness(5, 2, 5, 8) };
            
            for (int i = 0; i < modeOptions.Length; i++)
            {
                if (i % 2 == 0)
                {
                    var rowPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 1, 0, 1) };
                    
                    var btn1 = new Button
                    {
                        Content = modeOptions[i].Item1,
                        Tag = modeOptions[i].Item2,
                        Margin = new Thickness(2, 0, 1, 0),
                        Padding = new Thickness(5, 4, 5, 4),
                        Background = System.Windows.Media.Brushes.Transparent,
                        Foreground = System.Windows.Media.Brushes.LightGray,
                        BorderThickness = new Thickness(1),
                        BorderBrush = System.Windows.Media.Brushes.Gray,
                        FontSize = 9,
                        Width = 145,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    btn1.Click += (s, e) => SelectTerrainMode((TerrainBrushMode)btn1.Tag);
                    rowPanel.Children.Add(btn1);
                    
                    if (i + 1 < modeOptions.Length)
                    {
                        var btn2 = new Button
                        {
                            Content = modeOptions[i + 1].Item1,
                            Tag = modeOptions[i + 1].Item2,
                            Margin = new Thickness(1, 0, 2, 0),
                            Padding = new Thickness(5, 4, 5, 4),
                            Background = System.Windows.Media.Brushes.Transparent,
                            Foreground = System.Windows.Media.Brushes.LightGray,
                            BorderThickness = new Thickness(1),
                            BorderBrush = System.Windows.Media.Brushes.Gray,
                            FontSize = 9,
                            Width = 145,
                            HorizontalAlignment = HorizontalAlignment.Left
                        };
                        btn2.Click += (s, e) => SelectTerrainMode((TerrainBrushMode)btn2.Tag);
                        rowPanel.Children.Add(btn2);
                    }
                    
                    modePanel.Children.Add(rowPanel);
                }
            }
            panel.Children.Add(modePanel);

            AddSection(panel, (string)FindResource("Radius"));
            AddSlider(panel, 10, 256, 64, (v) => _editorState.BrushSettings.Radius = (int)v);

            AddSection(panel, (string)FindResource("Strength"));
            AddSlider(panel, 0.1f, 1.0f, 0.5f, (v) => _editorState.BrushSettings.Strength = v);

            AddSection(panel, (string)FindResource("Falloff"));
            var falloffOptions = new[] { "Linear", "Soft", "Sharp" };
            foreach (var falloff in falloffOptions)
            {
                var opt = new RadioButton
                {
                    Content = FindResource(falloff),
                    Margin = new Thickness(8, 1, 8, 1),
                    Foreground = System.Windows.Media.Brushes.LightGray,
                    FontSize = 9,
                    Tag = falloff
                };
                opt.Checked += (s, e) => SelectFalloff(falloff);
                panel.Children.Add(opt);
            }
        }

        private void ShowWaterInspector()
        {
            var panel = FindName("InspectorPanel") as StackPanel;
            if (panel == null) return;

            panel.Children.Clear();

            var title = new TextBlock
            {
                Text = (string)FindResource("WaterTools"),
                Foreground = System.Windows.Media.Brushes.LimeGreen,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(10, 10, 10, 8)
            };
            panel.Children.Add(title);

            AddSection(panel, (string)FindResource("SeaLevel"));
            AddSlider(panel, 0f, 500f, _viewModel.SeaLevel, (v) => _viewModel.SeaLevel = v);

            AddSection(panel, (string)FindResource("Tools"));
            var toolLabels = new[] 
            { 
                ((string)FindResource("Set"), "Set"),
                ((string)FindResource("Raise"), "Raise"),
                ((string)FindResource("Lower"), "Lower"),
                ((string)FindResource("Flatten"), "Flatten")
            };
            var toolPanel = new StackPanel { Margin = new Thickness(5, 2, 5, 8) };
            
            for (int i = 0; i < toolLabels.Length; i++)
            {
                if (i % 2 == 0)
                {
                    var rowPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 1, 0, 1) };
                    
                    var btn1 = new Button
                    {
                        Content = toolLabels[i].Item1,
                        Tag = toolLabels[i].Item2,
                        Margin = new Thickness(2, 0, 1, 0),
                        Padding = new Thickness(5, 4, 5, 4),
                        Background = System.Windows.Media.Brushes.Transparent,
                        Foreground = System.Windows.Media.Brushes.LightGray,
                        BorderThickness = new Thickness(1),
                        BorderBrush = System.Windows.Media.Brushes.Gray,
                        FontSize = 9,
                        Width = 145,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    btn1.Click += (s, e) => SelectWaterTool((string)btn1.Tag);
                    rowPanel.Children.Add(btn1);
                    
                    if (i + 1 < toolLabels.Length)
                    {
                        var btn2 = new Button
                        {
                            Content = toolLabels[i + 1].Item1,
                            Tag = toolLabels[i + 1].Item2,
                            Margin = new Thickness(1, 0, 2, 0),
                            Padding = new Thickness(5, 4, 5, 4),
                            Background = System.Windows.Media.Brushes.Transparent,
                            Foreground = System.Windows.Media.Brushes.LightGray,
                            BorderThickness = new Thickness(1),
                            BorderBrush = System.Windows.Media.Brushes.Gray,
                            FontSize = 9,
                            Width = 145,
                            HorizontalAlignment = HorizontalAlignment.Left
                        };
                        btn2.Click += (s, e) => SelectWaterTool((string)btn2.Tag);
                        rowPanel.Children.Add(btn2);
                    }
                    
                    toolPanel.Children.Add(rowPanel);
                }
            }
            panel.Children.Add(toolPanel);
        }

        private void SelectWaterTool(string tool)
        {
            _editorState.CurrentCategory = EditorCategory.Water;
            _viewModel.StatusText = $"Editando: Água - {tool}";
        }

        private void ShowResourcesInspector()
        {
            var panel = FindName("InspectorPanel") as StackPanel;
            if (panel == null) return;

            panel.Children.Clear();

            var title = new TextBlock
            {
                Text = "RESOURCES",
                Foreground = System.Windows.Media.Brushes.LimeGreen,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(10, 10, 10, 8)
            };
            panel.Children.Add(title);

            AddSection(panel, "Type");
            var resources = new[] { "Ore", "Oil", "Fertile", "Water" };
            var resPanel = new StackPanel { Margin = new Thickness(5, 2, 5, 8) };
            
            for (int i = 0; i < resources.Length; i++)
            {
                if (i % 2 == 0)
                {
                    var rowPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 1, 0, 1) };
                    
                    var btn1 = new Button
                    {
                        Content = resources[i],
                        Margin = new Thickness(2, 0, 1, 0),
                        Padding = new Thickness(5, 4, 5, 4),
                        Background = System.Windows.Media.Brushes.Transparent,
                        Foreground = System.Windows.Media.Brushes.LightGray,
                        BorderThickness = new Thickness(1),
                        BorderBrush = System.Windows.Media.Brushes.Gray,
                        FontSize = 9,
                        Width = 145,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    rowPanel.Children.Add(btn1);
                    
                    if (i + 1 < resources.Length)
                    {
                        var btn2 = new Button
                        {
                            Content = resources[i + 1],
                            Margin = new Thickness(1, 0, 2, 0),
                            Padding = new Thickness(5, 4, 5, 4),
                            Background = System.Windows.Media.Brushes.Transparent,
                            Foreground = System.Windows.Media.Brushes.LightGray,
                            BorderThickness = new Thickness(1),
                            BorderBrush = System.Windows.Media.Brushes.Gray,
                            FontSize = 9,
                            Width = 145,
                            HorizontalAlignment = HorizontalAlignment.Left
                        };
                        rowPanel.Children.Add(btn2);
                    }
                    
                    resPanel.Children.Add(rowPanel);
                }
            }
            panel.Children.Add(resPanel);

            AddSection(panel, "Density");
            AddSlider(panel, 0f, 1.0f, 0.5f);
        }

        private void ShowUtilitiesInspector()
        {
            var panel = FindName("InspectorPanel") as StackPanel;
            if (panel == null) return;

            panel.Children.Clear();

            var title = new TextBlock
            {
                Text = "UTILITIES",
                Foreground = System.Windows.Media.Brushes.LimeGreen,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(10, 10, 10, 8)
            };
            panel.Children.Add(title);

            AddSection(panel, "Edit");
            var buttons = new[] { "Undo", "Redo", "Reset", "Flatten" };
            var btnPanel = new StackPanel { Margin = new Thickness(5, 2, 5, 8) };
            
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i % 2 == 0)
                {
                    var rowPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 1, 0, 1) };
                    
                    var btn1 = new Button
                    {
                        Content = buttons[i],
                        Margin = new Thickness(2, 0, 1, 0),
                        Padding = new Thickness(5, 4, 5, 4),
                        Background = System.Windows.Media.Brushes.Transparent,
                        Foreground = System.Windows.Media.Brushes.LightGray,
                        BorderThickness = new Thickness(1),
                        BorderBrush = System.Windows.Media.Brushes.Gray,
                        FontSize = 9,
                        Width = 145,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    rowPanel.Children.Add(btn1);
                    
                    if (i + 1 < buttons.Length)
                    {
                        var btn2 = new Button
                        {
                            Content = buttons[i + 1],
                            Margin = new Thickness(1, 0, 2, 0),
                            Padding = new Thickness(5, 4, 5, 4),
                            Background = System.Windows.Media.Brushes.Transparent,
                            Foreground = System.Windows.Media.Brushes.LightGray,
                            BorderThickness = new Thickness(1),
                            BorderBrush = System.Windows.Media.Brushes.Gray,
                            FontSize = 9,
                            Width = 145,
                            HorizontalAlignment = HorizontalAlignment.Left
                        };
                        rowPanel.Children.Add(btn2);
                    }
                    
                    btnPanel.Children.Add(rowPanel);
                }
            }
            panel.Children.Add(btnPanel);

            AddSection(panel, "View");
            var viewOptions = new[] { "Grid", "Wireframe" };
            foreach (var opt in viewOptions)
            {
                var chk = new CheckBox
                {
                    Content = opt,
                    Margin = new Thickness(8, 2, 8, 2),
                    Foreground = System.Windows.Media.Brushes.LightGray,
                    FontSize = 9
                };
                panel.Children.Add(chk);
            }
        }

        private void AddSection(StackPanel panel, string title)
        {
            var section = new TextBlock
            {
                Text = title.ToUpper(),
                Foreground = System.Windows.Media.Brushes.Gray,
                FontSize = 9,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(10, 8, 10, 3)
            };
            panel.Children.Add(section);
        }

        private void AddSlider(StackPanel panel, float min, float max, float value, Action<float> onChanged = null)
        {
            var slider = new Slider
            {
                Minimum = min,
                Maximum = max,
                Value = value,
                Margin = new Thickness(8, 2, 8, 6),
                Background = System.Windows.Media.Brushes.Transparent,
                Foreground = System.Windows.Media.Brushes.Gray,
                Height = 18
            };
            
            if (onChanged != null)
            {
                slider.ValueChanged += (s, e) => onChanged((float)e.NewValue);
            }
            
            panel.Children.Add(slider);

            var valueText = new TextBlock
            {
                Text = $"{value:F1}",
                Margin = new Thickness(8, 0, 8, 4),
                Foreground = System.Windows.Media.Brushes.LightGray,
                FontSize = 9,
                TextAlignment = TextAlignment.Right
            };
            panel.Children.Add(valueText);
        }

        // Métodos de seleção de modo
        private void SelectTerrainMode(TerrainBrushMode mode)
        {
            _currentTerrainMode = mode;
            _editorState.CurrentTerrainMode = mode;
        }

        private void SelectFalloff(string falloff)
        {
            _editorState.BrushSettings.Falloff = falloff switch
            {
                "Linear" => BrushFalloff.Linear,
                "Soft" => BrushFalloff.Smooth,
                "Sharp" => BrushFalloff.Sharp,
                _ => BrushFalloff.Smooth
            };
        }

        // ===== VIEWPORT INTERACTION (EDIÇÃO) =====
        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_viewModel.IsHeightmapLoaded || _editorState.CurrentCategory != EditorCategory.Terrain)
                return;

            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            _isDrawing = true;
            _lastMousePos = e.GetPosition(sender as IInputElement);
            
            ApplyBrush(e.GetPosition(sender as IInputElement));
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDrawing || _lastMousePos == null)
                return;

            var currentPos = e.GetPosition(sender as IInputElement);
            
            // Spacing: só aplicar se o cursor percorreu X pixels
            double distance = Math.Sqrt(
                Math.Pow(currentPos.X - _lastMousePos.Value.X, 2) +
                Math.Pow(currentPos.Y - _lastMousePos.Value.Y, 2)
            );

            if (distance > _editorState.BrushSettings.Spacing)
            {
                ApplyBrush(currentPos);
                _lastMousePos = currentPos;
            }
        }

        private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
            _lastMousePos = null;

            // Aplicar constraints de câmera ao final da interação
            if (_viewport3D?.Camera is System.Windows.Media.Media3D.PerspectiveCamera camera && 
                _viewModel?.CurrentHeightmapOriginal != null)
            {
                float mapWidth = _viewModel.CurrentHeightmapOriginal.Width;
                float mapHeight = _viewModel.CurrentHeightmapOriginal.Height;
                float terrainMaxHeight = (float)_viewModel.VerticalScale * 2;
                
                _viewModel.CameraController.ApplyCameraConstraints(
                    _viewport3D, 
                    mapWidth, 
                    mapHeight, 
                    terrainMaxHeight
                );
            }
        }

        private void ApplyBrush(Point screenPos)
        {
            // TODO: Implementar hit test real com HelixViewport3D
            // Por agora, placebo para demonstrar
            
            // Feedback no status bar
            _viewModel.StatusText = $"Editando: {_currentTerrainMode}";
        }

        /// <summary>
        /// Reseta a câmera para vista de cima (top-down) com orientação correta
        /// Chamado após criar/importar mapa ou ao pressionar Reset Camera
        /// </summary>
        private void ResetCameraToTopView(HelixViewport3D viewport)
        {
            if (_viewModel?.CurrentHeightmapOriginal == null || viewport?.Camera is not System.Windows.Media.Media3D.PerspectiveCamera camera)
                return;

            float mapWidth = _viewModel.CurrentHeightmapOriginal.Width;
            float mapHeight = _viewModel.CurrentHeightmapOriginal.Height;
            float terrainMaxHeight = (float)_viewModel.VerticalScale * 2;

            _viewModel.CameraController.ResetCameraToTopView(viewport, mapWidth, mapHeight, terrainMaxHeight);
        }
    }
}
