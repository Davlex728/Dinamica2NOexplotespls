using System;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public const string CargoTag = "Load";
 
    [Header("Mision")]
    [SerializeField] private int totalDeliveries = 3;
 
    [Header("Referencias")]
    [SerializeField] private CraneHook craneHook;
    [SerializeField] private GameObject boxTemplate;
 
    [Header("Zonas de recogida — arrastra aqui los Emptys")]
    [SerializeField] private Transform[] pickupPoints;
 
    [Header("Zonas de entrega — arrastra aqui los Emptys")]
    [SerializeField] private DeliveryZone[] deliveryZones;
    [SerializeField] private Transform[] deliveryPoints;
    [SerializeField] private float deliveryZoneRadius = 3f;
 
    [Header("Rayos (recogida y entrega)")]
    [SerializeField] private float pickupBeamHeight   = 40f;
    [SerializeField] private float pickupBeamWidth    = 0.35f;
    [SerializeField] private Color pickupBeamColor    = new Color(1f, 0.92f, 0.2f, 0.9f);
 
    [Space]
    [SerializeField] private float deliveryBeamHeight = 40f;
    [SerializeField] private float deliveryBeamWidth  = 0.35f;
    [SerializeField] private Color deliveryBeamColor  = new Color(0.2f, 1f, 0.35f, 0.9f);
 
    private MissionRound[] rounds;
    private int  currentRoundIndex;
    private bool carryingCargo;
    private bool missionComplete;
    private bool isCompletingDelivery;
 
    public int  CompletedDeliveries => currentRoundIndex;
    public int  TotalDeliveries     => totalDeliveries;
    public bool IsMissionComplete   => missionComplete;
 
    public event Action<int, int> OnDeliveryProgress;
    public event Action           OnMissionCompleted;
 
    private sealed class MissionRound
    {
        public GameObject   pickupBox;
        public GuideBeam    pickupBeam;
        public DeliveryZone deliveryZone;
        public GuideBeam    deliveryBeam;
    }
 
    // =========================================================================
    // Unity lifecycle
    // =========================================================================
 
    private void Awake()
    {
        if (craneHook   == null) craneHook   = FindFirstObjectByType<CraneHook>();
        if (boxTemplate == null) boxTemplate = GameObject.FindGameObjectWithTag(CargoTag);
    }
 
    private void Start()
    {
        if (craneHook == null)
        {
            Debug.LogError("DeliveryManager: no se encontro CraneHook en la escena.");
            enabled = false;
            return;
        }
        if (boxTemplate == null)
        {
            Debug.LogError("DeliveryManager: no hay plantilla de caja con tag Load.");
            enabled = false;
            return;
        }
        if (pickupPoints == null || pickupPoints.Length == 0)
        {
            Debug.LogError("DeliveryManager: asigna al menos un Empty en Pickup Points.");
            enabled = false;
            return;
        }
        if (deliveryPoints == null || deliveryPoints.Length == 0)
        {
            Debug.LogError("DeliveryManager: asigna al menos un Empty en Delivery Points.");
            enabled = false;
            return;
        }
 
        BuildMissionRounds();
        craneHook.OnCargoPickedUp += HandleCargoPickedUp;
        craneHook.OnCargoReleased += HandleCargoReleased;
        BeginRound(0);
    }
 
    private void OnDestroy()
    {
        if (craneHook != null)
        {
            craneHook.OnCargoPickedUp -= HandleCargoPickedUp;
            craneHook.OnCargoReleased -= HandleCargoReleased;
        }
    }
 
    // =========================================================================
    // Construccion de rondas
    // =========================================================================
 
    private void BuildMissionRounds()
    {
        boxTemplate.SetActive(false);
 
        int roundCount = Mathf.Min(totalDeliveries, pickupPoints.Length, deliveryPoints.Length);
        rounds = new MissionRound[roundCount];
 
        for (int i = 0; i < roundCount; i++)
        {
            var     round       = new MissionRound();
            Vector3 pickupPos   = pickupPoints[i].position;
            Vector3 deliveryPos = deliveryPoints[i].position;
 
            // Raiz del punto de recogida
            var pickupRoot = new GameObject("PickupSpot_" + (i + 1));
            pickupRoot.transform.SetParent(transform, false);
            pickupRoot.transform.position = pickupPos;
 
            round.pickupBox = CreatePickupBox(pickupPos, i + 1);
            round.pickupBox.transform.SetParent(pickupRoot.transform, true);
 
            // Rayo amarillo (recogida)
            var pickupBeamObj = new GameObject("PickupBeam");
            pickupBeamObj.transform.SetParent(pickupRoot.transform, false);
            round.pickupBeam = pickupBeamObj.AddComponent<GuideBeam>();
            round.pickupBeam.Configure(pickupBeamHeight, pickupBeamWidth, pickupBeamColor);
            round.pickupBeam.SetTarget(pickupPos);
 
            // Zona + rayo de entrega
            round.deliveryZone = CreateDeliveryZone(deliveryPos, i + 1);
 
            var deliveryBeamObj = new GameObject("DeliveryBeam");
            deliveryBeamObj.transform.SetParent(round.deliveryZone.transform, false);
            round.deliveryBeam = deliveryBeamObj.AddComponent<GuideBeam>();
            round.deliveryBeam.Configure(deliveryBeamHeight, deliveryBeamWidth, deliveryBeamColor);
            round.deliveryBeam.SetTarget(deliveryPos);
 
            rounds[i] = round;
        }
 
        deliveryZones = new DeliveryZone[roundCount];
        for (int i = 0; i < roundCount; i++)
            deliveryZones[i] = rounds[i].deliveryZone;
    }
 
    private GameObject CreatePickupBox(Vector3 position, int index)
    {
        GameObject box = Instantiate(boxTemplate, position, boxTemplate.transform.rotation, transform);
        box.name = "PickupBox_" + index;
        box.tag  = CargoTag;
        box.SetActive(false);
 
        if (!box.TryGetComponent(out Rigidbody rb))
            rb = box.AddComponent<Rigidbody>();
        rb.mass       = 0.5f;
        rb.useGravity = true;
 
        if (!box.TryGetComponent(out BoxCollider _))
            box.AddComponent<BoxCollider>();
 
        return box;
    }
 
    private DeliveryZone CreateDeliveryZone(Vector3 position, int index)
    {
        var zoneObject = new GameObject("DeliveryZone_" + index);
        zoneObject.transform.SetParent(transform, false);
        zoneObject.transform.position = position;
 
        var sphereCollider       = zoneObject.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius    = deliveryZoneRadius;
 
        var zone = zoneObject.AddComponent<DeliveryZone>();
        zone.Initialize(this);
        return zone;
    }
 
    // =========================================================================
    // Logica de rondas
    // =========================================================================
 
    private void BeginRound(int roundIndex)
    {
        if (missionComplete || rounds == null || roundIndex >= rounds.Length) return;
 
        currentRoundIndex    = roundIndex;
        carryingCargo        = false;
        isCompletingDelivery = false;
 
        MissionRound round = rounds[roundIndex];
        ResetBox(round.pickupBox, pickupPoints[roundIndex].position);
 
        round.pickupBox.SetActive(true);
        round.pickupBeam.SetActive(true);
        round.deliveryBeam.SetActive(false);
 
        NotifyProgress();
    }
 
    private static void ResetBox(GameObject box, Vector3 position)
    {
        box.transform.SetPositionAndRotation(position, Quaternion.identity);
        if (box.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity  = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
 
    private void HandleCargoPickedUp(GameObject cargo)
    {
        if (missionComplete || !IsActiveRoundCargo(cargo)) return;
 
        carryingCargo = true;
        MissionRound round = rounds[currentRoundIndex];
        round.pickupBeam.SetActive(false);
        round.deliveryBeam.SetActive(true);
    }
 
    private void HandleCargoReleased(GameObject cargo)
    {
        if (missionComplete || !carryingCargo || !IsActiveRoundCargo(cargo)) return;
 
        if (rounds[currentRoundIndex].deliveryZone.GetComponent<Collider>()
                .bounds.Contains(cargo.transform.position))
        {
            CompleteCurrentDelivery(cargo);
        }
    }
 
    public void TryCompleteDelivery(DeliveryZone zone, GameObject cargo)
    {
        if (missionComplete || isCompletingDelivery || !carryingCargo || !IsActiveRoundCargo(cargo)) return;
        if (zone != rounds[currentRoundIndex].deliveryZone) return;
 
        CompleteCurrentDelivery(cargo);
    }
 
    private void CompleteCurrentDelivery(GameObject cargo)
    {
        if (isCompletingDelivery || missionComplete) return;
 
        isCompletingDelivery = true;
        carryingCargo        = false;
 
        MissionRound round = rounds[currentRoundIndex];
        round.deliveryBeam.SetActive(false);
        round.pickupBox.SetActive(false);
 
        int nextRound     = currentRoundIndex + 1;
        currentRoundIndex = nextRound;
        NotifyProgress();
 
        if (nextRound >= rounds.Length)
        {
            missionComplete = true;
            cargo.SetActive(false);
            OnMissionCompleted?.Invoke();
            Debug.Log("Mision completada! Has entregado las 3 cajas.");
            return;
        }
 
        BeginRound(nextRound);
    }
 
    private bool IsActiveRoundCargo(GameObject cargo)
    {
        return cargo != null
            && cargo.CompareTag(CargoTag)
            && rounds != null
            && currentRoundIndex < rounds.Length
            && cargo == rounds[currentRoundIndex].pickupBox;
    }
 
    private void NotifyProgress()
    {
        OnDeliveryProgress?.Invoke(currentRoundIndex, totalDeliveries);
        OnHUDTextChanged?.Invoke(GetHUDText());
    }
 
    public string GetHUDText()
    {
        if (missionComplete)
            return "Mision completada! " + totalDeliveries + "/" + totalDeliveries + " entregas";
 
        string state = carryingCargo
            ? "Lleva la caja al rayo VERDE"
            : "Ve al rayo AMARILLO y recoge la caja";
        int displayRound = Mathf.Min(currentRoundIndex + 1, totalDeliveries);
        return "Entrega " + displayRound + "/" + totalDeliveries + " - " + state;
    }
 
    public event Action<string> OnHUDTextChanged;
}
