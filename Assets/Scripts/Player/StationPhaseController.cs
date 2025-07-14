using UnityEngine;
using TMPro;

public class StationPhaseController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stationTitle;

    // ���� ������ ������ �� ������ ���������, �� ������ ���������

    void Start()
    {
        ExperienceManager.Instance.OnPhaseUnlocked += OnPhaseUnlocked;

        int currentLevel = ExperienceManager.Instance.CurrentLevel;
        if (stationTitle != null)
        {
            stationTitle.text = $"STATION {currentLevel}";
        }

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