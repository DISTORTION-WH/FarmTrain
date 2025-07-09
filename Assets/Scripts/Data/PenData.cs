// PenRelatedData.cs
using UnityEngine;
using System.Collections.Generic;

// ������ ��� ������ ����������� ������ ��������� ������
[System.Serializable]
public class PenLevelData
{
    [Tooltip("���������� ������������� ������ �� ���� ������ (���������, �������, ���������� � �.�.).")]
    public Sprite penSprite;
    [Tooltip("������������ ����������� �� ���� ������.")]
    public int capacity;
    [Tooltip("�������-���������, ������� ����� ������ � �������� ��� �������� �� ���� �������.")]
    public ItemData requiredUpgradeItem; // null ��� ���������� ������
    [Tooltip("���� �� ���� ������� ��������� �������������� ���������?")]
    public bool providesAutoFeeding;
    public AudioClip upgradeApplySound;

}

// ������������ ��� ������ ���� �������� (��������, ��� �����)
// �������� � AnimalPenManager
[System.Serializable]
public class PenConfigData
{
    [Tooltip("��� ������ ���� �������� ���� �����.")]
    public AnimalData animalData;

    // ����� �������� �� ����� ��� ������
    [Tooltip("��� ������� SpriteRenderer, ������� ���������� ��� �����.")]
    public string penSpriteRendererName;
    [Tooltip("��� �������, ���� ����� ���������� ��������.")]
    public string animalParentName;
    [Tooltip("��� ����������, ������� ������������ ������������ ��������.")]
    public string placementAreaName;

    [Tooltip("������ ���� ��������� ������� ��������� ��� ����� ������.")]
    public List<PenLevelData> upgradeLevels;
}

// "�����" ������ ��� TrainPenController
public class PenRuntimeInfo
{
    public PenConfigData config;
    public SpriteRenderer penSpriteRenderer; // <<< ������ ������ SpriteRenderer, � �� Transform
    public Transform animalParent;
    public Collider2D placementArea;

    [HideInInspector] public int currentLevel = 0; // ������� ������� ��������� (������ � ������ upgradeLevels)
}