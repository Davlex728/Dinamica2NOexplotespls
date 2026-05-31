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
    public float mainThrustPower = 15f;
    public float autoLevelForce = 15f;

    [Header("Particulas")]
    public ParticleSystem[] thrusterParticles = new ParticleSystem[4];
    public float maxParticleEmission = 50f;

    [Header("Rotación (Yaw)")]
    public float yawTorque = 5f;

    private float fl_input, fr_input, rl_input, rr_input;
    private float mainThrust_input;
    private float yaw_input;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        foreach (var ps in thrusterParticles)
        {
            if (ps != null)
            {
                var emission = ps.emission;
                emission.rateOverTime = 0f;
            }
        }
    }

    public void OnFrontLeft(InputAction.CallbackContext context) => fl_input = context.ReadValue<float>();
    public void OnFrontRight(InputAction.CallbackContext context) => fr_input = context.ReadValue<float>();
    public void OnRearLeft(InputAction.CallbackContext context) => rl_input = context.ReadValue<float>();
    public void OnRearRight(InputAction.CallbackContext context) => rr_input = context.ReadValue<float>();

    public void OnMainThrust(InputAction.CallbackContext context) => mainThrust_input = context.ReadValue<float>();

    public void OnYaw(InputAction.CallbackContext context)
    {
        Vector2 inputVec = context.ReadValue<Vector2>();
        yaw_input = inputVec.x;
    }

    void FixedUpdate()
    {
        ApplyThrust(0, fl_input, mainThrust_input);
        ApplyThrust(1, fr_input, mainThrust_input);
        ApplyThrust(2, rl_input, mainThrust_input);
        ApplyThrust(3, rr_input, mainThrust_input);

        if (mainThrust_input > 0.05f)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 mainForce = transform.up * mainThrustPower * mainThrust_input;
                rb.AddForceAtPosition(mainForce, thrusters[i].position, ForceMode.Force);
            }
        }

        Vector3 correctionTorque = Vector3.Cross(transform.up, Vector3.up);
        rb.AddTorque(correctionTorque * autoLevelForce, ForceMode.Acceleration);

        if (Mathf.Abs(yaw_input) > 0.05f)
        {
            rb.AddRelativeTorque(Vector3.up * yaw_input * yawTorque, ForceMode.Acceleration);
        }
    }

    private void ApplyThrust(int index, float individualInput, float globalInput)
    {
        float totalThrustIntent = Mathf.Clamp01(individualInput + globalInput);

        if (individualInput > 0.05f)
        {
            Vector3 force = transform.up * thrustPower * individualInput;
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