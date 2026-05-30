using UnityEngine;

public class GuideBeam : MonoBehaviour
{
    [SerializeField] private float beamHeight = 40f;
        [SerializeField] private float beamWidth  = 0.35f;
        [SerializeField] private Color beamColor  = new Color(1f, 0.92f, 0.2f, 0.9f);
     
        // El cubo que hace de "rayo". Se crea por código, no hace falta asignar nada en el Inspector.
        private GameObject beamCube;
        private Vector3    groundPosition;
     
        // -------------------------------------------------------------------------
        // API pública (misma que antes, sin cambios en DeliveryManager)
        // -------------------------------------------------------------------------
     
        public void Configure(float height, float width, Color color)
        {
            beamHeight = height;
            beamWidth  = width;
            beamColor  = color;
     
            // Si el cubo ya existe, actualizamos sus valores en caliente
            if (beamCube != null)
                ApplyVisualSettings();
        }
     
        public void SetTarget(Vector3 worldPosition)
        {
            groundPosition = worldPosition;
            UpdateBeamTransform();
        }
     
        public void SetActive(bool active)
        {
            EnsureBeamCube();
            beamCube.SetActive(active);
     
            if (active)
                UpdateBeamTransform();
        }
     
        // -------------------------------------------------------------------------
        // Internos
        // -------------------------------------------------------------------------
     
        private void EnsureBeamCube()
        {
            if (beamCube != null) return;
     
            // Creamos un cubo primitivo y lo adjuntamos a este GameObject
            beamCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beamCube.name = "BeamCube";
            beamCube.transform.SetParent(transform, false);
     
            // Quitamos el collider para que no interfiera con la física del juego
            Destroy(beamCube.GetComponent<Collider>());
     
            ApplyVisualSettings();
            beamCube.SetActive(false); // empieza apagado
        }
     
        private void ApplyVisualSettings()
        {
            var rend = beamCube.GetComponent<MeshRenderer>();
     
            // Creamos un material con transparencia simple (URP/Built-in compatible)
            // Si usas HDRP sustituye "Sprites/Default" por "HDRP/Unlit" o el que prefieras
            var mat = new Material(Shader.Find("Sprites/Default"))
            {
                color = beamColor
            };
            rend.material = mat;
        }
     
        private void UpdateBeamTransform()
        {
            EnsureBeamCube();
     
            // El cubo se escala para que parezca un rayo vertical:
            // - X y Z = grosor del haz
            // - Y = altura total
            beamCube.transform.localScale = new Vector3(beamWidth, beamHeight, beamWidth);
     
            // Lo posicionamos con la base en el suelo (groundPosition) y que suba hacia arriba
            beamCube.transform.position = groundPosition + Vector3.up * (beamHeight * 0.5f);
     
            // Sin rotación adicional (el cubo ya es vertical por defecto)
            beamCube.transform.rotation = Quaternion.identity;
        }
}
