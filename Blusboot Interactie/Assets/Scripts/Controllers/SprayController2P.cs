using UnityEngine;
using System;

public class PotentiometerObjectController2P : MonoBehaviour
{
    public ArduinoConnector arduinoConnector;

    // --- Player 1 Pot Values ---
    private int potentiometerValue1 = 0; // Range: 0 to 1023
    private int potentiometerValue2 = 0; // Range: 0 to 1023

    // --- Player 2 Pot Values ---
    private int potentiometerValue3 = 0; // Range: 0 to 1023
    private int potentiometerValue4 = 0; // Range: 0 to 1023

    public WaveManager waveManager;

    [Header("Player 1 Target")]
    public Transform targetObjectP1; // Player 1's object to control

    [Header("Player 2 Target")]
    public Transform targetObjectP2; // Player 2's object to control
    public event Action<int, int> OnAnyPotChanged;
    void Awake()
    {
        // Find the ArduinoConnector
        arduinoConnector = FindAnyObjectByType<ArduinoConnector>();
        if (arduinoConnector != null)
        {
            // --- Subscribe to Player 1 pot events ---
            arduinoConnector.OnPotentiometer1Changed += HandlePotentiometer1Changed;
            arduinoConnector.OnPotentiometer2Changed += HandlePotentiometer2Changed;

            // --- Subscribe to Player 2 pot events ---
            arduinoConnector.OnPotentiometer3Changed += HandlePotentiometer3Changed;
            arduinoConnector.OnPotentiometer4Changed += HandlePotentiometer4Changed;
        }
        else
        {
            Debug.LogError("PotentiometerObjectController2P: ArduinoConnector not found in the scene.");
        }
    }

    // --- Player 1 Pot Updates ---
    void HandlePotentiometer1Changed(int value)
    {
        potentiometerValue1 = value;
        UpdatePlayer1Rotation();

        OnAnyPotChanged?.Invoke(1, value); // potIndex 1
    }

    void HandlePotentiometer2Changed(int value)
    {
        potentiometerValue2 = value;
        UpdatePlayer1Rotation();

        OnAnyPotChanged?.Invoke(2, value); // potIndex 2
    }

    void HandlePotentiometer3Changed(int value)
    {
        potentiometerValue3 = value;
        UpdatePlayer2Rotation();

        OnAnyPotChanged?.Invoke(3, value); // potIndex 3
    }

    void HandlePotentiometer4Changed(int value)
    {
        potentiometerValue4 = value;
        UpdatePlayer2Rotation();

        OnAnyPotChanged?.Invoke(4, value); // potIndex 4
    }

    // --- Apply Rotations to Each Player's Target ---

    void UpdatePlayer1Rotation()
    {
        if (targetObjectP1 != null)
        {
            // We consider this "player activity"
            waveManager.RegisterPlayerActivity();

            // Similar mapping as your original script
            float rotationAngleY = MapValue(potentiometerValue1, 0, 1023, 135f, -135f);
            float rotationAngleX = MapValue(potentiometerValue2, 0, 1023, 70f, -70f);

            // Apply rotation around X and Y axes
            targetObjectP1.localRotation = Quaternion.Euler(rotationAngleX, rotationAngleY, 0f);
        }
    }

    void UpdatePlayer2Rotation()
    {
        if (targetObjectP2 != null)
        {
            // We consider this "player activity" as well
            waveManager.RegisterPlayerActivity();

            // Same mapping logic for player 2
            float rotationAngleY = MapValue(potentiometerValue3, 0, 1023, 135f, -135f);
            float rotationAngleX = MapValue(potentiometerValue4, 0, 1023, 70f, -70f);

            targetObjectP2.localRotation = Quaternion.Euler(rotationAngleX, rotationAngleY, 0f);
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
            arduinoConnector.OnPotentiometer3Changed -= HandlePotentiometer3Changed;
            arduinoConnector.OnPotentiometer4Changed -= HandlePotentiometer4Changed;
        }
    }
}
