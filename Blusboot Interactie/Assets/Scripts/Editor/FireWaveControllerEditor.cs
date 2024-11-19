using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FireWaveController))]
public class FireWaveControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FireWaveController fireWaveController = (FireWaveController)target;
        if (GUILayout.Button("Start Fire Wave"))
        {
            fireWaveController.StartFireWave();
        }
    }
}
