using UnityEngine;

public class ResourceLoader : IResourceLoader
{
    public GameObject LoadPrefab(string path)
    {
        var prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            throw new System.Exception($"Failed to load prefab at path: {path}");
        }
        return prefab;
    }
}