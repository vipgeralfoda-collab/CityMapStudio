# ?? REFERÊNCIA TÉCNICA - PNG 16-bit Import

## Classes e Interfaces

### Infrastructure.IO

#### IPngHeightmap16BitReader
```csharp
public interface IPngHeightmap16BitReader
{
    Heightmap16Bit Read(string filePath);
}
```

#### PngHeightmap16BitReader
```csharp
public class PngHeightmap16BitReader : IPngHeightmap16BitReader
{
    public Heightmap16Bit Read(string filePath)
    {
        // 1. Valida: arquivo existe
        // 2. Valida: é PNG
        // 3. Carrega com ImageSharp
        // 4. Valida: formato L16
        // 5. Valida: dimensões (2x2 a 4096x4096)
        // 6. ProcessPixelRows(): lê pixels
        // 7. Retorna Heightmap16Bit
    }
}
```

**Exceções Possíveis:**
- `FileNotFoundException`: Arquivo não existe
- `ArgumentException`: Não é .png
- `InvalidOperationException`: Não é L16, dimensões inválidas

---

#### IImportHeightmapFromPng
```csharp
public interface IImportHeightmapFromPng
{
    Heightmap16Bit ImportFromFile(string filePath);
}
```

#### ImportHeightmapFromPng
```csharp
public class ImportHeightmapFromPng : IImportHeightmapFromPng
{
    private readonly IPngHeightmap16BitReader _reader;
    
    public Heightmap16Bit ImportFromFile(string filePath)
    {
        // 1. Valida: path não vazio
        // 2. Chama reader.Read()
        // 3. Valida: dimensões mínimas
        // 4. Retorna Heightmap16Bit
        // 5. Trata exceções com contexto
    }
}
```

---

### Application Layer

#### IHeightmapGenerator
```csharp
public interface IHeightmapGenerator
{
    Heightmap16Bit Generate(int width, int height, float scale, int seed = 0);
}
```

**Mantido para teste procedural (não modificado).**

---

### Domain Layer

#### Heightmap16Bit
```csharp
public class Heightmap16Bit
{
    public int Width { get; set; }
    public int Height { get; set; }
    public ushort[] Data { get; set; }
    
    public ushort GetHeight(int x, int y) { ... }
    public void SetHeight(int x, int y, ushort value) { ... }
}
```

---

### UI.WPF ViewModels

#### MainViewModel
```csharp
public partial class MainViewModel : ObservableObject
{
    // Properties
    [ObservableProperty] MeshGeometry3D TerrainMesh;
    [ObservableProperty] float VerticalScale = 50f;
    [ObservableProperty] string StatusText = "Ready";
    [ObservableProperty] string HeightmapInfo = "No heightmap loaded";
    [ObservableProperty] Heightmap16Bit CurrentHeightmap;
    
    // Commands
    public ICommand GenerateTestTerrainCommand { get; }
    public ICommand ImportHeightmapCommand { get; set; }
    public ICommand ResetCameraCommand { get; set; }
    
    // Methods
    public void ImportHeightmapFromFile(string filePath)
    {
        try
        {
            StatusText = "Importing...";
            var heightmap = _importHeightmap.ImportFromFile(filePath);
            CurrentHeightmap = heightmap;
            TerrainMesh = _meshGenerator.GenerateTerrainMesh(
                CurrentHeightmap, VerticalScale, 1.0f);
            StatusText = $"Heightmap imported: {Path.GetFileName(filePath)}";
        }
        catch (Exception ex)
        {
            StatusText = $"Import Error: {ex.Message}";
        }
    }
}
```

---

## XAML Bindings

### MainWindow.xaml
```xaml
<Button Command="{Binding ImportHeightmapCommand}" 
        Content="Import Heightmap (PNG 16-bit)" 
        Background="#2196F3"
        Foreground="White"/>
```

---

## Code-Behind

