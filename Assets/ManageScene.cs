using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Method to load a scene by name
    public void ChangeSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Method to load a scene by index
    public void ChangeSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // Optional: Example method to quit the game
    public void QuitGame()
    {   
        Application.Quit();
    }
}
