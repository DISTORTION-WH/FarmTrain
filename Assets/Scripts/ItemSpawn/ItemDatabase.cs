// ItemDatabase.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> allItems;

    private Dictionary<string, ItemData> itemsByName;
    private bool isInitialized = false;

    private void Initialize()
    {
        if (isInitialized) return;

        itemsByName = new Dictionary<string, ItemData>();
        foreach (var item in allItems)
        {
            if (item != null && !string.IsNullOrEmpty(item.itemName))
            {
                // --- ������� ��������� ---
                // "�������" ���� ����� ����������� � �������
                string cleanKey = item.itemName.Trim();

                if (!itemsByName.ContainsKey(cleanKey))
                {
                    itemsByName.Add(cleanKey, item);
                }
                else
                {
                    Debug.LogWarning($"� ItemDatabase ������ �������� ����� ������� �����: '{cleanKey}'");
                }
            }
            else
            {
                Debug.LogWarning("� ItemDatabase ������ ������ ������� ��� ������� � ������ ������.");
            }
        }
        isInitialized = true;
    }

    // ��������� ����� ��� ��������� ItemData �� ��� ����� (itemName)
    public ItemData GetItemByName(string name)
    {
        Initialize();

        // --- ������� ��������� ---
        // "�������" ������� ��� ����� �������
        string cleanName = name.Trim();

        itemsByName.TryGetValue(cleanName, out ItemData item);
        return item;
    }

    public ItemData GetSeedByPlantName(string plantName)
    {
        Initialize();

        // ����� ���� ����� �������� �������, ���� ����� �������� ����� ��������� �������
        string cleanPlantName = plantName.Trim();

        return allItems.FirstOrDefault(item =>
            item.itemType == ItemType.Seed &&
            item.associatedPlantData != null &&
            item.associatedPlantData.plantName.Trim() == cleanPlantName
        );
    }
}