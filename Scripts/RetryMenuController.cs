using UnityEngine;
using UnityEngine.SceneManagement;

public class RetryMenuController : MonoBehaviour
{

    [SerializeField] private MainMenuController mainMenuController;

    private void OnEnable()
    {
        mainMenuController = FindObjectOfType<MainMenuController>();
        mainMenuController.OpenRetryMenu();
    }

    public void ReloadGame()
    {
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
        Time.timeScale = 1f;
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        Time.timeScale = 1f;
    }

}