### MainWindow.xaml.cs
```csharp
private void MainWindow_Loaded(object sender, RoutedEventArgs e)
{
    _viewModel = new MainViewModel();
    DataContext = _viewModel;
    
    var viewport3D = FindName("Viewport3D") as HelixViewport3D;
    
    _viewModel.ImportHeightmapCommand = new RelayCommand(() =>
    {
        var dialog = new OpenFileDialog
        {
            Filter = "PNG 16-bit Grayscale (*.png)|*.png|All files (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments),
            Title = "Import Heightmap (PNG 16-bit Grayscale)"
        };
        
        if (dialog.ShowDialog() == true)
        {
            _viewModel.ImportHeightmapFromFile(dialog.FileName);
        }
    });
}
```

---

## Mensagens de Status

| Situação | StatusText |
|----------|-----------|
| Pronto | "Ready" |
| Importando | "Importing..." |
| Sucesso | "Heightmap imported: filename.png" |
| PNG 8-bit | "Import Error: Arquivo é PNG 8-bit (grayscale). Use PNG 16-bit (grayscale)." |
| Formato inválido | "Import Error: Erro ao importar heightmap: Formato PNG não suportado. Use grayscale 16-bit." |
| Arquivo grande | "Import Error: Erro ao importar heightmap: Dimensões muito grandes: 5000x5000. Máximo: 4096x4096" |
| Arquivo não existe | "Import Error: Arquivo não encontrado: /path/file.png" |
| Gerando teste | "Generating..." |
| Teste gerado | "Test terrain generated" |

---

## Performance: Processamento de Pixel

```csharp
// ? LENTO (alocação por linha)
for (int y = 0; y < height; y++)
    for (int x = 0; x < width; x++)
        ushort value = pixels[y * width + x];

// ? RÁPIDO (stack allocation com Span)
image.ProcessPixelRows(accessor =>
{
    for (int y = 0; y < height; y++)
    {
        Span<L16> pixelRow = accessor.GetRowSpan(y);
        for (int x = 0; x < width; x++)
        {
            ushort value = pixelRow[x].PackedValue;
        }
    }
});
```

**Performance:** ~2-3x mais rápido com ProcessPixelRows.

---

## Validação: Checklist

```
Arquivo PNG 16-bit:
? Existe? (File.Exists)
? .png extension? (EndsWith)
? ImageSharp consegue carregar?
? Formato L16? (Image<L16>)
? Grayscale? (not L8, RGBA, etc)
? Dimensões >= 2x2?
? Dimensões <= 4096x4096?
? Pixels lidos corretamente?

PASS ? ? Heightmap16Bit criado
FAIL ? ? InvalidOperationException com mensagem clara
```

---

## Ciclo de Vida: Importar PNG

```
1. User Interaction (WPF)
   ?
2. OpenFileDialog ? filePath
   ?
3. MainViewModel.ImportHeightmapFromFile(filePath)
   ?? StatusText = "Importing..."
   ?? Call: _importHeightmap.ImportFromFile(filePath)
   ?  ?? PngHeightmap16BitReader.Read()
   ?  ?  ?? Valida arquivo
   ?  ?  ?? Image.Load() ImageSharp
   ?  ?  ?? Valida L16
   ?  ?  ?? ProcessPixelRows()
   ?  ?  ?? Return Heightmap16Bit
   ?  ?? Return Heightmap16Bit
   ?? CurrentHeightmap = heightmap
   ?? TerrainMesh = meshGenerator.GenerateTerrainMesh()
   ?? StatusText = "Heightmap imported: ..."
   ?? HeightmapInfo = "512x512"
   ?? Catch Exception ? StatusText = error
   ?
4. MVVM Binding Updates UI
   ?? TerrainMesh ? HelixViewport3D.Geometry
   ?? StatusText ? TextBlock
   ?? HeightmapInfo ? TextBlock
   ?
5. HelixViewport3D Renders
   ?? MeshGeometry3D visualizado
   ?? Terreno 3D em verde
   ?? User interacts (orbit, zoom, pan)
```

---

## SixLabors.ImageSharp: Tipos

