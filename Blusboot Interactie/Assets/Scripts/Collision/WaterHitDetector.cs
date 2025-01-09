using UnityEngine;
using System.Collections.Generic;
using System;

public class WaterHitDetector : MonoBehaviour
{
    private ParticleSystem waterSpray;
    private List<ParticleCollisionEvent> collisionEvents;
    private FireController fireController;

    void Start()
    {
        collisionEvents = new List<ParticleCollisionEvent>();
        fireController = GetComponent<FireController>();
        if (fireController == null)
        {
            Debug.LogError("WaterHitDetector: No FireController found on the object.");
        }
    }

    void OnParticleCollision(GameObject other)
    {
        int playerID = 0;
        if (other.CompareTag("WaterSpray"))
        {
            // if the name of the waterspray is "WaterSprayP1" or "WaterSprayP2"
            if (other.name == "WaterSprayP1")
            {
                playerID = 1;
            }
            else if (other.name == "WaterSprayP2")
            {
                playerID = 2;
            }
            waterSpray = other.GetComponent<ParticleSystem>();
            int numCollisionEvents = waterSpray.GetCollisionEvents(gameObject, collisionEvents);
            print("WaterHitDetector: " + numCollisionEvents + " water particles hit the object with ID: " + playerID);
            if (fireController != null)
            {
                // Use the number of collision events as the amount of water hitting the object
                float extinguishAmount = numCollisionEvents;
                // Pass the extinguish amount to the FireController
                fireController.Extinguish(extinguishAmount, playerID);
            }
        }
    }
}
