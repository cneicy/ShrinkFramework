//------------------------------------------------------------
// Shrink Framework
// Author Eicy.
// Homepage: https://github.com/cneicy/ShrinkFramework
// Feedback: mailto:im@crash.work
//------------------------------------------------------------
#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ScreenAspect.Editor
{
    [InitializeOnLoad]
    public static class AutoCameraAttachInitializer
    {
        private static bool _isInitialized;

        static AutoCameraAttachInitializer()
        {
            EditorApplication.update += Initialize;
        }

        private static void Initialize()
        {
            if (_isInitialized || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (!IsSceneReady()) 
            {
                EditorApplication.delayCall += Initialize;
                return;
            }

            _isInitialized = true;
            ProcessCameraAttachments();
        }

        private static bool IsSceneReady()
        {
            var scene = EditorSceneManager.GetActiveScene();
            return scene.IsValid() && scene.isLoaded;
        }

        private static void ProcessCameraAttachments()
        {
            var mainCamera = FindOrCreateMainCamera();
            if (mainCamera == null) return;

            foreach (var type in GetAttachableTypes())
            {
                AttachComponentIfMissing(mainCamera.gameObject, type);
            }
        }

        private static Camera FindOrCreateMainCamera()
        {
            var mainCamera = Camera.main;
            if (mainCamera != null) return mainCamera;

            // 自动创建带基本配置的主相机
            var cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            var camera = cameraGO.AddComponent<Camera>();
            cameraGO.AddComponent<AudioListener>();
        
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("相机自动创建已基本完成");
            return camera;
        }

        private static Type[] GetAttachableTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => {
                    try { return asm.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t => t.GetCustomAttribute<AutoAttachToMainCameraAttribute>() != null)
                .ToArray();
        }

        private static void AttachComponentIfMissing(GameObject target, Type componentType)
        {
            if (target.GetComponent(componentType) != null) return;

            var component = target.AddComponent(componentType);
            EditorUtility.SetDirty(target);
            Debug.Log($"自动挂载 {componentType.Name} 到 {target.name}");
        }
    }
}
#endif