using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CraneHook : MonoBehaviour
{
   public const string CargoTag = "Load";
 
    [Header("Area del Gancho")]
    public float hookRadius = 3f;
    public Vector3 hookOffset = new Vector3(0, -1.5f, 0);
    public Transform cableOrigin;
 
    [Header("Fisicas del Cable (Joints)")]
    public float springForce = 50f;
    public float springDamper = 5f;
    public float cableLength = 4f;
 
    [Header("Visuales del Cable")]
    public float cableWidth = 0.05f;
    public Material cableMaterial;
 
    private Rigidbody droneRb;
    private SpringJoint currentCable;
    private LineRenderer lineRenderer;
 
    public GameObject CurrentCargo => currentCable != null ? currentCable.gameObject : null;
    public bool IsCarryingCargo => currentCable != null;

    public GameObject canvas;
    
    public event Action<GameObject> OnCargoPickedUp;
    public event Action<GameObject> OnCargoReleased;

    private void Awake()
    {
        canvas.SetActive(false);
    }

    void Start()
    {
        droneRb = GetComponent<Rigidbody>();
 
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = cableWidth;
        lineRenderer.endWidth = cableWidth;
 
        if (cableMaterial != null)
            lineRenderer.material = cableMaterial;
 
        if (cableOrigin == null)
            cableOrigin = transform;
    }
 
    public void OnToggleHook(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (currentCable == null) TryHook();
            else ReleaseHook();
        }
    }
 
    private void TryHook()
    {
        Vector3 hookPosition = transform.TransformPoint(hookOffset);
        Collider[] hits = Physics.OverlapSphere(hookPosition, hookRadius);
 
        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag(CargoTag)) continue;
 
            Rigidbody cargoRb = hit.attachedRigidbody;
            if (cargoRb == null) continue;
 
            currentCable = hit.gameObject.AddComponent<SpringJoint>();
            currentCable.connectedBody = droneRb;
            currentCable.autoConfigureConnectedAnchor = false;
            currentCable.connectedAnchor = transform.InverseTransformPoint(cableOrigin.position);
 
            // Usamos el collider directamente para evitar NullReference con hit.bounds
            currentCable.anchor = Vector3.up * hit.bounds.size.y * 0.5f;
 
            currentCable.spring = springForce;
            currentCable.damper = springDamper;
            currentCable.minDistance = 0f;
            currentCable.maxDistance = cableLength;
 
            lineRenderer.enabled = true;
            OnCargoPickedUp?.Invoke(hit.gameObject);
            break;
        }
    }
 
    private void ReleaseHook()
    {
        if (currentCable == null) return;
 
        GameObject releasedCargo = currentCable.gameObject;
        Destroy(currentCable);
        currentCable = null;
        lineRenderer.enabled = false;
        OnCargoReleased?.Invoke(releasedCargo);
    }
 
    private void Update()
    {
        if (currentCable != null && lineRenderer.enabled)
        {
            lineRenderer.SetPosition(0, cableOrigin.position);
            lineRenderer.SetPosition(1, currentCable.gameObject.transform.TransformPoint(currentCable.anchor));
        }
        else if (lineRenderer.enabled)
        {
            lineRenderer.enabled = false;
        }
    }

    public void OnPause()
    {
        Time.timeScale = 0f;
        canvas.SetActive(true);
    }
 
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.TransformPoint(hookOffset), hookRadius);
    }
}