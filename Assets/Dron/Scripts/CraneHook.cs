using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CraneHook : MonoBehaviour
{
    [Header("Área del Gancho")]
    public float hookRadius = 3f;
    public Vector3 hookOffset = new Vector3(0, -1.5f, 0); // DOnde se crea la detccion en respecto al dron habria que hacer un gizmo o poner algo 

    // NUEVO: Asigna aquí un Empty Object colocado en la panza de tu dron. De aquí saldrá la cuerda visible.
    public Transform cableOrigin;

    [Header("Físicas del Cable (Joints)")]
    public float springForce = 50f;
    public float springDamper = 5f;
    public float cableLength = 4f;

    [Header("Visuales del Cable")]
    public float cableWidth = 0.05f; // Grosor del cable
    public Material cableMaterial;   // Arrastra aquí un material (ej. negro mate o HDRP Unlit negro)

    private Rigidbody droneRb;
    private SpringJoint currentCable;
    private LineRenderer lineRenderer; // Componente para dibujar el cable

    void Start()
    {
        droneRb = GetComponent<Rigidbody>();

        // Preparamos el LineRenderer al inicio
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.enabled = false; // Lo apagamos hasta que enganchemos algo
        lineRenderer.positionCount = 2; // Solo necesitamos dos puntos: origen y destino
        lineRenderer.startWidth = cableWidth;
        lineRenderer.endWidth = cableWidth;

        if (cableMaterial != null)
        {
            lineRenderer.material = cableMaterial;
        }

        // Si no asignas origen, por defecto sale del centro del dron para no dar error
        if (cableOrigin == null)
        {
            cableOrigin = transform;
        }
    }

    public void OnToggleHook(InputAction.CallbackContext context)
    {
        // Performed para que sea feedback mas rapido
        if (context.performed)
        {
            if (currentCable == null) TryHook();
            else ReleaseHook();
        }
    }

    private void TryHook()
    {
        // Para buscar los objetos para recoger(como lo del platfromer fran de proyecto)
        Vector3 hookPosition = transform.TransformPoint(hookOffset);
        Collider[] hits = Physics.OverlapSphere(hookPosition, hookRadius);

        foreach (Collider hit in hits)
        {
            // Solo pillar objetos con el tag que si no es una locura y enganchas paredes (:
            if (hit.CompareTag("Load"))
            {
                Rigidbody cargoRb = hit.GetComponent<Rigidbody>();
                if (cargoRb != null)
                {
                    // Creamos el Spring Joint en la carga y lo conectamos al dron
                    currentCable = hit.gameObject.AddComponent<SpringJoint>();
                    currentCable.connectedBody = droneRb;

                    // Apagamos la configuración automática para que no se vuelva loco ni pierda colisión
                    currentCable.autoConfigureConnectedAnchor = false;

                    // De dónde cuelga en el dron (ahora usamos el transform visual 'cableOrigin' en coordenadas locales del dron)
                    currentCable.connectedAnchor = transform.InverseTransformPoint(cableOrigin.position);

                    // De dónde se agarra en la carga (ahora calcula la parte superior exacta del objeto)
                    currentCable.anchor = Vector3.up * hit.bounds.extents.y;

                    currentCable.spring = springForce;
                    currentCable.damper = springDamper;

                    // Aseguramos que tire para acercarse pero respetando el límite máximo
                    currentCable.minDistance = 0f;
                    currentCable.maxDistance = cableLength;

                    // activar cable
                    lineRenderer.enabled = true;

                    break; // Solo  engancha la primera 
                }
            }
        }
    }

    private void ReleaseHook()
    {
        // Para soltar la carga destruimos el joint y au
        if (currentCable != null)
        {
            Destroy(currentCable);

            // apagar cable
            lineRenderer.enabled = false;
        }
    }

    private void Update()
    {
        // actualizar el cable en tiuempo real
        if (currentCable != null && lineRenderer.enabled)
        {
            // Punto 0: Dónde está enganchado en el Dron (ahora desde tu objeto 'cableOrigin')
            Vector3 startPoint = cableOrigin.position;

            // Punto 1: Dónde está enganchado en la Carga (coordenadas del mundo)
            // currentCable.gameObject es la carga. TransformPoint convierte el ancla local a mundo.
            Vector3 endPoint = currentCable.gameObject.transform.TransformPoint(currentCable.anchor);

            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
        }
        else if (lineRenderer.enabled)
        {
            // Por seguridad, si el joint se rompe apagamos la línea
            lineRenderer.enabled = false;
        }
    }

    // Para ver el gancho
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.TransformPoint(hookOffset), hookRadius);
    }
}