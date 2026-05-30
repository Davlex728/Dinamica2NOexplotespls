using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class GoofyDroneEventsController : MonoBehaviour
{
    // hay que aadir un collider arriba del dron pequeito por si vuelca que pierda o un boton de reset o algo porque si vuelca es injugable y no se puede recuperar el control del dron
    [Header("Propulsores (0: FL, 1: FR, 2: RL, 3: RR)")]
    public Transform[] thrusters = new Transform[4];
    public float thrustPower = 25f;

    [Header("Asistencias de Vuelo")]
    public float mainThrustPower = 15f; // Fuerza extra para el botón que activa todos los motores a la vez
    public float autoLevelForce = 15f;  // Fuerza para que intente quedarse recto

    [Header("Particulas")]
    public ParticleSystem[] thrusterParticles = new ParticleSystem[4];
    public float maxParticleEmission = 50f;

    // Variables internas para guardar si el botón está pulsado o no
    private float fl_input, fr_input, rl_input, rr_input; //Fran fl es front left y rl es rear left  etc por si acaso como las ruedas de la f1
    private float mainThrust_input; // Variable para el botón de empuje global
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        foreach (var ps in thrusterParticles)
        {
            if (ps != null)
            {
                var emission = ps.emission;
                emission.rateOverTime = 0f;
            }
        }
    }

    // El 'context' nos da la información de si la tecla se acaba de pulsar, mantener o soltar
    public void OnFrontLeft(InputAction.CallbackContext context) => fl_input = context.ReadValue<float>();
    public void OnFrontRight(InputAction.CallbackContext context) => fr_input = context.ReadValue<float>();
    public void OnRearLeft(InputAction.CallbackContext context) => rl_input = context.ReadValue<float>();
    public void OnRearRight(InputAction.CallbackContext context) => rr_input = context.ReadValue<float>();

    // Evento para el botón de empuje global (los 4 a la vez)
    public void OnMainThrust(InputAction.CallbackContext context) => mainThrust_input = context.ReadValue<float>();

    void FixedUpdate()
    {
        // Aplicamos el empuje individual
        ApplyThrust(0, fl_input, mainThrust_input);
        ApplyThrust(1, fr_input, mainThrust_input);
        ApplyThrust(2, rl_input, mainThrust_input);
        ApplyThrust(3, rr_input, mainThrust_input);

        // Aplicamos el empuje global si se está pulsando
        if (mainThrust_input > 0.05f)
        {
            for (int i = 0; i < 4; i++)
            {
                // Empuja los 4 propulsores al mismo tiempo
                Vector3 mainForce = transform.up * mainThrustPower * mainThrust_input;
                rb.AddForceAtPosition(mainForce, thrusters[i].position, ForceMode.Force);
            }
        }

        // Estabilizador para que no vuelquw
        // Calcula la diferencia entre la inclinación del dron y el cielo (Vector3.up)
        Vector3 correctionTorque = Vector3.Cross(transform.up, Vector3.up);
        // Aplica un giro (Torque) para corregir esa inclinación y que no vuelque tan fácil si quitas estoes injugable
        rb.AddTorque(correctionTorque * autoLevelForce, ForceMode.Acceleration);
    }

    private void ApplyThrust(int index, float individualInput, float globalInput)
    {
        float totalThrustIntent = Mathf.Clamp01(individualInput + globalInput);

        if (individualInput > 0.05f)
        {
            // Aplica la fuerza hacia arriba en la posición local del propulsor correspondiente
            Vector3 force = transform.up * thrustPower * individualInput; // asi no es de o a 100 el rpopulsor hay mas "control" por parte del jugador
            rb.AddForceAtPosition(force, thrusters[index].position, ForceMode.Force);
        }

        if (thrusterParticles.Length > index && thrusterParticles[index] != null)
        {
            var emission = thrusterParticles[index].emission;

            if (totalThrustIntent > 0.05f)
            {
                emission.rateOverTime = totalThrustIntent * maxParticleEmission;
            }
            else
            {
                emission.rateOverTime = 0f;
            }
        }
    }
}