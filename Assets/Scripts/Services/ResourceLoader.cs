using UnityEngine;


public class ResourceLoader : IResourceLoader
{
    public GameObject LoadPrefab(string path) => Resources.Load<GameObject>(path);
    public T Load<T>(string path) where T : Object => Resources.Load<T>(path);
}