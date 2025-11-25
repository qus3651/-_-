using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartApp : MonoBehaviour
{
    public void OnRestartButtonClick()
    {
        SceneManager.LoadScene("Choice");
    }
}
