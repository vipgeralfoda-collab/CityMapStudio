using System.Windows.Media.Media3D;
using CityMapStudio.Domain.Models;

namespace CityMapStudio.Rendering3D.Services
{
    public class TerrainMeshData
    {
        public MeshGeometry3D TerrainMesh { get; set; }
        public MeshGeometry3D WaterMesh { get; set; }
    }

    /// <summary>
    /// Gera malhas 3D de terreno com suporte a LOD (Level of Detail) por step (subamostragem)
    /// </summary>
    public class TerrainMeshGenerator
    {
        /// <summary>
        /// Gera mesh do terreno com suporte a LOD via step
        /// step=1: cada ponto (LOD0, máximo detalhe)
        /// step=2: ponto a cada 2 (LOD1)
        /// step=4: ponto a cada 4 (LOD2)
        /// step=8: ponto a cada 8 (LOD3)
        /// step=16: ponto a cada 16 (LOD4, mínimo detalhe)
        /// </summary>
        public TerrainMeshData GenerateTerrainMeshWithSeaLevel(
            Heightmap16Bit heightmap,
            float verticalScale,
            float seaLevel,
            float cellSize = 1.0f,
            int step = 1)
        {
            var terrainMesh = new MeshGeometry3D();
            var waterMesh = new MeshGeometry3D();

            int width = heightmap.Width;
            int height = heightmap.Height;

            // Garantir que step é válido
            step = Math.Max(1, step);

            // Dicionários para mapeamento de vértices (terra e água separadas)
            var terrainVertexMap = new Dictionary<int, int>();
            var waterVertexMap = new Dictionary<int, int>();
            int terrainVertexCount = 0;
            int waterVertexCount = 0;

            // ===== GERAR VÉRTICES DO TOPO (com step) =====
            for (int y = 0; y < height; y += step)
            {
                for (int x = 0; x < width; x += step)
                {
                    float heightValue = (heightmap.GetHeight(x, y) / 65535f) * verticalScale;
                    float posX = x * cellSize;
                    float posZ = y * cellSize;
                    int index = y * width + x;

                    if (heightValue >= seaLevel)
                    {
                        terrainVertexMap[index] = terrainVertexCount++;
                        terrainMesh.Positions.Add(new Point3D(posX, heightValue, posZ));
                    }
                    else
                    {
                        waterVertexMap[index] = waterVertexCount++;
                        waterMesh.Positions.Add(new Point3D(posX, heightValue, posZ));
                    }
                }
            }

            // ===== GERAR TRIÂNGULOS DO TOPO (com step) =====
            for (int y = 0; y < height - step; y += step)
            {
                for (int x = 0; x < width - step; x += step)
                {
                    int v0_old = y * width + x;
                    int v1_old = y * width + x + step;
                    int v2_old = (y + step) * width + x;
                    int v3_old = (y + step) * width + x + step;

                    // Terra - Triângulo 1 (v0-v1-v2)
                    if (terrainVertexMap.ContainsKey(v0_old) && 
                        terrainVertexMap.ContainsKey(v1_old) && 
                        terrainVertexMap.ContainsKey(v2_old))
                    {
                        terrainMesh.TriangleIndices.Add(terrainVertexMap[v0_old]);
                        terrainMesh.TriangleIndices.Add(terrainVertexMap[v1_old]);
                        terrainMesh.TriangleIndices.Add(terrainVertexMap[v2_old]);
                    }

                    // Terra - Triângulo 2 (v1-v3-v2)
                    if (terrainVertexMap.ContainsKey(v1_old) && 
                        terrainVertexMap.ContainsKey(v3_old) && 
                        terrainVertexMap.ContainsKey(v2_old))
                    {
                        terrainMesh.TriangleIndices.Add(terrainVertexMap[v1_old]);
                        terrainMesh.TriangleIndices.Add(terrainVertexMap[v3_old]);
                        terrainMesh.TriangleIndices.Add(terrainVertexMap[v2_old]);
                    }

                    // Água - Triângulo 1 (v0-v1-v2)
                    if (waterVertexMap.ContainsKey(v0_old) && 
                        waterVertexMap.ContainsKey(v1_old) && 
                        waterVertexMap.ContainsKey(v2_old))
                    {
                        waterMesh.TriangleIndices.Add(waterVertexMap[v0_old]);
                        waterMesh.TriangleIndices.Add(waterVertexMap[v1_old]);
                        waterMesh.TriangleIndices.Add(waterVertexMap[v2_old]);
                    }

                    // Água - Triângulo 2 (v1-v3-v2)
                    if (waterVertexMap.ContainsKey(v1_old) && 
                        waterVertexMap.ContainsKey(v3_old) && 
                        waterVertexMap.ContainsKey(v2_old))
                    {
                        waterMesh.TriangleIndices.Add(waterVertexMap[v1_old]);
                        waterMesh.TriangleIndices.Add(waterVertexMap[v3_old]);
                        waterMesh.TriangleIndices.Add(waterVertexMap[v2_old]);
                    }
                }
            }

            int terrainTopVertexCount = terrainMesh.Positions.Count;

            // ===== GERAR VÉRTICES DAS LATERAIS (com step mais grosso) =====
            int sideStep = Math.Max(step * 2, 2); // Laterais sempre com step >= 2

            // Parede frente (y = 0)
            for (int x = 0; x < width; x += sideStep)
            {
                float heightValue = (heightmap.GetHeight(x, 0) / 65535f) * verticalScale;
                float posX = x * cellSize;
                terrainMesh.Positions.Add(new Point3D(posX, 0, 0));
                terrainMesh.Positions.Add(new Point3D(posX, heightValue, 0));
            }

            // Parede trás (y = height-1)
            for (int x = 0; x < width; x += sideStep)
            {
                float heightValue = (heightmap.GetHeight(x, height - 1) / 65535f) * verticalScale;
                float posX = x * cellSize;
                float posZ = (height - 1) * cellSize;
                terrainMesh.Positions.Add(new Point3D(posX, 0, posZ));
                terrainMesh.Positions.Add(new Point3D(posX, heightValue, posZ));
            }

            // Parede esquerda (x = 0)
            for (int y = 0; y < height; y += sideStep)
            {
                float heightValue = (heightmap.GetHeight(0, y) / 65535f) * verticalScale;
                float posZ = y * cellSize;
                terrainMesh.Positions.Add(new Point3D(0, 0, posZ));
                terrainMesh.Positions.Add(new Point3D(0, heightValue, posZ));
            }

            // Parede direita (x = width-1)
            for (int y = 0; y < height; y += sideStep)
            {
                float heightValue = (heightmap.GetHeight(width - 1, y) / 65535f) * verticalScale;
                float posX = (width - 1) * cellSize;
                float posZ = y * cellSize;
                terrainMesh.Positions.Add(new Point3D(posX, 0, posZ));
                terrainMesh.Positions.Add(new Point3D(posX, heightValue, posZ));
            }

            int sideStartIndex = terrainTopVertexCount;

            // ===== GERAR TRIÂNGULOS DAS LATERAIS (simplificados) =====
            int frontBaseStart = sideStartIndex;
            int frontCount = (width + sideStep - 1) / sideStep;
            for (int i = 0; i < frontCount - 1; i++)
            {
                int base0 = frontBaseStart + i * 2;
                int base1 = frontBaseStart + (i + 1) * 2;
                int top0 = base0 + 1;
                int top1 = base1 + 1;

                terrainMesh.TriangleIndices.Add(base0);
                terrainMesh.TriangleIndices.Add(top1);
                terrainMesh.TriangleIndices.Add(top0);

                terrainMesh.TriangleIndices.Add(base0);
                terrainMesh.TriangleIndices.Add(base1);
                terrainMesh.TriangleIndices.Add(top1);
            }

            int backBaseStart = frontBaseStart + frontCount * 2;
            int backCount = (width + sideStep - 1) / sideStep;
            for (int i = 0; i < backCount - 1; i++)
            {
                int base0 = backBaseStart + i * 2;
                int base1 = backBaseStart + (i + 1) * 2;
                int top0 = base0 + 1;
                int top1 = base1 + 1;

                terrainMesh.TriangleIndices.Add(base0);
                terrainMesh.TriangleIndices.Add(top0);
                terrainMesh.TriangleIndices.Add(top1);

                terrainMesh.TriangleIndices.Add(base0);
                terrainMesh.TriangleIndices.Add(top1);
                terrainMesh.TriangleIndices.Add(base1);
            }

            int leftBaseStart = backBaseStart + backCount * 2;
            int leftCount = (height + sideStep - 1) / sideStep;
            for (int i = 0; i < leftCount - 1; i++)
            {
                int base0 = leftBaseStart + i * 2;
                int base1 = leftBaseStart + (i + 1) * 2;
                int top0 = base0 + 1;
                int top1 = base1 + 1;

                terrainMesh.TriangleIndices.Add(base0);
                terrainMesh.TriangleIndices.Add(top0);
                terrainMesh.TriangleIndices.Add(top1);

                terrainMesh.TriangleIndices.Add(base0);
                terrainMesh.TriangleIndices.Add(top1);
                terrainMesh.TriangleIndices.Add(base1);
            }

            int rightBaseStart = leftBaseStart + leftCount * 2;
            int rightCount = (height + sideStep - 1) / sideStep;
            for (int i = 0; i < rightCount - 1; i++)
            {
                int base0 = rightBaseStart + i * 2;
                int base1 = rightBaseStart + (i + 1) * 2;
                int top0 = base0 + 1;
                int top1 = base1 + 1;

                terrainMesh.TriangleIndices.Add(base0);
                terrainMesh.TriangleIndices.Add(top1);
                terrainMesh.TriangleIndices.Add(top0);

                terrainMesh.TriangleIndices.Add(base0);
                terrainMesh.TriangleIndices.Add(base1);
                terrainMesh.TriangleIndices.Add(top1);
            }

            // ===== ADICIONAR MALHA DE ÁGUA PLANA (SE NECESSÁRIO) =====
            // Se não houver água subterrânea, criar malha de água plana na superfície
            if (waterMesh.Positions.Count == 0)
            {
                // Criar malha de água plana na altura do seaLevel cobrindo toda a área
                // Adicionar 4 vértices nos cantos do mapa
                waterMesh.Positions.Add(new Point3D(0, seaLevel, 0));
                waterMesh.Positions.Add(new Point3D((width - 1) * cellSize, seaLevel, 0));
                waterMesh.Positions.Add(new Point3D((width - 1) * cellSize, seaLevel, (height - 1) * cellSize));
                waterMesh.Positions.Add(new Point3D(0, seaLevel, (height - 1) * cellSize));

                // Criar 2 triângulos para cobrir toda a área
                waterMesh.TriangleIndices.Add(0);
                waterMesh.TriangleIndices.Add(1);
                waterMesh.TriangleIndices.Add(2);

                waterMesh.TriangleIndices.Add(0);
                waterMesh.TriangleIndices.Add(2);
                waterMesh.TriangleIndices.Add(3);

                // Adicionar normals (apontando para cima para iluminação correta)
                var upNormal = new Vector3D(0, 1, 0);
                waterMesh.Normals.Add(upNormal);
                waterMesh.Normals.Add(upNormal);
                waterMesh.Normals.Add(upNormal);
                waterMesh.Normals.Add(upNormal);
            }

            terrainMesh.Freeze();
            waterMesh.Freeze();

            return new TerrainMeshData
            {
                TerrainMesh = terrainMesh,
                WaterMesh = waterMesh
            };
        }

        /// <summary>
        /// Versão simples sem LOD (usa step=1)
        /// </summary>
        public MeshGeometry3D GenerateTerrainMesh(Heightmap16Bit heightmap, float verticalScale, float cellSize = 1.0f, float seaLevel = 0f)
        {
            var data = GenerateTerrainMeshWithSeaLevel(heightmap, verticalScale, seaLevel, cellSize, step: 1);
            return data.TerrainMesh;
        }
    }
}
