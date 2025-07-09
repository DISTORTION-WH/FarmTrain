using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class LocomotiveController : MonoBehaviour
{
    // ��������� ������ �����: �� ���� ��������, ���� ����� � �������.
    public enum TrainState { Moving, DockedAtStation }

    private TrainState currentState;

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
            UIManager.Instance.ShowGoToStationButton(false); // �������� ������ ����� ���������

            // <<< ��� �������� ����������� >>>
            // �� �������� ���������, ��� ��������� � ���� �������.
            // ��� ������� XP � ���������� ������ ��� �������.
            ExperienceManager.Instance.EnterStation();

            UnityEngine.SceneManagement.SceneManager.LoadScene("Station_1");
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
    #endregion
}