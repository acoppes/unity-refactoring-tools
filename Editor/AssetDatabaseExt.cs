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
            None = 0,
            ConsiderChildren = 1
        }
        
        public static List<T> FindAssets<T>(string[] folders = null) where T : Object
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T)}", folders);
            return guids.Select(g => AssetDatabase.LoadAssetAtPath<T>(
                AssetDatabase.GUIDToAssetPath((string)g))).ToList();
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

        public static void SortByRootPrefab(List<GameObject> prefabs)
        {
            // We sort by no variant prefabs first
            prefabs.Sort(delegate(GameObject a, GameObject b)
            {
                var aIsVariant = PrefabUtility.IsPartOfVariantPrefab(a);
                var bIsVariant = PrefabUtility.IsPartOfVariantPrefab(b);

                if (!aIsVariant && bIsVariant)
                    return -1;

                if (aIsVariant && !bIsVariant)
                    return 1;

                // if both no variants or both variants, we just use the name to compare just to be consistent.
                return a.name.CompareTo(b.name);
            });
        }

        public static List<GameObject> FindPrefabs(IEnumerable<Type> types, FindOptions options, string[] folders)
        {
            var considerChildren = options.HasFlag(FindOptions.ConsiderChildren);

            var guids = AssetDatabase.FindAssets("t:Prefab", folders);

            var prefabs = guids.Select(g => AssetDatabase.LoadAssetAtPath<GameObject>(
                AssetDatabase.GUIDToAssetPath(g))).ToList();

            if (considerChildren)
            {
                IEnumerable<GameObject> result = prefabs;
                // By default is the AND of all specified Types
                foreach (var type in types)
                {
                    result = result.Where(p => p.GetComponentInChildren(type) != null);
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