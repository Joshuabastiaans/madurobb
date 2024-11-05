using UnityEngine;
using System.Collections.Generic;

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
        if (other.CompareTag("WaterSpray"))
        {
            if (waterSpray == null)
            {
                waterSpray = other.GetComponent<ParticleSystem>();
            }

            int numCollisionEvents = waterSpray.GetCollisionEvents(gameObject, collisionEvents);

            if (fireController != null)
            {
                // Use the number of collision events as the amount of water hitting the object
                float extinguishAmount = numCollisionEvents;
                // Pass the extinguish amount to the FireController
                fireController.Extinguish(extinguishAmount);
            }
        }
    }
}
