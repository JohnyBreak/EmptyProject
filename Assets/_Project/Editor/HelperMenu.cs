using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor
{
    public static class HelperMenu
    {
        private const string CoreSceneName = "CoreScene";
        private const string StartSceneName = "StartScene";
        
        [MenuItem("Helper/Scene Navigation/Go To Core Scene")]
        public static void GoToCoreScene()
        {
            if (Application.isPlaying)
            {
                SceneManager.LoadScene(CoreSceneName);
                Debug.Log($"[Helper] Switched to {CoreSceneName} (Play Mode)");
            }
            else
            {
                string scenePath = FindScenePath(CoreSceneName);
                if (!string.IsNullOrEmpty(scenePath))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(scenePath);
                        Debug.Log($"[Helper] Opened {CoreSceneName} (Edit Mode)");
                    }
                }
                else
                {
                    Debug.LogError($"[Helper] Scene '{CoreSceneName}' not found in Build Settings!");
                }
            }
        }

        [MenuItem("Helper/Scene Navigation/Go To Start Scene")]
        public static void GoToStartScene()
        {
            if (Application.isPlaying)
            {
                SceneManager.LoadScene(StartSceneName);
                Debug.Log($"[Helper] Switched to {StartSceneName} (Play Mode)");
            }
            else
            {
                string scenePath = FindScenePath(StartSceneName);
                if (!string.IsNullOrEmpty(scenePath))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(scenePath);
                        Debug.Log($"[Helper] Opened {StartSceneName} (Edit Mode)");
                    }
                }
                else
                {
                    Debug.LogError($"[Helper] Scene '{StartSceneName}' not found in Build Settings!");
                }
            }
        }

        [MenuItem("Helper/Save System/Clear All Save Data")]
        public static void ClearAllSaveData()
        {
            if (EditorUtility.DisplayDialog(
                "Clear Save Data", 
                "Are you sure you want to delete ALL save data?\n\nThis action cannot be undone!", 
                "Yes, Delete All", 
                "Cancel"))
            {
                string savePath = Path.Combine(Application.persistentDataPath, "Saves");
                
                try
                {
                    if (Directory.Exists(savePath))
                    {
                        Directory.Delete(savePath, true);
                        Debug.Log($"[Helper] Deleted all save data from: {savePath}");
                        
                        EditorUtility.DisplayDialog(
                            "Save Data Cleared", 
                            $"All save data has been deleted successfully!\n\nPath: {savePath}", 
                            "OK");
                    }
                    else
                    {
                        Debug.Log("[Helper] Save directory doesn't exist, nothing to delete");
                        
                        EditorUtility.DisplayDialog(
                            "No Save Data", 
                            "No save data found to delete.", 
                            "OK");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[Helper] Failed to delete save data: {ex.Message}");
                    
                    EditorUtility.DisplayDialog(
                        "Error", 
                        $"Failed to delete save data:\n{ex.Message}", 
                        "OK");
                }
            }
        }

        [MenuItem("Helper/Save System/Open Save Data Folder")]
        public static void OpenSaveDataFolder()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "Saves");
            
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
                Debug.Log($"[Helper] Created save directory: {savePath}");
            }
            
            EditorUtility.RevealInFinder(savePath);
            Debug.Log($"[Helper] Opened save data folder: {savePath}");
        }

        [MenuItem("Helper/Save System/Show Save Data Info")]
        public static void ShowSaveDataInfo()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "Saves");
            
            if (!Directory.Exists(savePath))
            {
                EditorUtility.DisplayDialog(
                    "Save Data Info", 
                    "No save data directory found.\n\nSaves will be created at:\n" + savePath, 
                    "OK");
                return;
            }
            
            string[] saveFiles = Directory.GetFiles(savePath, "*.json");
            
            if (saveFiles.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "Save Data Info", 
                    "No save files found.\n\nPath: " + savePath, 
                    "OK");
            }
            else
            {
                string fileList = "";
                foreach (string file in saveFiles)
                {
                    string fileName = Path.GetFileName(file);
                    FileInfo fileInfo = new FileInfo(file);
                    fileList += $"• {fileName} ({fileInfo.Length} bytes)\n";
                }
                
                EditorUtility.DisplayDialog(
                    "Save Data Info", 
                    $"Found {saveFiles.Length} save file(s):\n\n{fileList}\nPath: {savePath}", 
                    "OK");
            }
        }

        [MenuItem("Helper/Development/Reload Current Scene")]
        public static void ReloadCurrentScene()
        {
            if (Application.isPlaying)
            {
                UnityEngine.SceneManagement.Scene currentScene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(currentScene.name);
                Debug.Log($"[Helper] Reloaded scene: {currentScene.name}");
            }
            else
            {
                UnityEngine.SceneManagement.Scene currentScene = EditorSceneManager.GetActiveScene();
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(currentScene.path);
                    Debug.Log($"[Helper] Reloaded scene: {currentScene.name}");
                }
            }
        }

        [MenuItem("Helper/Development/Clear Console")]
        public static void ClearConsole()
        {
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
            
            Debug.Log("[Helper] Console cleared");
        }
        
        private static string FindScenePath(string sceneName)
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    string name = Path.GetFileNameWithoutExtension(scene.path);
                    if (name == sceneName)
                    {
                        return scene.path;
                    }
                }
            }
            
            string[] sceneGuids = AssetDatabase.FindAssets($"{sceneName} t:Scene");
            foreach (string guid in sceneGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = Path.GetFileNameWithoutExtension(path);
                if (name == sceneName)
                {
                    Debug.LogWarning($"[Helper] Scene '{sceneName}' found in project but not in Build Settings. Consider adding it to Build Settings.");
                    return path;
                }
            }
            
            return null;
        }
        
        [MenuItem("Helper/Scene Navigation/Go To Core Scene", true)]
        public static bool ValidateGoToCoreScene()
        {
            return !string.IsNullOrEmpty(FindScenePath(CoreSceneName));
        }

        [MenuItem("Helper/Scene Navigation/Go To Start Scene", true)]
        public static bool ValidateGoToStartScene()
        {
            return !string.IsNullOrEmpty(FindScenePath(StartSceneName));
        }
    }
}