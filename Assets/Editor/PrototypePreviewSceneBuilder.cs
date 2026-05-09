using System.Linq;
using WaterGrow.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WaterGrow.EditorTools
{
    public static class PrototypePreviewSceneBuilder
    {
        private const string MainScenePath = "Assets/01_Scenes/MainScene.unity";

        [MenuItem("WaterGrow/Rebuild Prototype Preview Scene")]
        public static void RebuildMainScene()
        {
            Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

            DeleteIfExists("Systems");
            DeleteIfExists("PrototypePreviewCanvas");
            DeleteIfExists("Main Camera");
            DeleteIfExists("EventSystem");

            PrototypePreviewBootstrap bootstrap = Object.FindObjectOfType<PrototypePreviewBootstrap>();
            if (bootstrap == null)
            {
                GameObject bootstrapObject = new GameObject("PrototypePreviewBootstrap");
                bootstrap = bootstrapObject.AddComponent<PrototypePreviewBootstrap>();
            }

            bootstrap.RebuildPreviewScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("WaterGrow prototype preview scene rebuilt.");
        }

        public static void RebuildMainSceneBatch()
        {
            RebuildMainScene();
        }

        private static void DeleteIfExists(string objectName)
        {
            foreach (GameObject target in Object.FindObjectsOfType<GameObject>().Where(item => item.name == objectName).ToList())
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
