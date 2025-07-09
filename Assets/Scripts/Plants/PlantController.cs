using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;
public class PlantController : MonoBehaviour
{


    [Header("Growth")]
    // Ссылка на data растения
    [SerializeField] PlantData plantData;
    [SerializeField] GameObject icon_water;
    [SerializeField] GameObject worldItemPrefab;

    private InventoryManager inventoryManager;

    //
    SpriteRenderer _spriteRenderer;
    PlantData.StageGrowthPlant Stageplant;

    Vector2Int[] IdSlots;


    float timePerGrowthStage = 0.0f;
    float timeWaterNeed = 0.0f;

    bool isNeedWater = false;
    bool isFertilize = false;

    void Start()
    {
        inventoryManager = InventoryManager.Instance; // И поиск синглтона тоже

        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (plantData != null)
        {
            Stageplant = PlantData.StageGrowthPlant.defaultStage;
            timePerGrowthStage = plantData.timePerGrowthStage;
            timeWaterNeed = plantData.waterNeededInterval;
            _spriteRenderer.sprite = plantData.growthStagesSprites[0];
            InvokeRepeating("StartPlantGrowth", 0f, timePerGrowthStage);
            InvokeRepeating("StartWaterNeededInterval", 0f, timeWaterNeed);
            Debug.Log($"Spawning plant {plantData.plantName}");

            CheckForAchievement(plantData.plantName);
        }
        else
        {
            Debug.LogError("Отсутствует ссылка на данные растения");
            Destroy(gameObject);
        }

        float countSlot = plantData.Weight;
    }

    void StartPlantGrowth()
    {
        if (!isNeedWater)
        {
            switch (Stageplant)
            {
                case PlantData.StageGrowthPlant.defaultStage:
                    _spriteRenderer.sprite = plantData.growthStagesSprites[0];
                    Stageplant = PlantData.StageGrowthPlant.SecondStage;
                    break;
                case PlantData.StageGrowthPlant.SecondStage:
                    _spriteRenderer.sprite = plantData.growthStagesSprites[1];
                    Stageplant = PlantData.StageGrowthPlant.ThirdStage;
                    break;
                case PlantData.StageGrowthPlant.ThirdStage:
                    _spriteRenderer.sprite = plantData.growthStagesSprites[2];
                    Stageplant = PlantData.StageGrowthPlant.FourthStage;
                    break;
                case PlantData.StageGrowthPlant.FourthStage:
                    _spriteRenderer.sprite = plantData.growthStagesSprites[3];
                    CancelInvoke("StartPlantGrowth");
                    break;
            }
        }
    }

    void StartWaterNeededInterval()
    {
        if (!isNeedWater && Stageplant != PlantData.StageGrowthPlant.FourthStage)
        {
            isNeedWater = true;
            GameObject iconWater = Instantiate(icon_water, new Vector3(transform.position.x + 0.3f, transform.position.y + 0.4f, transform.position.z), Quaternion.identity);
            if (iconWater != null)
            {
                iconWater.transform.parent = transform;
                iconWater.name = "icon_water";
                Debug.Log($"Plant {name} need Water!");
            }
            else
            {
                Debug.LogWarning("Ошибка спавна иконки нужды воды");
            }
        }
    }

    void CheckForAchievement(string namePlant)
    {
        if (AchievementManager.allTpyesPlant.Contains(namePlant))
        {
            Debug.Log($"Type plant {namePlant} planted");
            if (AchievementManager.allTpyesPlant.Remove(namePlant))
                GameEvents.TriggerOnCollectAllPlants(1);
            else
            {
                Debug.LogWarning("This type of plant is undefind");
            }
        }
    }

