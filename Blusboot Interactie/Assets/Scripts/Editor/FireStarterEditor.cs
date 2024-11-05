using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FireStarter))]
public class FireStarterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector GUI elements
        DrawDefaultInspector();

        // Get a reference to the FireStarter script
        FireStarter fireStarter = (FireStarter)target;

        // Add a space in the inspector
        EditorGUILayout.Space();

        // Create buttons in the inspector for each fire
        if (GUILayout.Button("Start Fire 1"))
        {
            fireStarter.StartFire1();
        }

        if (GUILayout.Button("Start Fire 2"))
        {
            fireStarter.StartFire2();
        }

        if (GUILayout.Button("Start Fire 3"))
        {
            fireStarter.StartFire3();
        }
    }
}
