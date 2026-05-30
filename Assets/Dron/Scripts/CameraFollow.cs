using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 localOffset = new Vector3(0, 5f, -8f);

    //  menor = mas rapido y tenso
    public float smoothTime = 0.15f;

    // Variable interna  para que SmoothDamp funcione ( inercia de la camara)
    private Vector3 currentVelocity = Vector3.zero;

    void LateUpdate()
    {
        if (target != null)
        {
            // esto es para que la camara no vuelque si el dron vuelca
            Vector3 flatEuler = new Vector3(0, target.eulerAngles.y, 0);
            Quaternion flatRotation = Quaternion.Euler(flatEuler);

            // Calculamos dónde DEBERÍA estar la cámara
            Vector3 desiredPosition = target.position + (flatRotation * localOffset);

            // interpolacion para suavizar la camara y que no sea horrososa (USANDO SMOOTHDAMP)
            // Esto garantiza que si el dron va a match 3, la cámara acelera para no perderlo nunca.
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref currentVelocity,
                smoothTime
            );

            //Esto por que a veces pierde el dron la camara
            transform.LookAt(target.position, Vector3.up);
        }
    }
}