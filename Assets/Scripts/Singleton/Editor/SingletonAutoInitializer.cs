//------------------------------------------------------------
// Shrink Framework
// Author Eicy.
// Homepage: https://github.com/cneicy/ShrinkFramework
// Feedback: mailto:im@crash.work
//------------------------------------------------------------
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Singleton.Editor
{
    [InitializeOnLoad]
    public static class SingletonAutoInitializer
    {
        private static bool _isInitialized;

        static SingletonAutoInitializer()
        {
            EditorApplication.update += InitializeSingletons;
        }

        private static void InitializeSingletons()
        {
            if (_isInitialized || EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            
            if (!IsSceneLoaded())
            {
                EditorApplication.delayCall += InitializeSingletons;
                return;
            }

            _isInitialized = true;
            InitializeAllSingletonTypes();
        }

        private static bool IsSceneLoaded()
        {
            var scene = EditorSceneManager.GetActiveScene();
            return scene.IsValid() && scene.isLoaded;
        }

        private static void InitializeAllSingletonTypes()
        {
            var singletonType = typeof(Singleton<>);
            var targetTypes = new List<Type>();
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    targetTypes.AddRange(assembly.GetTypes()
                        .Where(IsConcreteSingleton));
                }
                catch (ReflectionTypeLoadException)
                {
                    // 忽略无法加载的类型
                }
            }

            foreach (var type in targetTypes)
            {
                CheckAndCreateSingleton(type);
            }
        }

        private static bool IsConcreteSingleton(Type type)
        {
            return type is { IsClass: true, IsAbstract: false, BaseType: { IsGenericType: true } }
                   && type.BaseType.GetGenericTypeDefinition() == typeof(Singleton<>);
        }

        private static void CheckAndCreateSingleton(Type singletonType)
        {
            if (Object.FindAnyObjectByType(singletonType) != null)
                return;
            
            if (ShouldSkipAutoCreate(singletonType))
                return;
            
            var go = new GameObject($"{singletonType.Name}");
            go.AddComponent(singletonType);
            
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"已自动创建单例 {singletonType.Name} 。");
        }

        private static bool ShouldSkipAutoCreate(Type type)
        {
            return type.GetCustomAttributes(typeof(DisableAutoCreateAttribute), true).Length > 0;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class DisableAutoCreateAttribute : Attribute { }
#endif
}