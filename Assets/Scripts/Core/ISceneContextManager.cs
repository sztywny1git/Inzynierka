using System.Threading.Tasks;

public interface ISceneContextManager
{
    Task LoadSceneAsync(string sceneName);
}