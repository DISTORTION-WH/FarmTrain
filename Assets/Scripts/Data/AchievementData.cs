using UnityEngine;

// Enum �������� ��� ��
public enum TypeOfAchivment
{
    BountifulHarvest,
    Rancher,
    BuddingTycoon,
    TheWholeGangsHere,
    MasterGardener,
    StateoftheArtFarm,
    FarmingLegend
}

[CreateAssetMenu(fileName = "New Achievement", menuName = "Achievements/Achievement")]
public class AchievementData : ScriptableObject
{
    [Header("�������� ����������")]
    public int IDArchievment;
    public string Name; 
    [TextArea] public string Description;
    public Sprite Icon;
    public int reward;


    [Header("������� ����������")]
    public TypeOfAchivment typeOfAchivment;
    public int goal; // ������� ��������

    
}