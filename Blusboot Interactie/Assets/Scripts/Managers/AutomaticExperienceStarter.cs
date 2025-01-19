using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticExperienceStarter : MonoBehaviour
{
    [Header("Benodigde Referenties")]
    public WaveManager waveManager;
    public PotentiometerObjectController2P potController;

    [Header("Instellingen")]
    [Tooltip("Tijd in seconden dat we moeten wachten nadat de ervaring is gestopt vóór hij weer mag aangaan.")]
    public float cooldownDuration = 5f;

    [Tooltip("Minimal absolute verschil voordat we een beweging als 'groot' beschouwen.")]
    public int movementThreshold = 50;

    private bool isCooldownActive = false;
    public bool canStartExperience = true;
    // Vooraf vullen we de 'oude' waardes van elke potmeter. 
    // We nemen als key de potIndex (1..4), en als value de laatst bekende waarde. 
    private Dictionary<int, int> lastPotValues = new Dictionary<int, int>()
    {
        {1, 0}, {2, 0}, {3, 0}, {4, 0}
    };

    private void OnEnable()
    {
        if (potController != null)
        {
            // Subscribe op de nieuwe event OnAnyPotChanged
            potController.OnAnyPotChanged += OnPotmeterChanged;
        }
    }

    private void OnDisable()
    {
        if (potController != null)
        {
            potController.OnAnyPotChanged -= OnPotmeterChanged;
        }
    }

    /// <summary>
    /// Deze methode wordt aangeroepen wanneer een potmeter verandert.
    /// </summary>
    private void OnPotmeterChanged(int potIndex, int newValue)
    {
        // Haal de oude waarde op
        int oldValue = lastPotValues[potIndex];
        // Bepaal het absolute verschil
        int diff = Mathf.Abs(newValue - oldValue);

        // Sla de nieuwe waarde op als 'oldValue' voor de volgende keer
        lastPotValues[potIndex] = newValue;

        // Als het verschil groter is dan onze threshold, beschouwen we dit als een echte beweging
        if (diff >= movementThreshold)
        {
            // Debug.Log($"Pot {potIndex} maakte een grote beweging: Δ={diff}. Check of we de ervaring kunnen starten.");
            // print(" can we start wave:" + waveManager.isExperienceActive);
            // print(canStartExperience);
            if (!waveManager.isExperienceActive && canStartExperience)
            {
                Debug.Log("Ervaring staat uit en geen cooldown actief => we starten de ervaring.");
                waveManager.StartExperience();
                waveManager.TurnOnWaterPlayers();
                canStartExperience = false;
                // StartCoroutine(CooldownRoutine());
            }
        }
    }

    public void StartCooldown()
    {
        StopAllCoroutines();
        StartCoroutine(EndExperience());
    }

    private IEnumerator EndExperience()
    {
        yield return new WaitForSeconds(cooldownDuration);
        canStartExperience = true;
    }
}
