using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
 


    public override void OnInspectorGUI()
    {
        // Update the serialized object
        serializedObject.Update();

        // Draw the default inspector
        DrawDefaultInspector();

        MapGenerator mapGenerator = (MapGenerator)target;

        // Ensure only one MapGenerator is selected
     
            // Add a button to the inspector
            if (GUILayout.Button("Generate Map"))
            {
                mapGenerator.GenerateChunks();
            }
        
      

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}
