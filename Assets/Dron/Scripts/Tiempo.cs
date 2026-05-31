using System;
using UnityEngine;

public class Tiempo : MonoBehaviour
{
    [Header("Tiempo por entrega (segundos)")]
    [SerializeField] private float timePerDelivery = 60f;
 
    private float timeRemaining;
    private bool  isRunning;
 
    public float TimeRemaining => timeRemaining;
    public float TimePerDelivery => timePerDelivery;
    // Porcentaje de 0 a 1 para una barra de progreso
    public float TimeRatio => timePerDelivery > 0 ? timeRemaining / timePerDelivery : 0f;
 
    // Se dispara cuando el tiempo llega a 0
    public event Action OnTimeOut;
    // Se dispara cada frame con el tiempo restante (para actualizar el HUD)
    public event Action<float> OnTimeTick;
 
    public void StartTimer()
    {
        timeRemaining = timePerDelivery;
        isRunning     = true;
    }
 
    public void StopTimer()
    {
        isRunning = false;
    }
 
    public void ResetTimer()
    {
        timeRemaining = timePerDelivery;
        isRunning     = false;
    }
 
    private void Update()
    {
        if (!isRunning) return;
 
        timeRemaining -= Time.deltaTime;
        OnTimeTick?.Invoke(timeRemaining);
 
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning     = false;
            OnTimeOut?.Invoke();
        }
    }
}
