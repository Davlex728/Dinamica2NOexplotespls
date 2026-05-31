using TMPro;
using UnityEngine;

public class DeliveryHUD : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TMP_Text hudText;
    [SerializeField] private TMP_Text timerText;      // Texto separado para el tiempo (opcional)
    [SerializeField] private DeliveryManager deliveryManager;
    [SerializeField] private Tiempo timer;
 
    private void Awake()
    {
        if (deliveryManager == null)
            deliveryManager = FindFirstObjectByType<DeliveryManager>();
        if (timer == null)
            timer = FindFirstObjectByType<Tiempo>();
    }
 
    private void OnEnable()
    {
        deliveryManager.OnHUDTextChanged += UpdateMissionText;
        timer.OnTimeTick                 += UpdateTimerText;
        timer.OnTimeOut                  += OnTimeOut;
    }
 
    private void OnDisable()
    {
        deliveryManager.OnHUDTextChanged -= UpdateMissionText;
        timer.OnTimeTick                 -= UpdateTimerText;
        timer.OnTimeOut                  -= OnTimeOut;
    }
 
    private void Start()
    {
        UpdateMissionText(deliveryManager.GetHUDText());
        UpdateTimerText(timer.TimeRemaining);
    }
 
    private void UpdateMissionText(string text)
    {
        if (hudText != null)
            hudText.text = text;
    }
 
    private void UpdateTimerText(float seconds)
    {
        if (timerText == null) return;
 
        // Formato MM:SS
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", m, s);
 
        // Ponemos el texto en rojo cuando queden menos de 10 segundos
        timerText.color = seconds <= 10f ? Color.red : Color.white;
    }
 
    private void OnTimeOut()
    {
        if (timerText != null)
            timerText.text = "00:00";
    }
}
