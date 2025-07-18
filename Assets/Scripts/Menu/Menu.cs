using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private string secondSceneToLoad = "Initializer";
    [SerializeField] private AudioSource menuAudioSource;
    [SerializeField] private AudioClip menuMovingClip;
    [SerializeField] private AudioClip closeClip;

    void Awake()
    {
        if (menuAudioSource != null && menuMovingClip != null)
        {
            menuAudioSource.PlayOneShot(menuMovingClip);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void LoadGame()
    {
        if (menuAudioSource != null)
        {
            menuAudioSource.Stop();
        }
        Debug.Log("Loading scene: SampleScene");
        SceneManager.LoadScene(secondSceneToLoad);
    }
    
    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void OnSettingsCloseButtonClick()
    {
        if (menuAudioSource != null && closeClip != null)
        {
            menuAudioSource.PlayOneShot(closeClip);
        }
        updated_sound sound = FindObjectOfType<updated_sound>();
        if (sound != null)
        {
            sound.PlaySound();
        }

        Menu menu = FindObjectOfType<Menu>();
        if (menu != null)
        {
            menu.HideSettings();
        }
    }

    public void HideSettings()
    {
        Debug.Log("Loading scene: HideSettings");
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
             Application.Quit();
        #endif
    }
}