using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DeliveryZone : MonoBehaviour
{
    [SerializeField] private float deliveryRadius = 3f;
     
        private DeliveryManager manager;
     
        // Ya no necesita recibir un marker: el GuideBeam de entrega lo gestiona DeliveryManager.
        public void Initialize(DeliveryManager deliveryManager)
        {
            manager = deliveryManager;
        }
     
        private void Awake()
        {
            Collider zoneCollider    = GetComponent<Collider>();
            zoneCollider.isTrigger   = true;
     
            if (zoneCollider is SphereCollider sphereCollider)
                sphereCollider.radius = deliveryRadius;
        }
     
        private void OnTriggerStay(Collider other)
        {
            if (manager == null || !other.CompareTag(DeliveryManager.CargoTag)) return;
     
            // La caja debe estar suelta (sin SpringJoint) para contar como entregada
            if (other.GetComponent<SpringJoint>() != null) return;
     
            manager.TryCompleteDelivery(this, other.gameObject);
        }
     
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 1f, 0.35f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, deliveryRadius);
        }
}
