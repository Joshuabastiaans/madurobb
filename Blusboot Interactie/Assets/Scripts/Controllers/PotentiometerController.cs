using UnityEngine;

public class PotentiometerController : MonoBehaviour
{
    private ArduinoConnector arduinoConnector;

    private int potentiometerValue1 = 0;
    private int potentiometerValue2 = 0;

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
            Debug.LogError("PotentiometerController: ArduinoConnector not found in the scene.");
        }
    }

    void HandlePotentiometer1Changed(int value)
    {
        potentiometerValue1 = value;
        // Use potentiometerValue1 as needed
        Debug.Log("Potentiometer 1 Value: " + potentiometerValue1);
    }

    void HandlePotentiometer2Changed(int value)
    {
        potentiometerValue2 = value;
        // Use potentiometerValue2 as needed
        Debug.Log("Potentiometer 2 Value: " + potentiometerValue2);
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
