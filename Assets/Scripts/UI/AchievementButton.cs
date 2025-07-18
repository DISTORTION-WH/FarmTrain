using UnityEngine;
using UnityEngine.Audio;

public class AchievementButton : MonoBehaviour, IUIManageable
{
    public AudioSource audioSource;        // �������� �����
    public AudioClip closeSound;           // ���� ��������
    bool isOpen = false;
    [SerializeField] GameObject windowAchievement;

    void Start()
    {
        if (ExclusiveUIManager.Instance != null)
        {
            ExclusiveUIManager.Instance.Register(this);
        }
        if (windowAchievement != null)
        {
            windowAchievement.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (ExclusiveUIManager.Instance != null)
        {
            ExclusiveUIManager.Instance.Deregister(this);
        }
    }

    public void WindowHandler()
    {
        if (isOpen)
        {
            CloseUI();
        }
        else
        {
            ExclusiveUIManager.Instance.NotifyPanelOpening(this);
            GameStateManager.Instance.RequestPause(this);
            windowAchievement.SetActive(true);
            isOpen = true;
        }
    }

    public void CloseUI()
    {
        if (isOpen)
        {
            if (audioSource != null && closeSound != null)
            {
                audioSource.PlayOneShot(closeSound);
            }
            GameStateManager.Instance.RequestResume(this);
            windowAchievement.SetActive(false);
            isOpen = false;
        }
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}
