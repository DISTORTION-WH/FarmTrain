using UnityEngine;
using TMPro;

public class StationPhaseController : MonoBehaviour
{

    void Start()
    {
        ExperienceManager.Instance.OnPhaseUnlocked += OnPhaseUnlocked;

        int currentLevel = ExperienceManager.Instance.CurrentLevel;

        // ���� ��� ���� ���� ������� �� ����� ������ ���� (XP = 0)
        if (ExperienceManager.Instance.XpForNextPhase == 0)
        {
            // ����� �������� �������� ���������, ��� ����� �������
            TransitionManager.Instance.UnlockDeparture();
        }
    }

    void OnDestroy() // <<< �������� � OnDisable �� OnDestroy
    {
        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.OnPhaseUnlocked -= OnPhaseUnlocked;
        }
    }

    private void OnPhaseUnlocked(int level, GamePhase phase)
    {
        if (phase == GamePhase.Station)
        {
            // �������� �������� ���������, ��� ������ ����� ������������
            TransitionManager.Instance.UnlockDeparture();
        }
    }
}