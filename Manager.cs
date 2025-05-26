using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
   
    

    public void Play()
    {
        SceneManager.LoadScene("Planets");
    }

     
    public void Exit()
    {
        Application.Quit();
    }

     
}
