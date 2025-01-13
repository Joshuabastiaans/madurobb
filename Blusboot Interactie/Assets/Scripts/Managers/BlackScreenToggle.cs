using UnityEngine;
using UnityEngine.UI; // Required for UI components like Image

public class BlackScreenToggle : MonoBehaviour
{
    // Reference to the black screen UI object
    public GameObject blackScreen;

    void Start()
    {
        // Ensure the black screen starts inactive if assigned
        if (blackScreen != null)
        {
            blackScreen.SetActive(false);
        }
        else
        {
            Debug.LogError("Black screen GameObject is not assigned in the inspector.");
        }
    }

    void Update()
    {
        // Toggle the black screen with the "B" key
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (blackScreen != null)
            {
                blackScreen.SetActive(!blackScreen.activeSelf);
            }
        }
    }
}
