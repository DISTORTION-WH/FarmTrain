using UnityEditor.Rendering;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct ItemSpawnInfo
    {
        public ItemData itemData;
        public Vector3 position;
    }

    [Header("�������� ������")]
    [Tooltip("������, ������� ����� �������������� ��� ������ ��� ���� ����������� ���������.")]
    public GameObject worldItemPrefab;

    [Header("��������� �����")]
    [Tooltip("������ ��������� ��� ������ ��� ������ ����.")]
    public ItemSpawnInfo[] itemsToSpawnAtStart;

    [Header("��������� �� ���������")]
    [Tooltip("������� �� ���������, ���� �� ������ ������ ��� ������ SpawnItem.")]
    public Vector3 defaultSpawnScale = Vector3.one;

    [Header("������ �� �����������")]
    [Tooltip("������ �� ���������� ������/������ ��� ����������� ������������� ������.")]
    public TrainCameraController trainController;

    void Start()
    {
        if (trainController == null)
        {
            Debug.LogError("TrainCameraController �� �������� � ItemSpawner! ���������� ���������� ������������ ������.");
        }

        SpawnInitialItems();
    }

    void SpawnInitialItems()
    {
        if (itemsToSpawnAtStart == null || itemsToSpawnAtStart.Length == 0) { return; }

        foreach (ItemSpawnInfo spawnInfo in itemsToSpawnAtStart)
        {
            if (spawnInfo.itemData != null && spawnInfo.itemData.itemType != ItemType.Animal && worldItemPrefab == null)
            {
                Debug.LogError($"World Item Prefab �� �������� � ItemSpawner, �� �������� ���������� ������� {spawnInfo.itemData.itemName} ��� ������!");
                continue;
            }
            SpawnItem(spawnInfo.itemData, spawnInfo.position, defaultSpawnScale);
        }
    }

    public GameObject SpawnItem(ItemData dataToSpawn, Vector3 spawnPosition, Vector3 spawnScale)
    {
        if (dataToSpawn == null)
        {

            Debug.LogWarning("������� ���������� ������� � null ItemData.");
            return null;
        }

        

        if (dataToSpawn.itemType == ItemType.Animal && dataToSpawn.associatedAnimalData != null)
        {
            AnimalData animalData = dataToSpawn.associatedAnimalData;
            if (animalData.animalPrefab == null)
            {
                Debug.LogError($"� AnimalData '{animalData.speciesName}' �� �������� animalPrefab � ����������!");
                return null;
            }

            GameObject animalObject = Instantiate(animalData.animalPrefab, spawnPosition, Quaternion.identity);

            animalObject.transform.localScale = spawnScale;

            Transform parentWagon = null;
            bool parentAssignedSuccessfully = false;

            if (trainController != null)
            {
                parentAssignedSuccessfully = trainController.AssignParentWagonByPosition(animalObject.transform, spawnPosition);

                if (parentAssignedSuccessfully)
                {
                    parentWagon = animalObject.transform.parent;

                    if (parentWagon == null)
                    {
                        Debug.LogError($"AssignParentWagonByPosition ������ true, �� �������� � {animalObject.name} �� �����������! �������� ����������.", animalObject);
                        Destroy(animalObject);
                        return null;
                    }
                }
                else
                {
                    Debug.LogError($"�� ������� ����� ��� ��������� ������������ ����� ��� ��������� '{animalData.speciesName}' � ������� {spawnPosition}. �������� ����������.");
                    Destroy(animalObject);
                    return null;
                }
            }
            else
            {
                Debug.LogWarning("TrainController �� �������� � ItemSpawner. ���������� ���������� ����� ��� ���������. �������� ����������.");
                Destroy(animalObject);
                return null;
            }

            AnimalController animalController = animalObject.GetComponent<AnimalController>();
            if (animalController != null)
            {
                animalController.animalData = animalData;

                if (parentWagon != null)
                {
                    string placementAreaName = animalData.speciesName.Replace(" ", "") + "PlacementArea";
                    Transform placementAreaTransform = parentWagon.Find(placementAreaName);
                    if (placementAreaTransform != null)
                    {
                        Collider2D boundsCollider = placementAreaTransform.GetComponent<Collider2D>();
                        if (boundsCollider != null)
                        {
                            animalController.InitializeMovementBounds(boundsCollider.bounds, false);
                            Debug.Log($"������� {boundsCollider.bounds} �������� ��������� {animalData.speciesName} ({animalObject.name})");
                        }
                        else
                        {
                            Debug.LogError($"�� ������� 'AnimalPlacementArea' � ������ '{parentWagon.name}' ����������� ��������� Collider2D! �������� �� ������ ��������� ���������.");
                        }
                    }
                    else
                    {
                        Debug.LogError($"�� ������ �������� ������ � ������ 'AnimalPlacementArea' � ������ '{parentWagon.name}'! �������� �� ������ ��������� ���������.");
                    }
                }
                else
                {
                    Debug.LogError($"�� ������� �������� Transform ������������� ������ ({animalObject.name}) ��� ������ AnimalPlacementArea. �������� ����������.");
                    Destroy(animalObject);
                    return null;
                }

                Debug.Log($"��������� ��������: {animalData.speciesName} � ������� {spawnPosition}");
                return animalObject;
            }
            else
            {
                Debug.LogError($"�� ������� ��������� '{animalData.animalPrefab.name}' ({animalObject.name}) ����������� ��������� AnimalController! �������� ����������.");
                Destroy(animalObject);
                return null;
            }
        }
        else
        {
            if (worldItemPrefab == null)
            {
                Debug.LogError($"World Item Prefab �� �������� � ItemSpawner! ���������� ���������� ������� '{dataToSpawn.itemName}'.");
                return null;
            }

            GameObject newItemObject = Instantiate(worldItemPrefab, spawnPosition, Quaternion.identity);

            newItemObject.transform.localScale = spawnScale;

            if (trainController != null)
            {
                if (!trainController.AssignParentWagonByPosition(newItemObject.transform, spawnPosition))
                {
                    Debug.LogError($"�� ������� ����� ��� ��������� ������������ ����� ��� �������� '{dataToSpawn.itemName}' � ������� {spawnPosition}. ������� ���������.");
                    Destroy(newItemObject);
                    return null;
                }
            }
            else
            {
                Debug.LogWarning($"TrainController �� �������� � ItemSpawner. ������� '{dataToSpawn.itemName}' �� ����� �������� � ������.");
            }

            WorldItem worldItemComponent = newItemObject.GetComponent<WorldItem>();
            if (worldItemComponent != null)
            {
                worldItemComponent.itemData = dataToSpawn;
                worldItemComponent.InitializeVisuals();
                Debug.Log($"��������� �������: {dataToSpawn.itemName} � ������� {spawnPosition}");
                return newItemObject;
            }
            else
            {
                Debug.LogError($"�� ������� '{worldItemPrefab.name}' ����������� ��������� WorldItem! ������� '{dataToSpawn.itemName}' ���������.");
                Destroy(newItemObject);
                return null;
            }
        }
    }

    public GameObject SpawnItem(ItemData dataToSpawn, Vector3 spawnPosition)
    {
        return SpawnItem(dataToSpawn, spawnPosition, defaultSpawnScale);
    }


    // �������� ������� ��� ������ ������� ������ �� ������
    public GameObject TestSpawnBed(ItemData dataToSpawn, Vector3 spawnPosition, Vector3 spawnScale, Transform parentTransform)
    {
        if (dataToSpawn.itemType == ItemType.Pot)
        {


            BedData bedData = dataToSpawn.associatedBedData;

            if (bedData.bedlPrefab == null)
            {
                Debug.LogError($"� BedData '{bedData.speciesName}' �� �������� bedlPrefab � ����������!");
                return null;
            }

            GameObject bedObject = Instantiate(bedData.bedlPrefab, spawnPosition, Quaternion.identity);

            bedObject.transform.localScale = spawnScale;
            if (parentTransform != null) {
                bedObject.transform.parent = parentTransform;
                Debug.Log($"��������� ��������: {bedObject.name} � ������� {spawnPosition}");
                       return bedObject;
            }
            else
            {
                Debug.LogError($"����������� ������ �� ������� ��������, ������ �� ���������");
                            Destroy(bedObject);
                           return null;

            }
          

        }
        Debug.Log($"������� ������ �� ������, ������");
        return null;

    }


   
    public GameObject SpawnPlant(ItemData dataToSpawn, Vector3 spawnPosition, Vector3 spawnScale, Transform parentTransform, Vector2Int[] IdSelectedSlot)
    {
        if (dataToSpawn.itemType == ItemType.Seed)
        {


            PlantData plantData = dataToSpawn.associatedPlantData;

            if (plantData.PlantPrefab == null)
            {
                Debug.LogError($"� plantData '{plantData.name}' �� �������� PlantPrefab � ����������!");
                return null;
            }

            GameObject plantObject = Instantiate(plantData.PlantPrefab, spawnPosition, Quaternion.identity);

            plantObject.transform.localScale = spawnScale;
            plantObject.transform.parent = parentTransform;
            PlantController plantController = plantObject.GetComponent<PlantController>();

            if (plantController != null)
            {
               
                plantController.FillVectorInts(IdSelectedSlot);
            }
            else
            {
                Debug.LogError($"� ������� {plantObject.name} ��� plantController, �������� �� ����������!");
                return null;

            }

            Debug.Log($"��������� ��������: {plantObject.name} � ������� {spawnPosition}");
            return plantObject;




        }
        Debug.Log($"������� ������ �� ��������, ������");
        return null;
    }
    public void SpawnAndInitializePlant(ItemData seedData, Vector3 position, Vector3 scale, Transform parent, PlantSaveData saveData)
    {
        // 1. ���������� ��� ������������ ����� ��� �������� ������� ��������
        // �� ��� ����� ��������� ������ � ���������� ��� ID ������
        GameObject plantObject = SpawnPlant(seedData, position, scale, parent, saveData.occupiedSlots);

        // 2. ���������, ��� �������� ������� ���������
        if (plantObject != null)
        {
            // 3. �������� ��� ����������
            PlantController plantController = plantObject.GetComponent<PlantController>();
            if (plantController != null)
            {
                // 4. �������� ����� ������������� �� ����������.
                // ���� ����� �������� ������ ������ �����, ������� � �.�.
                plantController.InitializeFromSave(saveData);

                Debug.Log($"�������� {saveData.plantDataName} ������� ������������� �� ����������.");
            }
            else
            {
                // ��� ������ �� ������ ���������, ���� ��� SpawnPlant �������� ���������, �� �������� �� ��������
                Debug.LogError($"�� ������������ ������� �������� '{plantObject.name}' ����������� ��������� PlantController!");
            }
        }
        else
        {
            Debug.LogWarning($"����� SpawnPlant �� ���� ������� ������ ��� {seedData.itemName}. �������������� ��������.");
        }
    }

}