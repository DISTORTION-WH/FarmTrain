// SaveLoadManager.cs
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance;
    private string saveFilePath;

    // ���� ����� ��������� ������ ��� ������ ����
    public static GameSaveData LoadedData { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveGame()
    {
        Debug.Log("���������� ����...");
        GameSaveData saveData = new GameSaveData();

        // 1. ��������� ������ �� PlantManager
        saveData.upgradeWatering = PlantManager.instance.UpgradeWatering;

        // 2. ������� ��� GridGenerator'� �� �����
        GridGenerator[] gridGenerators = FindObjectsOfType<GridGenerator>();
        foreach (var grid in gridGenerators)
        {
            // ��������� ������ ����� � �� ������
            GridSaveData gridData = new GridSaveData { identifier = grid.gameObject.name };
            foreach (var slotEntry in grid.gridObjects)
            {
                SlotScripts slotScript = slotEntry.Value.GetComponent<SlotScripts>();
                SlotSaveData slotData = new SlotSaveData
                {
                    gridPosition = slotEntry.Key,
                    isPlanted = slotScript.isPlanted,
                    ishavebed = slotScript.ishavebed,
                    isRaked = slotScript.isRaked
                };
                gridData.slotsData.Add(slotData);
            }
            saveData.gridsData.Add(gridData);

            // 3. ��������� ������ � ���������, ������� �������� ��������� ��� ����� �����
            PlantController[] plants = grid.GetComponentsInChildren<PlantController>();
            foreach (var plant in plants)
            {
                // ��� ����� ������ ��� ����� ����� ���������� PlantController 
                saveData.plantsData.Add(plant.GetSaveData());
            }
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("���� ��������� �: " + saveFilePath);
    }

    public bool LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            Debug.Log("�������� ����������...");
            string json = File.ReadAllText(saveFilePath);
            LoadedData = JsonUtility.FromJson<GameSaveData>(json);
            Debug.Log("���������� ������� ��������� � ������.");
            return true;
        }

        Debug.LogWarning("���� ���������� �� ������.");
        LoadedData = null;
        return false;
    }
}