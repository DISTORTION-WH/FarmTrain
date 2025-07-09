using System;
using UnityEngine;


public static class GameEvents 
{
    // ����������� ���� ������
    public static event Action<int> OnHarvestTheCrop;

    // ����������� ���� ��������� �� �������� 
    public static event Action<int> OnCollectAnimalProduct;

    // ����������� ���� ����� 
    public static event Action<int> OnCollectCoin;

    //  ����������� ���������� ����� �������� 
    public static event Action<int> OnAddedNewAnimal;
    //  ����������� ���������� ����� ��������
    public static event Action<int> OnCollectAllPlants;

    // ����������� ����� ���������
    public static event Action<int> OnAddedNewUpdgrade;
    // ������������ ���������� ���� ������� 

    public static event Action<int> OnCompleteTheQuest;



    // ������ ��� ������ ������� �� ������ �������� 

    public static void TriggerHarvestCrop(int amount)
    {
        OnHarvestTheCrop?.Invoke(amount);
    }

    public static void TriggerCollectAnimalProduct(int amount)
    {
        OnCollectAnimalProduct?.Invoke(amount);
    }

    public static void TriggerCollectCoin(int amount)
    {
        OnCollectCoin?.Invoke(amount);
    }

    public static void TriggerAddedNewAnimal(int amount)
    {
        OnAddedNewAnimal?.Invoke(amount);
    }
    public static void TriggerOnCollectAllPlants(int amount)
    {
        OnCollectAllPlants?.Invoke(amount);
    }
    public static void TriggerAddedNewUpdgrade(int amount)
    {
        OnAddedNewUpdgrade?.Invoke(amount);
    }
    public static void TriggerCompleteTheQuest(int amount)
    {
        OnCompleteTheQuest?.Invoke(amount);
    }
}
