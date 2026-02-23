namespace CityMapStudio.Infrastructure.IO
{
    public interface IReadmeGenerator
    {
        void GenerateReadme(string outputPath, string mapName);
    }

    public class ReadmeGenerator : IReadmeGenerator
    {
        public void GenerateReadme(string outputPath, string mapName)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be empty", nameof(outputPath));

            string readme = $"""
                # {mapName} - Cities: Skylines II Terrain Package

                ## Como Importar este Mapa

                ### Pré-requisitos
                - Cities: Skylines II instalado
                - Map Editor desbloqueado (acessível via Steam)

                ### Passos para Importação

                1. **Copiar Heightmap**
                   - Localize a pasta de heightmaps do CS2:
                     Windows: `C:\Users\{Environment.UserName}\AppData\Local\Colossal Order\Cities Skylines II\Heightmaps`
                   - Copie o arquivo `heightmap.png` para esta pasta

                2. **Abrir Map Editor**
                   - Inicie Cities: Skylines II
                   - Vá para Map Editor (no menu principal)

                3. **Importar Heightmap**
                   - Clique em "New Map"
                   - Selecione "Import Heightmap"
                   - Escolha o arquivo `heightmap.png` copiado
                   - Ajuste os parâmetros conforme necessário

                4. **Configurar Detalhes**
                   - Propriedades do Mapa:
                     - Escala Vertical: {GetMetadataFromProjectJson()}
                     - Sea Level: (ajustável no editor)
                   - Coloque cidades, edifícios e detalhes
                   - Teste o mapa em game

                5. **Salvar**
                   - Salve como mapa oficial
                   - Publique na Workshop (opcional)

                ---

                ## Informações do Pacote

                Este pacote foi gerado pelo CityMapStudio e contém:
                - `heightmap.png`: Mapa de altitudes em 16-bit PNG
                - `project.json`: Metadados do projeto
                - `README.txt`: Este arquivo

                ## Suporte

                Para problemas ou dúvidas:
                1. Verifique se o heightmap foi copiado para a pasta correta
                2. Reinicie o Cities: Skylines II
                3. Tente importar novamente no Map Editor

                ---

                Criado com CityMapStudio
                {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
                """;

            File.WriteAllText(outputPath, readme);
        }

        private string GetMetadataFromProjectJson()
        {
            // Fallback: será atualizado dinamicamente
            return "Verifique project.json para detalhes completos";
        }
    }
}
