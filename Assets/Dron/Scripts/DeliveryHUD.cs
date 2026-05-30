using TMPro;
using UnityEngine;

public class DeliveryHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text hudText;
    [SerializeField] private DeliveryManager deliveryManager;
 
    private void Awake()
    {
        if (deliveryManager == null)
            deliveryManager = FindFirstObjectByType<DeliveryManager>();
    }
 
    private void OnEnable()
    {
        deliveryManager.OnHUDTextChanged += UpdateText;
    }
 
    private void OnDisable()
    {
        deliveryManager.OnHUDTextChanged -= UpdateText;
    }
 
    private void Start()
    {
        // Mostrar el texto inicial al arrancar
        UpdateText(deliveryManager.GetHUDText());
    }
 
    private void UpdateText(string text)
    {
        if (hudText != null)
            hudText.text = text;
    }
}
