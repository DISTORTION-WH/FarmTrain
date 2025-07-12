// SaveData.cs
using UnityEngine;
using System.Collections.Generic;

// ����������� ��� ������������ � JSON
[System.Serializable]
public class PlantSaveData
{
    public string plantDataName; // ��� ScriptableObject'� �������� ��� ��� ��������
    public string gridIdentifier; // � ������ GridGenerator'� ��������� ("GridGeneratorUp" ��� "GridGeneratorDown")
    public Vector2Int[] idSlots; // ����� ����� ��������

    public int currentStage; // ������� ������ ����� (int)
    public float growthTimer;
    public float waterNeedTimer;
    public bool isNeedWater;
    public bool isFertilize;
    public Vector3 currentposition;
}

[System.Serializable]
public class SlotSaveData
{
    public Vector2Int gridPosition;
    public bool isPlanted;
    public bool ishavebed;
    public bool isRaked;
    public bool isFertilize;
}

[System.Serializable]
public class GridSaveData
{
    public string identifier; // "GridGeneratorUp" ��� "GridGeneratorDown"
    public List<SlotSaveData> slotsData = new List<SlotSaveData>();
}

[System.Serializable]
public class GameSaveData
{
    // ��������� ���������
    public bool upgradeWatering;

    // ��������� ���� ����� � ��������
    public List<GridSaveData> gridsData = new List<GridSaveData>();
    public List<PlantSaveData> plantsData = new List<PlantSaveData>();
}