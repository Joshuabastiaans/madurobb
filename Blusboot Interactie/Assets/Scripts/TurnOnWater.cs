using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class TurnOnWater : MonoBehaviour
{
    public bool isTurnedOnAtStart = false;
    bool isTurnedOn;
    ParticleSystem waterParticleSystem;
    void Start()
    {
        isTurnedOn = isTurnedOnAtStart;
        waterParticleSystem = GetComponent<ParticleSystem>();
        ChangeWaterActiveState();
    }

    public void TurnOn()
    {
        isTurnedOn = true;
        ChangeWaterActiveState();
    }

    public void TurnOff()
    {
        isTurnedOn = false;
        ChangeWaterActiveState();
    }
    void ChangeWaterActiveState()
    {
        if (isTurnedOn)
        {
            waterParticleSystem.Play();
        }
        else
        {
            waterParticleSystem.Stop();
        }
    }
}
