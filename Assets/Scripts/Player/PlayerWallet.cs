using UnityEngine;
using System;
using TMPro; 

public class PlayerWallet : MonoBehaviour
{
    public static PlayerWallet Instance { get; private set; }

    [SerializeField] private int startingMoney = 500;
    [SerializeField] private int maxMoney = 999999999;

    private int currentMoney;

    public event Action<int> OnMoneyChanged;
    public event Action<int> OnMoneyAdded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentMoney = startingMoney;
        }
    }

    public int GetCurrentMoney()
    {
        return currentMoney;
    }

    public bool HasEnoughMoney(int amount)
    {
        return currentMoney >= amount;
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0) return; // ���������� ������� � ������������� ��������

        currentMoney += amount;
        currentMoney = Mathf.Min(currentMoney, maxMoney);

        // <<< �������� ����� �������
        // �������� ����, ������� ������ ���� ���������
        OnMoneyAdded?.Invoke(amount);

        // ������ ������� ��� UI � ������ ������ ��������� ��� ����
        OnMoneyChanged?.Invoke(currentMoney);
        GameEvents.TriggerCollectCoin(amount);
        Debug.Log($"��������� {amount} �����. �����: {currentMoney}");
    }


    public void SpendMoney(int amount)
    {
        if (amount < 0 || !HasEnoughMoney(amount)) return;
        currentMoney -= amount;
        OnMoneyChanged?.Invoke(currentMoney);
        Debug.Log($"��������� {amount} �����. �����: {currentMoney}");
    }

}