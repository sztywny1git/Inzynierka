using UnityEngine;

public interface IResourceLoader
{
    GameObject LoadPrefab(string path);
    T Load<T>(string path) where T : Object;
}