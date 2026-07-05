using UnityEngine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        Debug.Log("Carregando cena: " + sceneName);
    }

    public void exitgame()
    {
        Application.Quit();
        Debug.Log("Saindo do jogo...");
    }

     

}

