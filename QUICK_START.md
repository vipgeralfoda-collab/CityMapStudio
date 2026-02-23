# ?? QUICK START - Importação de Heightmap PNG 16-bit

## ? O que foi implementado?

A aplicação CityMapStudio agora suporta importação de **PNG 16-bit grayscale** para gerar terrenos 3D.

---

## ?? Como Usar (Passo a Passo)

### 1?? Obter um PNG 16-bit

**Opção A: Gerar automaticamente**
```bash
python3 generate_test_heightmap.py --size 512 --output test_terrain.png
```

Requisitos:
- Python 3.x
- `pip install Pillow numpy`

Resultado: `test_terrain.png` (512x512, terreno procedural)

---

**Opção B: Converter de arquivo existente**

GDAL (Linux/Mac):
```bash
gdal_translate -of PNG -ot UInt16 input.tif output.png
```

QGIS (Gráfico):
1. Abrir raster
2. Raster ? Convert ? PNG
3. Salvar como 16-bit

---

**Opção C: Baixar online**

Fontes:
- SRTM: https://lpdaac.usgs.gov/products/srtmgl1v003/
- Mapbox Terrain: https://www.mapbox.com/
- Terrain.Party: https://terrain.party/

---

### 2?? Abrir CityMapStudio

```
dotnet run --project CityMapStudio.UI.Wpf
```

Você verá:
- Toolbar no topo com 2 botões
- Viewport 3D vazio (fundo cinza)
- Painel de controles à direita

---

### 3?? Importar Heightmap PNG

1. **Clique** no botão azul: **"Import Heightmap (PNG 16-bit)"**
2. **Abre** OpenFileDialog
3. **Selecione** seu arquivo PNG 16-bit
4. **Clique** "Abrir"

---

### 4?? Ver Terreno 3D

O terreno aparece imediatamente com:
- ? Cor verde
- ? Dimensões exibidas (ex: "512x512")
- ? Status: "Heightmap imported: test_terrain.png"

---

### 5?? Interagir com o Terreno

**Orbit (Girar)**
- Botão direito do mouse + arrastar

**Zoom In/Out**
- Scroll do mouse (roda)

**Pan (Mover)**
- Botão do meio do mouse + arrastar

**Vertical Scale (Altura)**
- Mova slider à direita: 10m a 200m
- Terreno se regenera em tempo real

**Reset Camera**
- Clique botão "Reset Camera" para voltar à vista inicial

---

## ?? Exemplos de Teste

### Teste 1: Terreno Procedural
```bash
python3 generate_test_heightmap.py --size 256 --type perlin
# Resultado: procedural.png (256x256, ondulações)
```

### Teste 2: Pirâmide Simples
```bash
python3 generate_test_heightmap.py --size 512 --type pyramid
# Resultado: pyramid.png (512x512, forma de pico)
```

### Teste 3: Alta Resolução
```bash
python3 generate_test_heightmap.py --size 1024 --output highres.png
# Resultado: highres.png (1024x1024, mais detalhes)
```

---

## ?? Erros Comuns e Soluções

### Erro: "Arquivo é PNG 8-bit"
```
? Causa: PNG importado não é 16-bit
? Solução: Converter com GDAL ou QGIS
```

### Erro: "Formato PNG não suportado"
```
? Causa: PNG não é grayscale (é RGBA, RGB, etc)
? Solução: Converter para grayscale antes
```

### Erro: "Dimensões muito grandes"
```
? Causa: PNG > 4096x4096
? Solução: Redimensionar com GDAL ou ImageMagick
```

Exemplo:
```bash
gdal_translate -outsize 2048 2048 input.png output.png
```

### Terreno não aparece
```
? Causa: PNG inválido ou vazio
? Solução: 
   1. Verificar valores de pixel (não todos zeros)
   2. Testar com generate_test_heightmap.py
   3. Ver Status Bar para mensagem de erro
```

---

## ?? Especificações Técnicas

| Propriedade | Valor |
|-----------|-------|
| Formato | PNG 16-bit |
| Colorspace | Grayscale (L16) |
| Dimensões mín. | 2x2 pixels |
| Dimensões máx. | 4096x4096 pixels |
| Valor de altura | ushort (0-65535) |
| Range de escala | 10m a 200m |

---

## ?? Arquitetura

```
PNG 16-bit File
    ?
[OpenFileDialog] ? User selects file
    ?
MainWindow.xaml.cs ? File path
    ?
MainViewModel.ImportHeightmapFromFile()
    ?
ImportHeightmapFromPng (Infrastructure)
    ?
PngHeightmap16BitReader (SixLabors.ImageSharp)
    ?
Heightmap16Bit (Domain Model)
    ?
TerrainMeshGenerator (Rendering3D)
    ?
MeshGeometry3D
    ?
HelixViewport3D
    ?
User sees 3D terrain
```

---

## ?? Documentação Completa

- **HEIGHTMAP_IMPORT_GUIDE.md**: Guia técnico detalhado
- **IMPLEMENTATION_SUMMARY.md**: Sumário da implementação
- **generate_test_heightmap.py**: Utilitário de teste

---

## ? Features

? Importar PNG 16-bit grayscale
? Validação robusta de arquivo
? Mensagens de erro amigáveis
? Regeneração de mesh em tempo real
? Ajuste de escala vertical dinâmico
? Câmera 3D orbital
? Preview de dimensões do heightmap
? Status de importação claro

---

## ?? Controles Resumidos

| Ação | Controle |
|------|---------|
| Importar PNG | Clique botão azul |
| Girar terreno | Botão direito + arrastar |
| Zoom | Roda do mouse |
| Mover câmera | Botão meio + arrastar |
| Ajustar altura | Slider "Vertical Scale" |
| Resetar câmera | Clique "Reset Camera" |
| Gerar teste | Clique "Generate Test Terrain" |

---

## ?? Suporte

Se encontrar problemas:
1. Verifique se o PNG é 16-bit: `file terrain.png`
2. Veja mensagem de status na aplicação
3. Tente com `generate_test_heightmap.py`
4. Verifique logs de compilação

---

**Pronto para usar!** ??

Comece importando seu primeiro PNG 16-bit agora.
