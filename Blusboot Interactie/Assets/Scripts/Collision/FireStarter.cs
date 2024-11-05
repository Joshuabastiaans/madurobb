using UnityEngine;

public class FireStarter : MonoBehaviour
{
    // References to three FireController instances
    public FireController fireController1;
    public FireController fireController2;
    public FireController fireController3;

    // Method to start Fire 1
    public void StartFire1()
    {
        if (fireController1 != null)
        {
            fireController1.StartFire();
            Debug.Log("Fire 1 has been started.");
        }
        else
        {
            Debug.LogError("FireStarter: FireController1 reference is not assigned.");
        }
    }

    // Method to start Fire 2
    public void StartFire2()
    {
        if (fireController2 != null)
        {
            fireController2.StartFire();
            Debug.Log("Fire 2 has been started.");
        }
        else
        {
            Debug.LogError("FireStarter: FireController2 reference is not assigned.");
        }
    }

    // Method to start Fire 3
    public void StartFire3()
    {
        if (fireController3 != null)
        {
            fireController3.StartFire();
            Debug.Log("Fire 3 has been started.");
        }
        else
        {
            Debug.LogError("FireStarter: FireController3 reference is not assigned.");
        }
    }
}
