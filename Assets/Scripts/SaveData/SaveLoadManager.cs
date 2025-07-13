// SaveLoadManager.cs
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance;
    private string _saveFilePath;


    private GameSaveData _gameData;

    // ���� ����� ��������� ������ ��� ������ ����

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
            Debug.Log($"Path to save data {_saveFilePath}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
       
        LoadGame();
    }


    public void LoadGame()
    {
        if (!File.Exists(_saveFilePath))
        {
            Debug.Log("���� ���������� �� ������. �������� �� ����� ���������. ���������� ����� ����.");
            return;
        }

        Debug.Log("�������� ���� �� �����...");

        // 1. ������ JSON �� ����� � �������������
        string json = File.ReadAllText(_saveFilePath);
        _gameData = JsonUtility.FromJson<GameSaveData>(json);

        if (_gameData == null)
        {
            Debug.LogError("�� ������� ��������� ������ �� �����. ��������, ���� ���������.");
            return;
        }

        // --- ��������� ������� ���������� ������ ---
        ApplyLoadedData();
    }


    public void SaveGame()
    {
        Debug.Log("[SaveLoadManager]  ���������� ����...");
        _gameData = new GameSaveData();

        // --- �������� ������ � ������ (GridGenerators) ---
        var gridGenerators = FindObjectsOfType<GridGenerator>(); // ������������, ��� ������ ������ ��� ����������
        foreach (var grid in gridGenerators)
        {
            _gameData.gridsData.Add(grid.GetSaveData());
        }

        // --- �������� ������ � �������� (PlantController) ---
        var plants = FindObjectsOfType<PlantController>(); // ������������, ��� ������ �� �������� ��� ����������
        foreach (var plant in plants)
        {
            _gameData.plantsData.Add(plant.GetSaveData());
        }

        // --- ��������� ������ ������, ���� ����� ---
         _gameData.upgradeWatering = PlantManager.instance.UpgradeWatering;




        // --- ����������� � JSON � ��������� � ���� ---
        string json = JsonUtility.ToJson(_gameData, true); // true ��� ��������� ��������������
        File.WriteAllText(_saveFilePath, json);

        Debug.Log($"[SaveLoadManager]  ���� ��������� �: {_saveFilePath}");
    }
    public void ApplyLoadedData()
    {
        
        // --- ������� ������� ����� �� ������ �������� ---
        // ��� �����, ����� �� ���� ���������� ��� ������������ �����
        var oldPlants = FindObjectsOfType<PlantController>();
        foreach (var plant in oldPlants)
        {
            Destroy(plant.gameObject);
        }

        // --- ������� ��� ���������� ������ � ����� ---
        var gridGenerators = FindObjectsOfType<GridGenerator>().ToDictionary(g => g.identifier, g => g);

        // --- ��������� ��������� ������ ---
        foreach (var gridData in _gameData.gridsData)
        {
            if (gridGenerators.ContainsKey(gridData.identifier))
            {
                gridGenerators[gridData.identifier].ApplySaveData(gridData);
            }
        }

        // --- ���������� �������� ---
        foreach (var plantData in _gameData.plantsData)
        {
            if (gridGenerators.ContainsKey(plantData.gridIdentifier))
            {
                gridGenerators[plantData.gridIdentifier].SpawnPlantFromSave(plantData);
            }
        }

        Debug.Log("[SaveLoadManager]  �������� ���������.");
    }

}