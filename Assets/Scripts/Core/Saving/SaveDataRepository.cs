using UnityEngine;
using System.IO;

public class SaveDataRepository
{
    private readonly string _saveRootPath = Path.Combine(Application.persistentDataPath, "Saves");
    
    public SaveDataRepository()
    {
        Directory.CreateDirectory(_saveRootPath);
    }
    
    public bool TryLoad<T>(string fileName, out T data)
    {
        string path = Path.Combine(_saveRootPath, fileName);
        if (File.Exists(path))
        {
            data = JsonUtility.FromJson<T>(File.ReadAllText(path));
            return true;
        }
        data = default;
        return false;
    }
    
    public void Save<T>(string fileName, T data)
    {
        string path = Path.Combine(_saveRootPath, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, JsonUtility.ToJson(data));
    }

    public void Delete(string fileName)
    {
        string path = Path.Combine(_saveRootPath, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
    
    public bool Exists(string fileName) => File.Exists(Path.Combine(_saveRootPath, fileName));
}