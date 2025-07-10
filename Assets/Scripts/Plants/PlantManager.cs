using System.Collections.Generic;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    public static PlantManager instance;

    public bool UpgradeWatering;
    public ItemData _UpgradeData;

    public Dictionary<int, List<Vector2Int>> positionBed;

    public static GameSaveData SessionData { get; private set; }


    public static bool ShouldLoadSessionData { get; set; } = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // ��������� ����������� ������, ���� ��� ����
        // � ���� �� ��������� �� �����, � ������� ����.
        if (ShouldLoadSessionData && SessionData != null)
        {
            this.UpgradeWatering = SessionData.upgradeWatering;
        }

        // ������������� ������� (���� ��� ����������, �� ����� ������)
        positionBed = new Dictionary<int, List<Vector2Int>>();
        positionBed.Add(0, new List<Vector2Int>());
        positionBed.Add(1, new List<Vector2Int>());
        // CkeckValue(); // ���� ����� ����� ������ ��� �������� ��� �������
    }


    public bool CompleteWateringUpgrade()
    {
        Debug.Log("Complete upgrade");
       UpgradeWatering = true;
        return true;
    }
    public void AddNewPositionBed(int level, Vector2Int posBed)
    {

       
        if (!positionBed.ContainsKey(level))
        {
           positionBed.Add(level, new List<Vector2Int>());
           
        }
        positionBed[level].Add(posBed);
    }
    
    public List<Vector2Int> GetValueFromDictionary(int key)
    {
        if(positionBed.TryGetValue(key, out List<Vector2Int> value))
          return value;  
        else  return null;
    }
    public void SaveStateToMemory()
    {
        Debug.Log("--- ������ ���������� ��������� � ������ ---");
        GameSaveData saveData = new GameSaveData();
        SessionData = saveData;

        // 1. ��������� ������ �� ������ PlantManager
        SessionData.upgradeWatering = this.UpgradeWatering;
        Debug.Log($"���������: UpgradeWatering = {SessionData.upgradeWatering}");

        // 2. ������� ��� GridGenerator'� �� �����
        GridGenerator[] gridGenerators = FindObjectsOfType<GridGenerator>();
        // --- ���������� ��� 1 ---
        Debug.Log($"������� {gridGenerators.Length} �������� GridGenerator �� �����.");

        foreach (var grid in gridGenerators)
        {
            // --- ���������� ��� 2 ---
            Debug.Log($"���������� ������ ��� �����: '{grid.gameObject.name}'");

            GridSaveData gridData = new GridSaveData { identifier = grid.gameObject.name };

            // �������� ������ � ������
            // --- ���������� ��� 3 ---
            Debug.Log($"� ����� '{grid.gameObject.name}' ������� {grid.gridObjects.Count} ������ � ������� ��� ����������.");
            foreach (var slotEntry in grid.gridObjects)
            {
                SlotScripts slotScript = slotEntry.Value.GetComponent<SlotScripts>();
                SlotSaveData slotData = new SlotSaveData
                {
                    gridPosition = slotEntry.Key,
                    isPlanted = slotScript.isPlanted,
                    ishavebed = slotScript.ishavebed, // <--- ��������� ���
                    isRaked = slotScript.isRaked     // <--- ��� �����
                };

                // --- �������� ���� ��� ---
                if (slotData.ishavebed || slotData.isRaked)
                {
                    Debug.Log($"���������� �����: {slotEntry.Value.name} -> ishavebed={slotData.ishavebed}, isRaked={slotData.isRaked}");
                }
                // --- ����� ���� ---

                gridData.slotsData.Add(slotData);
            }
            SessionData.gridsData.Add(gridData);

            // 3. �������� ������ � ���������
            PlantController[] plants = grid.GetComponentsInChildren<PlantController>();
            // --- ���������� ��� 4 ---
            Debug.Log($"� ����� '{grid.gameObject.name}' ������� {plants.Length} �������� �������� ��� ����������.");
            foreach (var plant in plants)
            {
                SessionData.plantsData.Add(plant.GetSaveData());
            }
        }

        // --- ���������� ��� 5 ---
        Debug.Log($"--- ���� ���������� ---");
        Debug.Log($"����� ����� � SessionData: {SessionData.gridsData.Count}");
        // ������� ����� �����, ������� �� ���������
        foreach (var gridData in SessionData.gridsData)
        {
            Debug.Log($"�������� ������������� �����: '{gridData.identifier}'");
        }
        Debug.Log($"����� �������� � SessionData: {SessionData.plantsData.Count}");

        // 4. ������������� ����
        ShouldLoadSessionData = true;

        Debug.Log("--- ����� ����������. ShouldLoadSessionData = true ---");
    }



}
