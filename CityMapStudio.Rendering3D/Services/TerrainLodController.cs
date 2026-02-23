namespace CityMapStudio.Rendering3D.Services
{
    /// <summary>
    /// Define os thresholds de distância e steps para cada LOD
    /// </summary>
    public class LODProfile
    {
        public struct LODLevel
        {
            public int Step { get; set; }
            public float DistanceThreshold { get; set; }
            public float HysteresisMargin { get; set; }
        }

        public LODLevel[] Levels { get; set; }

        public LODProfile()
        {
            // Thresholds padrão (em unidades de mundo)
            Levels = new[]
            {
                new LODLevel { Step = 1, DistanceThreshold = 80f, HysteresisMargin = 8f },      // LOD0
                new LODLevel { Step = 2, DistanceThreshold = 140f, HysteresisMargin = 14f },    // LOD1
                new LODLevel { Step = 4, DistanceThreshold = 220f, HysteresisMargin = 22f },    // LOD2
                new LODLevel { Step = 8, DistanceThreshold = 320f, HysteresisMargin = 32f },    // LOD3
                new LODLevel { Step = 16, DistanceThreshold = float.MaxValue, HysteresisMargin = 0 } // LOD4
            };
        }
    }

    /// <summary>
    /// Controla qual LOD usar baseado na distância da câmera
    /// </summary>
    public class TerrainLodController
    {
        private LODProfile _profile;
        private int _currentLodIndex = 0;
        private float _terrainCenterX;
        private float _terrainCenterZ;
        private float _terrainSize;

        public int CurrentLodIndex => _currentLodIndex;
        public int CurrentStep => _profile.Levels[_currentLodIndex].Step;

        public TerrainLodController(LODProfile profile, float terrainCenterX, float terrainCenterZ, float terrainSize)
        {
            _profile = profile;
            _terrainCenterX = terrainCenterX;
            _terrainCenterZ = terrainCenterZ;
            _terrainSize = terrainSize;
        }

        /// <summary>
        /// Calcula qual LOD deveria estar ativo baseado na posição da câmera
        /// Retorna true se LOD mudou
        /// </summary>
        public bool UpdateLod(System.Windows.Media.Media3D.Point3D cameraPosition)
        {
            // Calcular distância da câmera ao centro do terreno (XZ apenas)
            float distX = (float)cameraPosition.X - _terrainCenterX;
            float distZ = (float)cameraPosition.Z - _terrainCenterZ;
            float distance = (float)Math.Sqrt(distX * distX + distZ * distZ);

            int desiredLod = FindDesiredLod(distance);

            if (desiredLod != _currentLodIndex)
            {
                _currentLodIndex = desiredLod;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Encontra o LOD desejado com histerese para evitar flicker
        /// </summary>
        private int FindDesiredLod(float distance)
        {
            for (int i = 0; i < _profile.Levels.Length; i++)
            {
                float threshold = _profile.Levels[i].DistanceThreshold;
                float margin = _profile.Levels[i].HysteresisMargin;

                // Se estamos no LOD atual, usar margem de hysterese
                if (i == _currentLodIndex)
                {
                    if (distance < threshold + margin)
                        return i;
                }
                else
                {
                    // Se mudando para este LOD, exigir estar bem dentro
                    if (distance < threshold - margin)
                        return i;
                }
            }

            // LOD4 é sempre o último
            return _profile.Levels.Length - 1;
        }
    }
}