    void WateringPlants()
    {
        isNeedWater = false;
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.name == "icon_water")
            {
                Destroy(child.gameObject);
                Debug.Log("Вы успешно полили растение!");
                if (SFXManager.Instance != null && SFXManager.Instance.wateringSound != null)
                {
                    SFXManager.Instance.PlaySFX(SFXManager.Instance.wateringSound);
                }
                break;
            }
           
        }
    }

    public void FillVectorInts(Vector2Int[] Posarray)
    {
        IdSlots = Posarray;
        //foreach (var slot in IdSlots)
        //{
        //    Debug.Log($"<<< Current IdSlot for Plant: {slot}");
        //}
    }

    private void FertilizePlant(Vector2Int[] idSlots)
    {
        float fertilizerGrowthMultiplie = plantData.fertilizerGrowthMultiplier;
        GameObject parent = transform.parent.gameObject;

        if (parent != null)
        {
            GridGenerator generator = parent.GetComponent<GridGenerator>();
            if (generator != null)
            {
                generator.FertilizerSlot(idSlots);
                timePerGrowthStage /= fertilizerGrowthMultiplie;
                isFertilize = true;
            }
            else
            {
                Debug.Log($"Для parent {parent.name} generator is null");
            }
        }
        else
        {
            Debug.Log($"Для растения {name} Parent is null");
        }
    }

    public void ClickHandler()
    {
        InventoryItem selectedItem = inventoryManager.GetSelectedItem();
        int selectedIndex = inventoryManager.SelectedSlotIndex; // Используем новое свойство

        if (selectedItem == null && Stageplant == PlantData.StageGrowthPlant.FourthStage)
        {
            Debug.Log(">>> Сбор урожая");
            GameObject parent = transform.parent.gameObject;

            if (parent != null)
            {
                GridGenerator gridGenerator = parent.GetComponent<GridGenerator>();
                if (gridGenerator != null)
                {
                    if (gridGenerator.FreeSlot(IdSlots))
                    {
                        if (SFXManager.Instance != null && SFXManager.Instance.shovelSound != null)
                        {
                            SFXManager.Instance.PlaySFX(SFXManager.Instance.shovelSound);
                        }

                        if (TryGetSeeds(plantData.seedDropChance))
                        {
                            GameObject seed = GetHarvest(transform.position, plantData.seedItem);
                        }

                        GameObject harvestedCrop = GetHarvest(transform.position, plantData.harvestedCrop);
                        if (harvestedCrop != null)
                        {
                            Debug.Log("Урожай собран!");
                        }
                        else
                        {
                            Debug.Log("Урожай не собран!");
                        }
                        Destroy(gameObject);
                    }
                    else
                    {
                        Debug.Log("Ошибка удаления растения");
                    }
                }
                else
                {
                    Debug.Log($"У {gameObject.name} нет родителя GridGenerator и контроллера gridGenerator");
                }
            }
            else
            {
                Debug.Log($"У {gameObject.name} нет родителя GridGenerator");
            }
        }
        else if (selectedItem == null)
        {
            Debug.Log("Выбери предмет из инвенторя");
        }
        else
        {
            if (!selectedItem.IsEmpty)
            {
                if (selectedItem.itemData.itemType == ItemType.Tool)
                {
                    if (selectedItem.itemData.itemName == "Shovel")
                    {
                        GameObject parent = transform.parent.gameObject;
                        if (parent != null)
                        {
                            GridGenerator gridGenerator = parent.GetComponent<GridGenerator>();
                            if (gridGenerator != null)
                            {
                                if (gridGenerator.FreeSlot(IdSlots))
                                    Destroy(gameObject);
                                else
                                {
                                    Debug.Log("Ошибка удаления растения");
                                }
                            }
                            else
                            {
                                Debug.Log($"У {gameObject.name} нет родителя GridGenerator и контроллера gridGenerator");
                            }
                        }
                        else
                        {
                            Debug.Log($"У {gameObject.name} нет родителя GridGenerator");
                        }
                    }
                    if (selectedItem.itemData.itemName == "watering_can")
                    {
                        if (isNeedWater)
                        {
                            WateringPlants();
                        }
                        else
                        {
                            Debug.Log($"У {gameObject.name} нет нужды в поливке");
                        }
                    }
                }
                else if (selectedItem.itemData.itemType == ItemType.Fertilizer)
                {
                    if (!isFertilize)
                    {
                        FertilizePlant(IdSlots);
                    }
                    else
                    {
                        Debug.Log("Растение уже удобрено!");
                    }
                }
            }
        }
    }

    // получить урожай (дублирование кода, но что поделать)
    public GameObject GetHarvest(Vector3 spawnPosition, ItemData itemTospawn)
    {
        float randomValue = UnityEngine.Random.Range(-0.3f, 0.3f);
        Vector3 spawnScale = Vector3.one;
        ItemData dataToSpawn = itemTospawn;
        if (worldItemPrefab == null)
        {
            Debug.LogError($"World Item Prefab не назначен в ItemSpawner! Невозможно заспавнить предмет '{dataToSpawn.itemName}'.");
            return null;
        }

        GameObject newItemObject = Instantiate(worldItemPrefab, new Vector3(spawnPosition.x + randomValue, spawnPosition.y + randomValue, spawnPosition.z), Quaternion.identity);

        newItemObject.transform.localScale = spawnScale;
        newItemObject.transform.parent = transform.parent;

        WorldItem worldItemComponent = newItemObject.GetComponent<WorldItem>();
        if (worldItemComponent != null)
        {
            worldItemComponent.itemData = dataToSpawn;
            worldItemComponent.InitializeVisuals();
            Debug.Log($"Заспавнен ПРЕДМЕТ: {dataToSpawn.itemName} в позиции {spawnPosition}");
            return newItemObject;
        }
        else
        {
            Debug.LogError($"На префабе '{worldItemPrefab.name}' отсутствует компонент WorldItem! Предмет '{dataToSpawn.itemName}' уничтожен.");
            Destroy(newItemObject);
            return null;
        }
    }

    private bool TryGetSeeds(double successRate)
    {
        if (successRate < 0 || successRate > 1)
        {
            throw new ArgumentException("Success rate must be between 0 and 1");
        }
        System.Random random = new System.Random();
        return random.NextDouble() < successRate;
    }
}