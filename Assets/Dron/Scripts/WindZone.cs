using System.Collections.Generic;
using UnityEngine;

public class WindZone : MonoBehaviour
{
    [Header("Configuración del Viento")]
    public float windForce = 15f;

    // Si es true, empuja según la rotación de este objeto (su eje Z/Forward).
    // Si es false, puedes definir un vector global.
    public bool useTransformDirection = true;
    public Vector3 customWindDirection = Vector3.right; //esatria guapo oner un gizmo para saber hacia donde va el viento


    // Lista para si hya muchos objetos
    private List<Rigidbody> rigidbodiesInZone = new List<Rigidbody>();

    private void OnTriggerEnter(Collider other)
    {
        // Si el objeto que entra tiene un Rigidbody  lo añadimos a la lista poner kinematico a las paredes y eso para que no se muevan
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !rb.isKinematic && !rigidbodiesInZone.Contains(rb))
        {
            rigidbodiesInZone.Add(rb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Cuando sale, lo quitamos de la lista
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && rigidbodiesInZone.Contains(rb))
        {
            rigidbodiesInZone.Remove(rb);
        }
    }

    private void FixedUpdate()
    {
        // Calculamos la dirección del viento una vez por frame
        Vector3 direction = useTransformDirection ? transform.forward : customWindDirection.normalized;

        // Aplicamos la fuerza constante a todos los objetos en la zona
        // Al reves para que si se borra un objeto de la lista no de error y se recorre toda la lista sin problemas 
        for (int i = rigidbodiesInZone.Count - 1; i >= 0; i--)
        {
            Rigidbody rb = rigidbodiesInZone[i];

            if (rb != null)
            {
                // simula el viento mas menos
                rb.AddForce(direction * windForce, ForceMode.Force);
            }
            else
            {
                rigidbodiesInZone.RemoveAt(i);
            }
        }
    }
    private void OnDrawGizmos() // temporal
    {
        // Determinamos la dirección igual que en el código
        Vector3 direction = useTransformDirection ? transform.forward : customWindDirection.normalized;

        // El tamaño de la flecha depende de la fuerza del viento (escalado para que no sea gigante)
        float arrowLength = Mathf.Clamp(windForce * 0.2f, 2f, 10f);
        Vector3 endPoint = transform.position + (direction * arrowLength);

        Gizmos.color = new Color(1f, 0f, 0f, 0.6f); // Rojo semitransparente

        // Dibujamos el palo de la flecha
        Gizmos.DrawLine(transform.position, endPoint);

        // Dibujamos la punta de la flecha
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + 20, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - 20, 0) * new Vector3(0, 0, 1);

        Gizmos.DrawLine(endPoint, endPoint + right * 1.5f);
        Gizmos.DrawLine(endPoint, endPoint + left * 1.5f);
    }
}
