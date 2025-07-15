using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;
using UnityEngine.UI;

public class EndCreditsController : MonoBehaviour
{
    [Header("����������")]
    [Tooltip("��������� RawImage, �� ������� ����� ������������ �����")]
    [SerializeField] private RawImage videoRawImage;

    [Tooltip("����� ����� ��� ����")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("�����")]
    [Tooltip("����������� ����, ������� ����� ������ �� ����� ������")]
    [SerializeField] private AudioClip creditsMusic;

    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private float musicFadeOutTime = 1.5f;

    // ����, ����� ������������� ������� ����� ������ �� �����
    private bool isExiting = false;

    void Start()
    {
        // --- 1. �������� ����������� ---
        if (videoPlayer == null || videoRawImage == null)
        {
            Debug.LogError("�� ��� ���������� ��������� � EndCreditsController! ��������� 'Video Raw Image' � 'Video Player'.");
            enabled = false; // ��������� ������, ����� �������� ������
            return;
        }

        // --- 2. ��������� ����� � ������ ---
        videoPlayer.isLooping = false;

        if (musicAudioSource == null) musicAudioSource = gameObject.AddComponent<AudioSource>();
        if (creditsMusic != null)
        {
            musicAudioSource.clip = creditsMusic;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }

        videoPlayer.Play();

        // --- 3. ������������� �� ������� ��������� ����� ---
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void Update()
    {
        // ��������� ������ ���������� ����� � ����� ������
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitGame();
        }
    }

    /// <summary>
    /// ���� ����� ���������� �������������, ����� ����� �������������.
    /// </summary>
    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("����� ���������. ����� �� ����.");
        ExitGame();
    }

    /// <summary>
    /// ���������� ����� �� ����.
    /// </summary>
    public void ExitGame()
    {
        if (isExiting) return;
        isExiting = true;

        videoPlayer.loopPointReached -= OnVideoFinished;

        StartCoroutine(FadeOutAndQuit());
    }

    /// <summary>
    /// �������� ��� �������� ��������� ������ � ������� ������ �� ����.
    /// </summary>
    private IEnumerator FadeOutAndQuit()
    {
        // ������ ������ ������
        if (musicAudioSource != null && musicAudioSource.isPlaying)
        {
            float startVolume = musicAudioSource.volume;
            float timer = 0f;
            while (timer < musicFadeOutTime)
            {
                timer += Time.deltaTime;
                musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, timer / musicFadeOutTime);
                yield return null;
            }
            musicAudioSource.Stop();
        }

        // <<< ������� ��������� ����� >>>
        Debug.Log("����� �� ����������...");

        // ��� ����������� ��������� �������� � � ���������, � � ������� ����.
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}