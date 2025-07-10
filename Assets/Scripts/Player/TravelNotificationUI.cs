using UnityEngine;
using System.Collections;

public class TravelNotificationUI : MonoBehaviour
{
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private float displayDuration = 4.0f;
    [SerializeField] private AudioClip notificationSound;
    private AudioSource audioSource;

    private Coroutine displayCoroutine;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (ExperienceManager.Instance == null)
        {
            Debug.LogError("ExperienceManager �� ������!");
            gameObject.SetActive(false);
            return;
        }

        // ������������� �� ������� ������������� ��������
        ExperienceManager.Instance.OnPhaseUnlocked += ShowNotification;
        notificationPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.OnPhaseUnlocked -= ShowNotification;
        }
    }

    private void ShowNotification(int level, GamePhase phase)
    {
        // ���������� ����������� ������ ��� �������� � ������ �� �������
        if (phase == GamePhase.Train)
        {
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }
            if (audioSource != null && notificationSound != null)
            {
                audioSource.PlayOneShot(notificationSound);
            }
            displayCoroutine = StartCoroutine(ShowAndHidePanel());
        }
    }

    private IEnumerator ShowAndHidePanel()
    {
        notificationPanel.SetActive(true);
        yield return new WaitForSeconds(displayDuration);
        notificationPanel.SetActive(false);
        displayCoroutine = null;
    }
}