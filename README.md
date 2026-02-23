# CityMapStudio

Uma aplicação WPF moderna para edição e geração de terrenos e mapas 3D, com suporte a exportação para Cities: Skylines 2.

## ?? Características

- **Editor 3D em Tempo Real**: Viewport interativo com HelixToolkit
- **Geração de Terreno**: Geração procedural de heightmaps com ruído Perlin
- **Sistema de Nivelamento (LOD)**: Otimização automática de geometria baseada na distância da câmera
- **Editor de Água**: Controle do nível do mar com preview em tempo real
- **Smooth & Normalize**: Suavização e normalização de heightmaps
- **Exportação para Cities: Skylines 2**: Formato nativo CS2 com empacotamento automático
- **Interface Multilíngue**: Suporte para português (PT-BR)
- **Renderização Otimizada**: Malhas 3D com caching e material dinâmico

## ??? Tecnologias

- **.NET 8** - Framework moderno e de alto desempenho
- **WPF** - Interface gráfica desktop robusta
- **HelixToolkit.Wpf** - Renderização 3D avançada
- **MVVM Toolkit** - Padrão MVVM reativo
- **SixLabors.ImageSharp** - Processamento de imagens PNG

## ?? Estrutura do Projeto

```
CityMapStudio/
??? CityMapStudio.UI.Wpf/           # Camada de apresentação (WPF)
??? CityMapStudio.Application/      # Lógica de aplicação
??? CityMapStudio.Domain/           # Modelos de domínio
??? CityMapStudio.Infrastructure/   # Persistência e I/O
??? CityMapStudio.Rendering3D/      # Renderização 3D
```

## ?? Como Começar

### Pré-requisitos
- .NET 8 SDK ou superior
- Visual Studio 2022 (recomendado) ou Visual Studio Code
- Windows 10/11

### Compilação

```bash
# Restaurar dependências
dotnet restore

# Compilar projeto
dotnet build

# Executar aplicação
dotnet run --project CityMapStudio.UI.Wpf
```

## ?? Uso

1. **Gerar Terreno**: Clique em "Test Map" para gerar um heightmap procedural
2. **Importar Heightmap**: Clique em "Import PNG" para carregar um PNG 16-bit grayscale
3. **Editar Terreno**: 
   - Selecione "Terrain" na esquerda
   - Use os modos: Raise, Lower, Smooth, Flatten, SetHeight, Noise
4. **Ajustar Água**: 
   - Selecione "Water" na esquerda
   - Use o slider de "Sea Level"
5. **Exportar**: Clique em "Export CS2" para gerar pacote para Cities: Skylines 2

## ?? Conhecidos/Melhorias

- [ ] Undo/Redo completo
- [ ] Pincéis de edição com preview
- [ ] Múltiplas camadas de recursos
- [ ] Modo noturno avançado
- [ ] Performance em mapas > 4096x4096

## ?? Licença

MIT License - Veja LICENSE para detalhes

## ?? Contribuindo

1. Faça um Fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## ?? Contato

Para problemas e sugestões, abra uma Issue no repositório.

---

**Status**: Em desenvolvimento ??
**Última atualização**: 2024
