using System.Windows.Media.Media3D;
using CityMapStudio.Domain.Models;

namespace CityMapStudio.Rendering3D.Services
{
    public class TerrainMeshData
    {
        public MeshGeometry3D TerrainMesh { get; set; }
        public MeshGeometry3D WaterMesh { get; set; }
    }

    public class TerrainMeshGenerator
    {
        public TerrainMeshData GenerateTerrainMeshWithSeaLevel(Heightmap16Bit heightmap, float verticalScale, float seaLevel, float cellSize = 1.0f)
        {
            var terrainMesh = new MeshGeometry3D();
            var waterMesh = new MeshGeometry3D();

            int width = heightmap.Width;
            int height = heightmap.Height;

            // Dicionários para mapeamento de vértices (terra e água separadas)
            var terrainVertexMap = new Dictionary<int, int>();
            var waterVertexMap = new Dictionary<int, int>();
            int terrainVertexCount = 0;
            int waterVertexCount = 0;

            // ===== GERAR VÉRTICES DO TOPO =====
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float heightValue = (heightmap.GetHeight(x, y) / 65535f) * verticalScale;
                    float posX = x * cellSize;
                    float posZ = y * cellSize;
                    int index = y * width + x;

                    if (heightValue >= seaLevel)
                    {
                        // Vértice de terra
                        terrainVertexMap[index] = terrainVertexCount++;
                        terrainMesh.Positions.Add(new Point3D(posX, heightValue, posZ));
                    }
                    else
                    {
                        // Vértice de água - criar na altura real do terreno
                        waterVertexMap[index] = waterVertexCount++;
                        waterMesh.Positions.Add(new Point3D(posX, heightValue, posZ));
                    }
                }
            }

            // ===== GERAR TRIÂNGULOS DO TOPO =====
            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    int v0_old = y * width + x;
                    int v1_old = y * width + x + 1;
                    int v2_old = (y + 1) * width + x;
                    int v3_old = (y + 1) * width + x + 1;

                    // Triângulo 1 terra
                    int t_v0 = terrainVertexMap.ContainsKey(v0_old) ? terrainVertexMap[v0_old] : -1;
                    int t_v2 = terrainVertexMap.ContainsKey(v2_old) ? terrainVertexMap[v2_old] : -1;
                    int t_v1 = terrainVertexMap.ContainsKey(v1_old) ? terrainVertexMap[v1_old] : -1;

                    if (t_v0 != -1 && t_v2 != -1 && t_v1 != -1)
                    {
                        terrainMesh.TriangleIndices.Add(t_v0);
                        terrainMesh.TriangleIndices.Add(t_v2);
                        terrainMesh.TriangleIndices.Add(t_v1);
                    }

                    // Triângulo 2 terra
                    int t_v3 = terrainVertexMap.ContainsKey(v3_old) ? terrainVertexMap[v3_old] : -1;
                    if (t_v1 != -1 && t_v2 != -1 && t_v3 != -1)
                    {
                        terrainMesh.TriangleIndices.Add(t_v1);
                        terrainMesh.TriangleIndices.Add(t_v2);
                        terrainMesh.TriangleIndices.Add(t_v3);
                    }

                    // Triângulos de água
                    int w_v0 = waterVertexMap.ContainsKey(v0_old) ? waterVertexMap[v0_old] : -1;
                    int w_v2 = waterVertexMap.ContainsKey(v2_old) ? waterVertexMap[v2_old] : -1;
                    int w_v1 = waterVertexMap.ContainsKey(v1_old) ? waterVertexMap[v1_old] : -1;

                    if (w_v0 != -1 && w_v2 != -1 && w_v1 != -1)
                    {
                        waterMesh.TriangleIndices.Add(w_v0);
                        waterMesh.TriangleIndices.Add(w_v2);
                        waterMesh.TriangleIndices.Add(w_v1);
                    }

