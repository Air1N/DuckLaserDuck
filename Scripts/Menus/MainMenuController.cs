using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject firstSelectedButton, settingsButton, volumeSlider, slotPullArm, quitButton, quitAccept, retryButton;
    [SerializeField] private LocalizeTexts localizationController;
    [SerializeField] private AudioClip defaultMusic;

    public void Start()
    {
        localizationController = FindObjectOfType<LocalizeTexts>();
        OpenMainMenu();
    }

    public void PlayGame()
    {
        GameObject.FindWithTag("music").GetComponent<AudioSource>().Stop();
        GameObject.FindWithTag("music").GetComponent<AudioSource>().PlayOneShot(defaultMusic);

        SceneManager.LoadScene("Game", LoadSceneMode.Single);
        Time.timeScale = 1;
    }

    public void Resume()
    {
        Time.timeScale = 1;
    }

    public void ExitToMenu()
    {
        GameObject.FindWithTag("music").GetComponent<AudioSource>().Stop();
        GameObject.FindWithTag("music").GetComponent<AudioSource>().PlayOneShot(defaultMusic);

        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        Time.timeScale = 1;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OpenMainMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void OpenSettingsMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(volumeSlider);

        localizationController.needsToFindLanguageDropdown = true;
    }

    public void CloseSettingsMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(settingsButton);

        localizationController.needsToFindLanguageDropdown = true;
    }

    public void OpenPauseMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(volumeSlider);
    }

    public void OpenUpgradeMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(slotPullArm);
    }

    public void OpenReallyQuit()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(quitAccept);
    }

    public void CloseReallyQuit()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(quitButton);
    }

    public void OpenRetryMenu()
    {
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(retryButton);
    }
}
