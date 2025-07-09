using UnityEngine;
using UnityEngine.Events;


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

[CreateAssetMenu(fileName = "Achievement",menuName = "Achievements/Achievement")]
public class AchievementData : ScriptableObject
{
    [Header("�������� ����������")]
    public int IDArchievment;
    public string Name;
    [TextArea] public string Description;
    public Sprite Icon;

    [Header("������� ����������")]
    public TypeOfAchivment typeOfAchivment;
    public int goal; // ������� ���������� � ���-��
    public bool isReceived = false;


}
