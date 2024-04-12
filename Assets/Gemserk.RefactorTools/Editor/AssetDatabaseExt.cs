using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gemserk.RefactorTools.Editor
{
    public static class AssetDatabaseExt
    {
        [Flags]
        public enum FindOptions
        {
            None = 1 << 0,
            ConsiderChildren = 1 << 1,
            ConsiderInactiveChildren = 1 << 2
        }

        public static string GetSearchFilter<T>()
        {
            return GetSearchFilter(typeof(T));
        }
        
        public static string GetSearchFilter(Type type)
        {
            return $"t:{type.Name}";
        }
        
        public static List<T> FindAssets<T>(string[] folders = null) where T : Object
        {
            return FindAssets(typeof(T), folders).Select(t => t as T).ToList();
        }
        
        public static List<T> FindAssets<T>(string text, string[] folders = null) where T : Object
        {
            return FindAssets(typeof(T), text, folders).Select(t => t as T).ToList();
        }
        
        public static List<Object> FindAssets(Type type, string[] folders = null)
        {
            return FindAssets(type, null, folders);
        }
        
        public static List<Object> FindAssets(Type type, string text, string[] folders = null)
        {
            var searchFilter = GetSearchFilter(type);
            
            if (!string.IsNullOrEmpty(text))
            {
                searchFilter += $" {text}";
            }
            
            var guids = AssetDatabase.FindAssets(searchFilter, folders);
            return guids.Select(g => AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(g), type)).ToList();
        }
        
        /// <summary>
        /// Similar to FindAssets but the main difference is that it doesn't filter by Type before searching but
        /// after, so it will be a bit slower, finding all objects and then checking them if they match the type.
        /// </summary>
        /// <param name="type">The type the asset should match (can be an interface)</param>
        /// <param name="text">Text filters for the search pattern.</param>
        /// <param name="folders">The folders to lookout for assets.</param>
        /// <returns></returns>
        public static List<Object> FindAssetsAll(Type type, string text = null, string[] folders = null)
        {
            var assets = FindAssets(typeof(Object), text, folders);
            return assets.Where(type.IsInstanceOfType).ToList();
        }
        
        public static List<Object> FindAssetsAll<T>(string text = null, string[] folders = null)
        {
            return FindAssetsAll(typeof(T), text, folders);
        }

        public static List<GameObject> FindPrefabs<T>(FindOptions options = 0, string text = null, string[] folders = null)
        {
            return FindPrefabs(new []{typeof(T)}, options, text, folders);
        }
        
        public static List<GameObject> FindPrefabs<T1, T2>(FindOptions options = 0, string text = null, string[] folders = null)
        {
            return FindPrefabs(new []{typeof(T1), typeof(T2)} , options, text, folders);
        }
        
        public static List<GameObject> FindPrefabs<T1, T2, T3>(FindOptions options = 0, string text = null, string[] folders = null)
        {
            return FindPrefabs(new []{typeof(T1), typeof(T2), typeof(T3)}, options, text, folders);
        }

        public static List<GameObject> FindPrefabs(IEnumerable<Type> types, FindOptions options, string text, string[] folders)
        {
            try
            {
                EditorUtility.DisplayProgressBar($"Finding Prefabs with types", "Start", 0);
                
                var considerChildren = options.HasFlag(FindOptions.ConsiderChildren) || options.HasFlag(FindOptions.ConsiderInactiveChildren);
                var considerDisabled = options.HasFlag(FindOptions.ConsiderInactiveChildren);

                var searchFilter = "t:Prefab";
                
                if (!string.IsNullOrEmpty(text))
                {
                    searchFilter += $" {text}";
                }
                
                var guids = AssetDatabase.FindAssets(searchFilter, folders);

                var prefabs = guids.Select(g => AssetDatabase.LoadAssetAtPath<GameObject>(
                    AssetDatabase.GUIDToAssetPath(g))).ToList();

                if (considerChildren)
                {
                    IEnumerable<GameObject> result = prefabs;
                    // By default is the AND of all specified Types
                    foreach (var type in types)
                    {
                        result = result.Where(p => p.GetComponentInChildren(type, considerDisabled) != null);
                    }
                    return result.ToList();
                }
                else
                {
                    IEnumerable<GameObject> result = prefabs;
                    // By default is the AND of all specified Types
                    foreach (var type in types)
                    {
                        result = result.Where(p => p.GetComponent(type) != null);
                    }
                    return result.ToList();
                }
                
            } finally {
                EditorUtility.ClearProgressBar();
            }
        }

        public static List<string> FindAllScenes()
        {
            var guidList = new List<string>();
            
            guidList.AddRange(AssetDatabase.FindAssets("t:scene", new []
            {
                "Assets"
            }));
            
            return guidList.Select(AssetDatabase.GUIDToAssetPath).ToList();
        }
    }
}