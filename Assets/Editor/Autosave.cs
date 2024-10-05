using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class Autosave
{
   static Autosave()
   {
      EditorApplication.playModeStateChanged += (mode) =>
      {
         if (mode == PlayModeStateChange.ExitingEditMode)
         {
            Debug.Log("Auto-saving all open scenes...");
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
         }
      };
   }
}