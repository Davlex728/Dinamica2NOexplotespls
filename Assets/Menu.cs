using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnPlay()
    {
        SceneManager.LoadScene("Play");
    }

    public void OnQuit()
    {
        Application.Quit();
        
    }
}
