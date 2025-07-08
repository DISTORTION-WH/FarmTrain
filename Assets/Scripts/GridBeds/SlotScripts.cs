using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlotScripts : MonoBehaviour
{
    public bool isPlanted = false; // ���� �� ��������
    public bool ishavebed = false;  // ���� �� ������
    public bool isRaked = false; // ���������� �� ������
    public bool isFertilize = false; // ���� �� ���������
    Color currentColor;
    SpriteRenderer spriteRenderer;
    Transform slot;



    private InventoryManager inventoryManager; // ������ �� �������� ���������

   

    [SerializeField] ItemSpawner _itemSpawner;
    [SerializeField] Vector3 _sizeBed;
    [SerializeField] Vector3 _sizePlant;

    void Start()
    {

        inventoryManager = InventoryManager.Instance; // � ����� ��������� ����
       
        if(_itemSpawner == null)
        {
            Debug.Log("itemSpaner not found!");
        }

       
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentColor = spriteRenderer.color;
       
      
    }

    public void PlantSeeds()
    {

       
        // �������� ��������� ������� � ������ ���������� �����
        InventoryItem selectedItem = inventoryManager.GetSelectedItem();
        int selectedIndex = inventoryManager.SelectedSlotIndex; // ���������� ����� ��������

       
        if(selectedItem == null)
        {
            Debug.Log("������ ������� �� ���������");
        }

        else
        {
            // ���������, ���� �� ��������� ������� � �������� �� �� ������� ��� �������
            if (!selectedItem.IsEmpty && selectedItem.itemData.itemType == ItemType.Pot)
            {

               

                if (ishavebed)
                {

                    Debug.Log("��� ��� ������, ����??");
                }
                else
                {
                    _itemSpawner.TestSpawnBed(selectedItem.itemData, transform.position, _sizeBed, gameObject.transform);
                    SFXManager.Instance.PlaySFX(SFXManager.Instance.placeBed);
                    ishavebed = true;
                    InventoryManager.Instance.RemoveItem(selectedIndex);

                }

            }

            if (!selectedItem.IsEmpty && selectedItem.itemData.itemType == ItemType.Seed && !isPlanted)
            {
                if (ishavebed) {
                    if (isRaked) {
                        if (!isPlanted) {
                            Transform parentSlot = transform.parent;

                            BedSlotController bedSlotController = parentSlot.GetComponent<BedSlotController>();
                            GridGenerator gridGenerator = parentSlot.GetComponent<GridGenerator>();



                            if (parentSlot != null)
                            {
                                float weightSeed = selectedItem.itemData.associatedPlantData.Weight;

                                // �������� ���� �������� 

                                switch (weightSeed)
                                {
                                    case 1:

                                        var tourple1 = gridGenerator.CheckFreeSlot(name);
                                        bool isFreeSlot1 = tourple1.Item1;
                                        Vector3 pos1 = tourple1.Item2;
                                        Vector2Int[] idSlots1 = tourple1.Item3;
                                        if (isFreeSlot1)
                                        {
                                            _itemSpawner.SpawnPlant(selectedItem.itemData, pos1, _sizePlant, gameObject.transform.parent, idSlots1,isFertilize);
                                            AudioClip plantSound = selectedItem.itemData.associatedPlantData.plantingSound;
                                            SFXManager.Instance.PlaySFX(plantSound);

                                            isPlanted = true;
                                            InventoryManager.Instance.RemoveItem(selectedIndex);
                                        }
                                        
                                        break;

                                    case 2:

                                        if (gridGenerator != null)
                                        {
                                            var tourple = gridGenerator.CheckFree2Slot(name);
                                            bool isFreeSlot = tourple.Item1;
                                            Vector3 pos = tourple.Item2;
                                            Vector2Int[] idSlots = tourple.Item3;
                                            if (isFreeSlot)
                                            {
                                                _itemSpawner.SpawnPlant(selectedItem.itemData, pos, _sizePlant, gameObject.transform.parent,idSlots, isFertilize);
                                                AudioClip plantSound = selectedItem.itemData.associatedPlantData.plantingSound;
                                                SFXManager.Instance.PlaySFX(plantSound);

                                                InventoryManager.Instance.RemoveItem(selectedIndex);
                                            }
                                            else
                                            {
                                                Debug.Log("�� ������� ������, ���� ������ ���");

                                            }

                                        }
                                        else
                                        {
                                            Debug.Log("������ ����������, ����������� ������ gridGenerator � ������������� Slot");
                                        }
                                        break;
                                    case 4:

                                        if (gridGenerator != null)
                                        {
                                            var tourple = gridGenerator.CheckSquareCells(name);
                                            bool isFreeSlot = tourple.Item1;
                                            Vector3 Plantposition = tourple.Item2;
                                            Vector2Int[] idSlots = tourple.Item3;
                                            if (isFreeSlot)
                                            {
                                                _itemSpawner.SpawnPlant(selectedItem.itemData, Plantposition, _sizePlant, gameObject.transform.parent, idSlots, isFertilize);
                                                AudioClip plantSound = selectedItem.itemData.associatedPlantData.plantingSound;
                                                SFXManager.Instance.PlaySFX(plantSound);

                                                InventoryManager.Instance.RemoveItem(selectedIndex);
                                            }
                                            else
                                            {
                                                Debug.Log("�� ������� ������, ���� ������ ���");

                                            }

                                        }
                                        else
                                        {
                                            Debug.Log("������ ����������, ����������� ������ BedSlotController � ������������� Slot");
                                        }
                                        break;

                                }

                            }
                            else
                            {
                                Debug.LogError("�� ������ ������������ ����! ������");
                            }
                        }
                        else
                        {
                            Debug.Log("��� ��� ������, ����??");
                        }
                    }
                    else
                    {
                        Debug.Log("������� ���� ���������� ������, � ����� ��� ������ ��������");

                    }

                }
                else
                {
                    Debug.Log("������� ���� ��������� ������!");
                }

                
               
            }
         

            if(!selectedItem.IsEmpty && selectedItem.itemData.itemType == ItemType.Tool)
            {
                if (ishavebed)
                {
                    if (selectedItem.itemData.itemName == "Rake")
                    {
                        GameObject childBed = FindChildWithTag("Bed");
                        if (childBed != null)
                        {
                            BedController bedController = childBed.GetComponent<BedController>();
                            if (bedController != null) {

                                bedController.ChangeStage(BedData.StageGrowthPlant.Raked, 1);
                                isRaked = true;
                                SFXManager.Instance.PlaySFX(SFXManager.Instance.rake);

                            }
                            else
                            {
                                Debug.LogError("bedController �� ������");
                            }
                        }
                        else
                        {
                            Debug.LogError("������ �� �������� �������� ��� �����, ������");
                        }
                    }
                   

                }
                else
                {
                    Debug.Log("������������ ����� ������ ���������� ������!");
                }
            }
            if (!selectedItem.IsEmpty && selectedItem.itemData.itemType == ItemType.Fertilizer)
            {
                if (ishavebed)
                {

                    if (isRaked)
                    {
                        if (!isFertilize)
                        {
                            GameObject childBed = FindChildWithTag("Bed");
                            if (childBed != null)
                            {
                                BedController bedController = childBed.GetComponent<BedController>();
                                if (bedController != null)
                                {
                                    isFertilize = true;
                                    SFXManager.Instance.PlaySFX(SFXManager.Instance.fertilize);

                                    bedController.ChangeStage(BedData.StageGrowthPlant.WithFertilizers, 3);
                                    InventoryManager.Instance.RemoveItem(selectedIndex);
                                }
                                else
                                {
                                    Debug.LogError("bedController �� ������");
                                }
                            }
                            else
                            {
                                Debug.LogError("������ �� �������� �������� ��� �����, ������");
                            }
                        }
                        else
                        {
                            Debug.Log("�� ������ ��� ������� ���������!");
                        }

                    }
                    else
                    {
                        Debug.Log("������������ ����� ������ ���������� ������!");
                    }


                }
                else
                {
                    Debug.Log("������������ ����� ������ ���������� ������!");
                }
            }

        }
        
    }

    private GameObject FindChildWithTag(string tag)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag(tag))
            {
                return child.gameObject;
            }
        }
        Debug.LogWarning($"No child with tag {tag} found.");
        return null;
    }



    public void ChangeColor()
    {

        slot.GetComponent<SpriteRenderer>().color = new Color(0, 255f, 0, 0.1f);

    }
    public void UnChangeColor()
    {
        slot.GetComponent<SpriteRenderer>().color = currentColor;
    }
}