                    int w_v3 = waterVertexMap.ContainsKey(v3_old) ? waterVertexMap[v3_old] : -1;
                    if (w_v1 != -1 && w_v2 != -1 && w_v3 != -1)
                    {
                        waterMesh.TriangleIndices.Add(w_v1);
                        waterMesh.TriangleIndices.Add(w_v2);
                        waterMesh.TriangleIndices.Add(w_v3);
                    }
                }
            }

            int terrainTopVertexCount = terrainMesh.Positions.Count;

            // ===== GERAR VÉRTICES DAS LATERAIS (apenas TERRA) =====
            // Parede frente (y = 0)
            for (int x = 0; x < width; x++)
            {
                float heightValue = (heightmap.GetHeight(x, 0) / 65535f) * verticalScale;
                float posX = x * cellSize;
                terrainMesh.Positions.Add(new Point3D(posX, 0, 0));       // base
                terrainMesh.Positions.Add(new Point3D(posX, heightValue, 0)); // topo
            }

            // Parede trás (y = height-1)
            for (int x = 0; x < width; x++)
            {
                float heightValue = (heightmap.GetHeight(x, height - 1) / 65535f) * verticalScale;
                float posX = x * cellSize;
                float posZ = (height - 1) * cellSize;
                terrainMesh.Positions.Add(new Point3D(posX, 0, posZ));           // base
                terrainMesh.Positions.Add(new Point3D(posX, heightValue, posZ)); // topo
            }

            // Parede esquerda (x = 0)
            for (int y = 0; y < height; y++)
            {
                float heightValue = (heightmap.GetHeight(0, y) / 65535f) * verticalScale;
                float posZ = y * cellSize;
                terrainMesh.Positions.Add(new Point3D(0, 0, posZ));       // base
                terrainMesh.Positions.Add(new Point3D(0, heightValue, posZ)); // topo
            }

            // Parede direita (x = width-1)
            for (int y = 0; y < height; y++)
            {
                float heightValue = (heightmap.GetHeight(width - 1, y) / 65535f) * verticalScale;
                float posX = (width - 1) * cellSize;
                float posZ = y * cellSize;
                terrainMesh.Positions.Add(new Point3D(posX, 0, posZ));           // base
                terrainMesh.Positions.Add(new Point3D(posX, heightValue, posZ)); // topo
            }

            int sideStartIndex = terrainTopVertexCount;

            // ===== GERAR TRIÂNGULOS DAS LATERAIS =====
            // Triângulos parede frente
            int frontBaseStart = sideStartIndex;
            for (int x = 0; x < width - 1; x++)
            {
                int base0 = frontBaseStart + x * 2;
                int base1 = frontBaseStart + (x + 1) * 2;
                int top0 = base0 + 1;
                int top1 = base1 + 1;

                terrainMesh.TriangleIndices.Add(base0);
                terrainMesh.TriangleIndices.Add(top1);
                terrainMesh.TriangleIndices.Add(top0);

                terrainMesh.TriangleIndices.Add(base0);
                terrainMesh.TriangleIndices.Add(base1);
                terrainMesh.TriangleIndices.Add(top1);
            }

            // Triângulos parede trás
            int backBaseStart = frontBaseStart + width * 2;
            for (int x = 0; x < width - 1; x++)
            {
                int base0 = backBaseStart + x * 2;
                int base1 = backBaseStart + (x + 1) * 2;
                int top0 = base0 + 1;
                int top1 = base1 + 1;

                terrainMesh.TriangleIndices.Add(base0);
                terrainMesh.TriangleIndices.Add(top0);
                terrainMesh.TriangleIndices.Add(top1);

                terrainMesh.TriangleIndices.Add(base0);
                terrainMesh.TriangleIndices.Add(top1);
                terrainMesh.TriangleIndices.Add(base1);
            }

            // Triângulos parede esquerda
            int leftBaseStart = backBaseStart + width * 2;
            for (int y = 0; y < height - 1; y++)
            {
                int base0 = leftBaseStart + y * 2;
                int base1 = leftBaseStart + (y + 1) * 2;
                int top0 = base0 + 1;
                int top1 = base1 + 1;

                terrainMesh.TriangleIndices.Add(base0);
                terrainMesh.TriangleIndices.Add(top0);
                terrainMesh.TriangleIndices.Add(top1);

                terrainMesh.TriangleIndices.Add(base0);
                terrainMesh.TriangleIndices.Add(top1);
                terrainMesh.TriangleIndices.Add(base1);
            }

            // Triângulos parede direita
            int rightBaseStart = leftBaseStart + height * 2;
            for (int y = 0; y < height - 1; y++)
            {
                int base0 = rightBaseStart + y * 2;
                int base1 = rightBaseStart + (y + 1) * 2;
                int top0 = base0 + 1;
                int top1 = base1 + 1;

                terrainMesh.TriangleIndices.Add(base0);
                terrainMesh.TriangleIndices.Add(top1);
                terrainMesh.TriangleIndices.Add(top0);

                terrainMesh.TriangleIndices.Add(base0);
                terrainMesh.TriangleIndices.Add(base1);
                terrainMesh.TriangleIndices.Add(top1);
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
        /// Versão simples: gera apenas o mesh de terra (sem água)
        /// Mantém compatibilidade com código antigo
        /// </summary>
        public MeshGeometry3D GenerateTerrainMesh(Heightmap16Bit heightmap, float verticalScale, float cellSize = 1.0f, float seaLevel = 0f)
        {
            var data = GenerateTerrainMeshWithSeaLevel(heightmap, verticalScale, seaLevel, cellSize);
            return data.TerrainMesh;
        }
    }
}
