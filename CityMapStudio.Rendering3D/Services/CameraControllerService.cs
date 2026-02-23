using System;
using System.Windows.Media.Media3D;

namespace CityMapStudio.Rendering3D.Services
{
    /// <summary>
    /// Gerencia a câmera do viewport 3D (orientação, constraints, reset)
    /// Garante que o mapa sempre apareça com orientação correta (vista de cima)
    /// </summary>
    public class CameraControllerService
    {
        private const float DefaultMarginPercentage = 0.1f; // 10% de margem

        /// <summary>
        /// Reseta a câmera para vista de cima (top-down) com orientação correta
        /// SEMPRE chamado ao criar/importar/resetar mapa
        /// </summary>
        public void ResetCameraToTopView(
            object viewportObj,  // HelixViewport3D (type-safe via duck typing)
            float mapWidth,
            float mapHeight,
            float terrainMaxHeight)
        {
            if (viewportObj == null)
                return;

            // Usar reflection para acessar Camera
            var cameraProperty = viewportObj.GetType().GetProperty("Camera");
            if (cameraProperty == null)
                return;

            var camera = cameraProperty.GetValue(viewportObj) as PerspectiveCamera;
            if (camera == null)
                return;

            // Calcular centro do mapa
            float centerX = mapWidth / 2f;
            float centerZ = mapHeight / 2f;
            float mapSize = Math.Max(mapWidth, mapHeight);

            // Altura média do terreno
            float averageTerrainHeight = terrainMaxHeight * 0.25f;

            // Distância da câmera acima do terreno
            float cameraDistance = mapSize * 1.3f;

            // Posição da câmera (acima do centro, olhando para baixo - vista frontal)
            camera.Position = new Point3D(
                centerX,                              // X: centro
                terrainMaxHeight + cameraDistance,    // Y: bem acima
                centerZ + cameraDistance * 0.7f       // Z: para frente (vista frontal, não lateral)
            );

            // Alvo (LookAt): centro do terreno em altura média
            var target = new Point3D(
                centerX,
                averageTerrainHeight,
                centerZ
            );

            // Direção de visão: para baixo em direção ao alvo
            camera.LookDirection = target - camera.Position;
            camera.LookDirection.Normalize();

            // Up direction: Y sempre aponta para cima (mantém o mapa reto)
            camera.UpDirection = new Vector3D(0, 1, 0);

            // Ajustar FOV para ver o mapa completo
            camera.FieldOfView = 50;

            // Ajustar near/far planes para evitar clipping
            camera.NearPlaneDistance = 0.01;
            camera.FarPlaneDistance = mapSize * 15;
        }

        /// <summary>
        /// Aplica constraints à câmera para impedir:
        /// - Atravessar o mapa
        /// - Ir abaixo do terreno
        /// - Afastar demais
        /// </summary>
        public void ApplyCameraConstraints(
            object viewportObj,  // HelixViewport3D
            float mapWidth,
            float mapHeight,
            float terrainMaxHeight,
            float groundMinY = 0)
        {
            if (viewportObj == null)
                return;

            var cameraProperty = viewportObj.GetType().GetProperty("Camera");
            if (cameraProperty == null)
                return;

            var camera = cameraProperty.GetValue(viewportObj) as PerspectiveCamera;
            if (camera == null)
                return;

            float mapSize = Math.Max(mapWidth, mapHeight);
            float margin = mapSize * DefaultMarginPercentage;

            var pos = camera.Position;

            // Clamp X/Z (não sair do mapa)
            float clampedX = Math.Clamp((float)pos.X, -margin, mapWidth + margin);
            float clampedZ = Math.Clamp((float)pos.Z, -margin, mapHeight + margin);

            // Clamp Y (não ficar abaixo do terreno, não afastar demais)
            float minY = groundMinY + 1.0f;                    // Mínimo: acima do chão
            float maxY = terrainMaxHeight + (mapSize * 2.5f); // Máximo: bem acima
            float clampedY = Math.Clamp((float)pos.Y, minY, maxY);

            // Aplicar constraints se houver mudança
            if (Math.Abs(pos.X - clampedX) > 0.01f ||
                Math.Abs(pos.Y - clampedY) > 0.01f ||
                Math.Abs(pos.Z - clampedZ) > 0.01f)
            {
                camera.Position = new Point3D(clampedX, clampedY, clampedZ);
            }

            // Constrair o target também
            var lookDir = camera.LookDirection;
            if (lookDir.Length > 0)
            {
                lookDir.Normalize();
                
                // Calcular target baseado na look direction
                var target = camera.Position + lookDir * 100;
                
                // Clamp target
                float targetX = Math.Clamp((float)target.X, -margin, mapWidth + margin);
                float targetZ = Math.Clamp((float)target.Z, -margin, mapHeight + margin);
                float targetY = Math.Clamp((float)target.Y, groundMinY, terrainMaxHeight + mapSize);

                var constrainedTarget = new Point3D(targetX, targetY, targetZ);
                camera.LookDirection = constrainedTarget - camera.Position;
            }
        }

        /// <summary>
        /// Calcula a distância ideal da câmera para ver o mapa inteiro
        /// </summary>
        public float CalculateOptimalDistance(float mapWidth, float mapHeight, float terrainMaxHeight)
        {
            float mapSize = Math.Max(mapWidth, mapHeight);
            return mapSize * 1.5f;
        }

        /// <summary>
        /// Verifica se câmera está muito perto do terreno
        /// </summary>
        public bool IsCameraInsideTerrainBounds(PerspectiveCamera camera, float mapWidth, float mapHeight)
        {
            if (camera == null)
                return false;

            var pos = camera.Position;
            return pos.X < 0 || pos.X > mapWidth ||
                   pos.Z < 0 || pos.Z > mapHeight;
        }

        /// <summary>
        /// Ajusta near/far planes baseado no tamanho do mapa
        /// Previne clipping e z-fighting
        /// </summary>
        public void UpdateCameraPlanes(
            PerspectiveCamera camera,
            float mapWidth,
            float mapHeight,
            float terrainMaxHeight)
        {
            if (camera == null)
                return;

            float mapSize = Math.Max(mapWidth, mapHeight);

            // Near: bem perto para detalhe
            camera.NearPlaneDistance = Math.Max(0.1, mapSize * 0.01f);

            // Far: bem longe para não clipar
            camera.FarPlaneDistance = mapSize * 10;
        }
    }
}
