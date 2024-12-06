using UnityEngine;

public class PotentiometerObjectController : MonoBehaviour
{
    private ArduinoConnector arduinoConnector;

    private int potentiometerValue1 = 0; // Range: 0 to 1023
    private int potentiometerValue2 = 0; // Range: 0 to 1023
    public WaveManager waveManager;
    public Transform targetObject; // Assign the object to control in the Inspector

    void Awake()
    {
        arduinoConnector = FindAnyObjectByType<ArduinoConnector>();
        if (arduinoConnector != null)
        {
            arduinoConnector.OnPotentiometer1Changed += HandlePotentiometer1Changed;
            arduinoConnector.OnPotentiometer2Changed += HandlePotentiometer2Changed;
        }
        else
        {
            Debug.LogError("PotentiometerObjectController: ArduinoConnector not found in the scene.");
        }
    }

    void HandlePotentiometer1Changed(int value)
    {
        potentiometerValue1 = value;
        UpdateObjectRotation();
    }

    void HandlePotentiometer2Changed(int value)
    {
        potentiometerValue2 = value;
        UpdateObjectRotation();
    }

    void UpdateObjectRotation()
    {
        if (targetObject != null)
        {
            waveManager.RegisterPlayerActivity();
            // Map potentiometer values to rotation angles (0 to 270 degrees)
            float rotationAngleY = MapValue(potentiometerValue1, 0, 1023, 135f, -135f);
            float rotationAngleX = MapValue(potentiometerValue2, 0, 1023, 70f, -70f);

            // Apply rotations around X and Y axes
            targetObject.localRotation = Quaternion.Euler(rotationAngleX, rotationAngleY, 0f);
        }
    }

    float MapValue(float value, float inMin, float inMax, float outMin, float outMax)
    {
        return Mathf.Lerp(outMin, outMax, (value - inMin) / (inMax - inMin));
    }

    void OnDestroy()
    {
        if (arduinoConnector != null)
        {
            arduinoConnector.OnPotentiometer1Changed -= HandlePotentiometer1Changed;
            arduinoConnector.OnPotentiometer2Changed -= HandlePotentiometer2Changed;
        }
    }
}