```csharp
// Pixel Formats
Image<Rgba32>    // RGBA 32-bit (não suportado)
Image<Rgb24>     // RGB 24-bit (não suportado)
Image<L8>        // Grayscale 8-bit (REJEITADO)
Image<L16>       // Grayscale 16-bit (ACEITO ?)

// Leitura de Pixels
var image = Image.Load<L16>("heightmap.png");

image.ProcessPixelRows(accessor =>
{
    for (int y = 0; y < image.Height; y++)
    {
        Span<L16> row = accessor.GetRowSpan(y);
        for (int x = 0; x < image.Width; x++)
        {
            L16 pixel = row[x];
            ushort value = pixel.PackedValue;  // 0-65535
        }
    }
});
```

---

## Conversão Automática

```csharp
// Se não for L16 nativo, tentar converter
if (image is not Image<L16> image16Bit)
{
    if (image is Image<Rgba32> imageRgba)
    {
        // Converter RGBA ? L16 (média dos canais)
        var converted = image.CloneAs<L16>();
        return ReadFromImage(converted);
    }
    else if (image is Image<L8> image8Bit)
    {
        // Rejeitar explicitamente
        throw new InvalidOperationException(
            "Arquivo é PNG 8-bit...");
    }
    else
    {
        throw new InvalidOperationException(
            "Formato PNG não suportado...");
    }
}
```

---

## Testes Recomendados

### Teste 1: PNG 16-bit Válido
```bash
python3 generate_test_heightmap.py --size 512 --output test.png
# Resultado esperado: Terreno renderizado
```

### Teste 2: PNG 8-bit (deve falhar)
```bash
convert heightmap.png -depth 8 test_8bit.png
# Resultado esperado: Erro "Use PNG 16-bit"
```

### Teste 3: Imagem RGBA
```bash
python3 -c "from PIL import Image; \
    img = Image.new('RGBA', (512, 512)); \
    img.save('test_rgba.png')"
# Resultado esperado: Processado ou erro claro
```

### Teste 4: Arquivo Grande
```bash
python3 generate_test_heightmap.py --size 5000
# Resultado esperado: Erro "Dimensões muito grandes"
```

---

## Debugging: Verificar Tipo PNG

```bash
# Linux/Mac
file heightmap.png
# Output: PNG image data, 512 x 512, 16-bit grayscale, non-interlaced

# Python
from PIL import Image
img = Image.open("heightmap.png")
print(img.mode)      # 'I' for 16-bit, 'L' for 8-bit
print(img.size)      # (width, height)
print(img.dtype)     # uint16 or uint8

# ImageSharp (C#)
using var image = Image.Load("heightmap.png");
Console.WriteLine($"Type: {image.GetType().Name}");
// Output: Image`1[[SixLabors.ImageSharp.PixelFormats.L16]]
```

---

## Dicas de Performance

1. **Use ProcessPixelRows** não acesso direto
2. **Valide dimensões antes** de alocar Data[]
3. **Cache Heightmap16Bit** se reutilizar
4. **Gere Mesh uma vez** por heightmap
5. **Use slider para VerticalScale** sem regenerar pixels

---

## Troubleshooting

| Problema | Causa | Solução |
|----------|-------|--------|
| "Arquivo não encontrado" | Path incorreto | Verificar path |
| "PNG 8-bit" | Arquivo é 8-bit | Converter com GDAL |
| "Formato não suportado" | RGBA/RGB | Usar grayscale |
| "Dimensões muito grandes" | > 4096x4096 | Redimensionar |
| Terreno vazio | PNG com valores 0 | Gerar com script |
| Aplicação lenta | PNG 4096x4096 | Usar 2048x2048 |
| Memoria insuficiente | PNG muito grande | Reduzir resolução |

---

## Referências

- **SixLabors.ImageSharp**: https://docs.sixlabors.com/articles/imagesharp/
- **Helix Toolkit**: http://helix-toolkit.org/
- **Clean Architecture**: https://blog.cleancoder.com/
- **MVVM Pattern**: https://www.mvvmlight.net/

---

**Documentação Técnica Completa!** ?
