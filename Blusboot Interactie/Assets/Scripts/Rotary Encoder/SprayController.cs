using UnityEngine;

public class SprayController : MonoBehaviour
{
    public Transform sprayTransform;      // Reference to the spray's transform (Particle System)
    public float rotationStep = 1f;       // Amount to change rotationValue per increment

    private float rotationValue = 0f;     // Internal rotation value
    private ParticleSystem sprayParticleSystem;
    private ArduinoConnector arduinoConnector;

    private float initialYRotation;       // Stores the initial Y rotation
    private float minSprayAngle;          // Minimum angle
    private float maxSprayAngle;          // Maximum angle
    public float rotationRange = 45f;     // Range of rotation

    void Awake()
    {
        // Get the Particle System component
        if (sprayTransform != null)
        {
            sprayParticleSystem = sprayTransform.GetComponent<ParticleSystem>();

            // Get the initial Y rotation
            initialYRotation = sprayTransform.localRotation.eulerAngles.y;

            // Set min and max rotational values to the current Y rotation
            minSprayAngle = initialYRotation - rotationRange;
            maxSprayAngle = initialYRotation + rotationRange;
        }
        else
        {
            Debug.LogError("Spray Transform is not assigned.");
        }

        // Find the ArduinoConnector instance and subscribe to its events
        arduinoConnector = FindFirstObjectByType<ArduinoConnector>();
        if (arduinoConnector != null)
        {
            arduinoConnector.OnRotationChanged += HandleRotationChanged;
            arduinoConnector.OnSwitchPressed += HandleSwitchPressed;
        }
        else
        {
            Debug.LogError("ArduinoConnector not found in the scene.");
        }
    }

    void Update()
    {
        // Control the spray angle based on the rotation value
        if (sprayTransform != null)
        {
            // Calculate the new Y rotation
            float newYRotation = initialYRotation + rotationValue * rotationStep;
            print("y rotation: " + newYRotation);
            print("rotation value: " + rotationValue);
            // Clamp the newYRotation within min and max angles
            newYRotation = Mathf.Clamp(newYRotation, minSprayAngle, maxSprayAngle);

            // Apply rotation to the spray's transform around the Y axis
            sprayTransform.localRotation = Quaternion.Euler(0, 0, newYRotation);
        }
    }

    void HandleRotationChanged(int increment)
    {
        rotationValue += increment;
        // clamp the rotation value between min and max angles
        rotationValue = Mathf.Clamp(rotationValue, -rotationRange, rotationRange);

    }

    void HandleSwitchPressed()
    {
        if (sprayParticleSystem != null)
        {
            var emission = sprayParticleSystem.emission;
            emission.enabled = !emission.enabled;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from ArduinoConnector events
        if (arduinoConnector != null)
        {
            arduinoConnector.OnRotationChanged -= HandleRotationChanged;
            arduinoConnector.OnSwitchPressed -= HandleSwitchPressed;
        }
    }
}
