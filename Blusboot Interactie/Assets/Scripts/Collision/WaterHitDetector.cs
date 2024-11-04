using UnityEngine;
using System.Collections.Generic;

public class WaterHitDetector : MonoBehaviour
{
    private ParticleSystem waterSpray;
    private List<ParticleCollisionEvent> collisionEvents;

    void Start()
    {
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void OnParticleCollision(GameObject other)
    {
        // Check if the colliding particles are from the WaterSpray
        if (other.CompareTag("WaterSpray"))
        {
            if (waterSpray == null)
            {
                waterSpray = other.GetComponent<ParticleSystem>();
            }

            int numCollisionEvents = waterSpray.GetCollisionEvents(gameObject, collisionEvents);

            for (int i = 0; i < numCollisionEvents; i++)
            {
                // Access collision details
                Vector3 collisionPoint = collisionEvents[i].intersection;
                Debug.Log("Water hit at position: " + collisionPoint);

                // Implement your logic here, e.g., apply damage, extinguish fire, etc.
            }
        }
    }
}
