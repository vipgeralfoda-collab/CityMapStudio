using System;
using System.Windows;
using Microsoft.Win32;
using CityMapStudio.UI.Wpf.ViewModels;
using HelixToolkit.Wpf;

namespace CityMapStudio.UI.Wpf
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = new MainViewModel();

            var viewport3D = FindName("Viewport3D") as HelixViewport3D;

            if (viewport3D != null)
            {
                // Criar as ações para os comandos
                Action onResetCamera = () => viewport3D.ResetCamera();

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
                                "Pacote exportado com sucesso!\n\nProcure a pasta com o nome do seu mapa na pasta selecionada.",
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

                // Inicializar comandos ANTES de setar DataContext
                _viewModel.InitializeViewCommands(onResetCamera, onImportHeightmap, onExportPackage);
            }

            // Setar DataContext DEPOIS de inicializar os comandos
            DataContext = _viewModel;
        }

        private void ImportHeightmapButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel?.ImportHeightmapCommand?.Execute(null);
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
