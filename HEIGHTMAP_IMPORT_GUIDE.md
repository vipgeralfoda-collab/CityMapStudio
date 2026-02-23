# CityMapStudio - Importação de Heightmap PNG 16-bit

## Pipeline de Importação Implementado

```
PNG 16-bit Grayscale (arquivo real)
   ?
PngHeightmap16BitReader (Infrastructure.IO)
   ?
Heightmap16Bit (Domain.Models)
   ?
ImportHeightmapFromPng (Infrastructure.IO)
   ?
TerrainMeshGenerator (Rendering3D)
   ?
HelixViewport3D (UI.WPF)
```

## Arquitetura

### Domain (CityMapStudio.Domain)
- **Heightmap16Bit.cs**
  - Width, Height, ushort[] Data
  - GetHeight(x, y) / SetHeight(x, y)

### Infrastructure (CityMapStudio.Infrastructure)
- **PngHeightmap16BitReader.cs**
  - Lê arquivos PNG usando SixLabors.ImageSharp
  - Valida formato: L16 (16-bit grayscale)
  - Rejeita PNG 8-bit com erro claro
  - Validações: mínimo 2x2, máximo 4096x4096

- **ImportHeightmapFromPng.cs**
  - Use case de importação
  - Orquestra leitura e validações
  - Tratamento de erros com mensagens amigáveis

### Rendering3D (CityMapStudio.Rendering3D)
- **TerrainMeshGenerator.cs**
  - Já existente, compatível com qualquer resolução
  - Gera mesh com topo + 4 laterais

### UI (CityMapStudio.UI.Wpf)
- **MainViewModel.cs**
  - ICommand ImportHeightmapCommand
  - Método ImportHeightmapFromFile(filePath)
  - Atualiza terreno em tempo real

- **MainWindow.xaml**
  - Botão "Import Heightmap (PNG 16-bit)" na toolbar

## Usando o Importador

1. Abrir CityMapStudio
2. Clicar "Import Heightmap (PNG 16-bit)"
3. Selecionar um arquivo PNG 16-bit grayscale
4. Terreno 3D é atualizado imediatamente
5. Vertical Scale slider funciona normalmente

## Validações Implementadas

? Arquivo existe?
? É PNG?
? Formato L16 (16-bit grayscale)?
? Dimensões mínimas (2x2)?
? Dimensões máximas (4096x4096)?
? Mensagens de erro claras

## Exemplo de Uso Programático

```csharp
var reader = new PngHeightmap16BitReader();
var importer = new ImportHeightmapFromPng(reader);
var heightmap = importer.ImportFromFile("terrain.png");

var generator = new TerrainMeshGenerator();
var mesh = generator.GenerateTerrainMesh(heightmap, verticalScale: 50f, cellSize: 1.0f);
```

## Testando com PNG 16-bit

Para criar um PNG 16-bit teste, use ferramentas como:
- GDAL: `gdal_translate -of PNG -ot UInt16 input.tif output.png`
- Python PIL/Pillow
- Photoshop (salvar como 16-bit PNG)
- QGIS
- Terrain.Party ou similar

Heightmaps recomendados:
- Resolução: 256x256, 512x512, 1024x1024
- Origem: SRTM, DEM, Mapbox Terrain, etc.

## Tratamento de Erros

PNG 8-bit:
```
Import Error: Arquivo é PNG 8-bit (grayscale). Use PNG 16-bit (grayscale).
```

Arquivo muito grande:
```
Import Error: Erro ao importar heightmap: Dimensões muito grandes: 5000x5000. Máximo: 4096x4096
```

Arquivo inválido:
```
Import Error: Arquivo não encontrado: /caminho/inexistente.png
```

## Próximos Passos (Opcional)

- ? Reamostragem automática para PNG > 4096x4096
- ? Suporte a GeoTIFF
- ? Preview em OpenFileDialog
- ? Histórico de arquivos recentes
- ? Drag & drop de PNG no viewport
