using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Plant", menuName = "Farming/Plant")]
public class PlantData : ScriptableObject
{
    public string plantName = "New Plant";

    [Header("Growth")]
    public List<Sprite> growthStagesSprites; // ������ �������� ��� ������� ����� ����� 
    public float timePerGrowthStage = 10.0f; // ����� � �������� �� ������ ���� �����
    public float waterNeededInterval = 5.0f; // ��� ����� ����� �������� 
    public float fertilizerGrowthMultiplier = 1.5f; // �� ������� ���������� ���� � ���������� 
    public float Weight = 1.0f; // ��� ��������, ��� ������� ������ �������� ��������
    public AudioClip plantingSound; // ���� ��� ����� ��������

    public enum StageGrowthPlant
    {
        defaultStage,
        SecondStage,
        ThirdStage,
        FourthStage,
    }


    [Header("Harvest")]
    public ItemData harvestedCrop; // ����� ������� (����) �������� ��� �����
    public ItemData seedItem; // ����� ������� (�������) ����� ������� ��� �����
    public GameObject PlantPrefab; // ������ ��������
    [Range(0f, 1f)] 
    public float seedDropChance = 0.8f; // ����������� ��������� �������
}