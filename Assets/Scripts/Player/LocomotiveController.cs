using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LocomotiveController : MonoBehaviour
{
    public static LocomotiveController Instance { get; private set; }

    [SerializeField] private AudioSource trainAudioSource;
    [SerializeField] private AudioClip trainMovingClip;
    [SerializeField] private float fadeDuration = 3.0f; // ������������ ��������� � ��������
    private Coroutine fadeOutCoroutine;
    [SerializeField] private AudioClip hornSound;
    // ��������� ������ �����: �� ���� ��������, ���� ����� � �������.
    public enum TrainState { Moving, DockedAtStation }

    public TrainState currentState;

    // ������ �� ������� �����
    public GameObject hornObject { get; private set; }
    private Animator hornAnimator;
    private AutoScrollParallax[] parallaxLayers;

    // ����� ���������
    private bool travelToStationUnlocked = false;
    private bool departureUnlocked = false;


    #region Unity Lifecycle
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (trainAudioSource == null)
            trainAudioSource = GetComponent<AudioSource>();
        FindSceneObjects();
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.OnPhaseUnlocked += OnPhaseUnlocked;
        }
    }

    void Start()
    {
        if (TransitionManager.isReturningFromStation)
        {
            TransitionManager.isReturningFromStation = false;
            OnReturnFromStation();
        }
        else
        {
            currentState = TrainState.Moving;
            CheckInitialTravelState();
        }
    }

    void LateUpdate()
    {
        UpdateHornHighlight();
        // ��������� ��������� UI � ����������� �� ���������
        UIManager.Instance.ShowGoToStationButton(currentState == TrainState.DockedAtStation);
        UpdateTrainSound();  // ��������� ���� ��������

    }
    private void UpdateTrainSound()
    {
        if (currentState == TrainState.Moving)
        {
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
                fadeOutCoroutine = null;
            }

            if (!trainAudioSource.isPlaying)
            {
                trainAudioSource.clip = trainMovingClip;
                trainAudioSource.loop = true;
                trainAudioSource.volume = 0.1f; // �� ������ ������ ������� ���������
                trainAudioSource.Play();
            }
        }
        else
        {
            // ���� ����� �� �������� � ���� ������ � ��������� ������� ���������
            if (trainAudioSource.isPlaying && fadeOutCoroutine == null)
            {
                fadeOutCoroutine = StartCoroutine(FadeOutSound());
            }
        }
    }
    private IEnumerator FadeOutSound()
    {
        float startVolume = trainAudioSource.volume;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            trainAudioSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
            yield return null;
        }

        trainAudioSource.Stop();
        trainAudioSource.volume = 0.3f; // ���������� ��������� ��� ���������� �������
        fadeOutCoroutine = null;
    }
    void OnDestroy()
    {
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.OnPhaseUnlocked -= OnPhaseUnlocked;
        }
    }
    #endregion

    #region Scene Object Management
    private void FindSceneObjects()
    {
        hornObject = GameObject.FindGameObjectWithTag("Horn");
        if (hornObject != null)
        {
            hornAnimator = hornObject.GetComponent<Animator>();
        }
        else Debug.LogError("[LocomotiveController] �� ������ ������ � ����� 'Horn'!");

        parallaxLayers = GameObject.FindGameObjectsWithTag("ParallaxLayer")
            .Select(go => go.GetComponent<AutoScrollParallax>()).Where(c => c != null).ToArray();
    }
    #endregion

    #region Event Handlers & Core Logic
    public void OnHornClicked()
    {
        if (trainAudioSource != null && hornSound != null)
        {
            trainAudioSource.PlayOneShot(hornSound);
        }
        switch (currentState)
        {
            case TrainState.Moving:
                if (travelToStationUnlocked)
                {
                    ArriveAtStation();
                }
                break;
            case TrainState.DockedAtStation:
                if (departureUnlocked)
                {
                    StartCoroutine(DepartSequence());
                }
                else
                {
                    Debug.Log("����� �����, �� ����������� �� ������� ��� �� ��������������.");
                }
                break;
        }
    }

    public void OnGoToStationButtonPressed()
    {
        if (currentState == TrainState.DockedAtStation)
        {
            var currentClip = RadioManager.Instance.audioSource.clip;
            var currentTime = RadioManager.Instance.audioSource.time;
            var wasPlaying = RadioManager.IsPlaying;

            RadioManager.Instance.radioPanel?.SetActive(false);

            UIManager.Instance.ShowGoToStationButton(false); // �������� ������ ����� ���������

            // <<< ��� �������� ����������� >>>
            // �� �������� ���������, ��� ��������� � ���� �������.
            // ��� ������� XP � ���������� ������ ��� �������.
            ExperienceManager.Instance.EnterStation();
            SceneManager.LoadScene("Station_1");
            StartCoroutine(LoadStationAndRestoreRadio(currentClip, currentTime, wasPlaying));
        }
    }

    private void OnPhaseUnlocked(int level, GamePhase phase)
    {
        if (phase == GamePhase.Train)
        {
            travelToStationUnlocked = true;
        }
        else if (phase == GamePhase.Station)
        {
            departureUnlocked = true;
            // ��������� ���� � TransitionManager, ����� �� ���������� ����� �������
            TransitionManager.isDepartureUnlocked = true;
        }
    }

    private void ArriveAtStation()
    {
        currentState = TrainState.DockedAtStation;
        // �� �� ���������� travelToStationUnlocked, ����� ��������� �����������

        foreach (var layer in parallaxLayers) layer.enabled = false;

        // �����: �� �� �������� AdvanceToNextPhase �����.
        // ����� ���� ���������� ������ ��� ������������� �����������.
    }

    private void OnReturnFromStation()
    {
        // ����� �� ������������, �� ��� ��� � ��������� "����������� � �������"
        currentState = TrainState.DockedAtStation;
        departureUnlocked = TransitionManager.isDepartureUnlocked;

        // ������������� ���, ��� ��� �� ��� ����������� ��� �������� �����
        foreach (var layer in parallaxLayers) layer.enabled = false;
    }

    private IEnumerator DepartSequence()
    {
        currentState = TrainState.Moving;
        travelToStationUnlocked = false;
        departureUnlocked = false;
        TransitionManager.isDepartureUnlocked = false;

        UpdateHornHighlight();
        UIManager.Instance.ShowGoToStationButton(false);

        yield return StartCoroutine(ScreenFaderManager.Instance.FadeOutAndInCoroutine(() =>
        {
            // --- �������� � �������� ���������� ---

            // 1. �������� ExperienceManager, ��� �� ������� �� ��������� �������
            ExperienceManager.Instance.DepartToNextTrainLevel();
            int newLevel = ExperienceManager.Instance.CurrentLevel; // �������� ����� �������

            // 2. �������� �������� ���������� � �������� �� � ����� ������
            foreach (var layer in parallaxLayers)
            {
                layer.enabled = true;
                layer.SetSpriteForLevel(newLevel); // <<< ��� �������� ���������
            }

            // --- ����� �������� ---
        }));
    }


    private void CheckInitialTravelState()
    {
        if (ExperienceManager.Instance.CurrentPhase == GamePhase.Train &&
            ExperienceManager.Instance.CurrentXP >= ExperienceManager.Instance.XpForNextPhase)
        {
            travelToStationUnlocked = true;
        }
    }

    private void UpdateHornHighlight()
    {
        if (hornAnimator == null) return;
        bool shouldHighlight = (currentState == TrainState.Moving && travelToStationUnlocked) ||
                               (currentState == TrainState.DockedAtStation && departureUnlocked);
        hornAnimator.SetBool("IsHighlighted", shouldHighlight);
    }

    private IEnumerator LoadStationAndRestoreRadio(AudioClip clip, float time, bool play)
    {
        yield return new WaitForSeconds(0.1f);

        RadioManager.Instance.audioSource.clip = clip;
        RadioManager.Instance.audioSource.time = time;
        if (play)
        {
            RadioManager.Instance.audioSource.Play();
            RadioManager.IsPlaying = true;
        }
    }
    #endregion
}