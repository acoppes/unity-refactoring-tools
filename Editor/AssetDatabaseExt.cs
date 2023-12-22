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

            // var searchFilter = GetSearchFilter<T>();
            // var guids = AssetDatabase.FindAssets(searchFilter, folders);
            // return guids.Select(g => AssetDatabase.LoadAssetAtPath<T>(
            //     AssetDatabase.GUIDToAssetPath(g))).ToList();
        }
        
        public static List<Object> FindAssets(Type type, string[] folders = null)
        {
            var searchFilter = GetSearchFilter(type);
            var guids = AssetDatabase.FindAssets(searchFilter, folders);
            return guids.Select(g => AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(g), type)).ToList();
        }

        public static List<GameObject> FindPrefabs<T>(FindOptions options = 0, string[] folders = null)
        {
            return FindPrefabs(new []{typeof(T)}, options, folders);
        }
        
        public static List<GameObject> FindPrefabs<T1, T2>(FindOptions options = 0, string[] folders = null)
        {
            return FindPrefabs(new []{typeof(T1), typeof(T2)} , options, folders);
        }
        
        public static List<GameObject> FindPrefabs<T1, T2, T3>(FindOptions options = 0, string[] folders = null)
        {
            return FindPrefabs(new []{typeof(T1), typeof(T2), typeof(T3)} , options, folders);
        }

        public static List<GameObject> FindPrefabs(IEnumerable<Type> types, FindOptions options, string[] folders)
        {
            var considerChildren = options.HasFlag(FindOptions.ConsiderChildren) || options.HasFlag(FindOptions.ConsiderInactiveChildren);
            var considerDisabled = options.HasFlag(FindOptions.ConsiderInactiveChildren);

            var guids = AssetDatabase.FindAssets("t:Prefab", folders);

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