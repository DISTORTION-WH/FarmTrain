using UnityEngine;
using System.Collections.Generic;

// ������������ ��� ������� ������
public enum QuestStatus
{
    NotAccepted,
    Accepted,
    Completed
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    [Header("Info")]
    public string id; // ���������� ������������� ������, �������� "01_FirstHarvest"
    public string title;
    [TextArea(4, 10)]
    public string description;
    [TextArea(4, 10)]
    public string shortDescription;

    [Header("Progression")]
    [Tooltip("� ������ ������ ���� ��������� ����� (1, 2, 3...)")]
    public int gameLevel; // �������� stationId
    [Tooltip("� ����� ���� ��������� ����� (����� ��� �������)")]
    public GamePhase phase; // ����� ����
    [Tooltip("�����, ������� �������� ����� ���������� �����. �������� ������, ���� ��� ��������� � �������.")]
    public Quest nextQuest; // ����� ����� ����������� ����� �����

    [Header("Goals & Rewards")]
    public List<QuestGoal> goals;
    public int rewardXP; // ������� � �����

    // ��� ���� ����������� QuestManager'��
    [HideInInspector] public QuestStatus status = QuestStatus.NotAccepted;
    [HideInInspector] public bool isPinned = false; // ������������� �� �����
    [HideInInspector] public bool hasBeenViewed = false; // ���������� �� ����� ����� � �������
}