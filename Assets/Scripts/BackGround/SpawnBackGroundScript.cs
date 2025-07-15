using UnityEngine;

public class SpawnBackGroundScript : MonoBehaviour
{
    public GameObject Object; // ������ ����
    [SerializeField] float timeInterval; // �������� �������
    [SerializeField] float startTimeInterval; // ������ �������� �������
   // [SerializeField] Sprite NewFonts;
     [SerializeField] Sprite OldFonts; // ��� ��� ��� �������� (������������ ��� ����� ����)




    [System.Serializable]
    public struct PositionSpawn
    {
        public float x;
        public float y;
        public float z;
    }
    public PositionSpawn spawnPoint; // ������� ������ �������




    void Start()
    {
        Object.GetComponent<SpriteRenderer>().sprite = OldFonts;

        InvokeRepeating("CreateObjects", startTimeInterval, timeInterval);

    }

    void CreateObjects()
    {
        // 1. ������� ������ �� �������
        GameObject newFon = Instantiate(Object, new Vector3(spawnPoint.x, spawnPoint.y, spawnPoint.z), Quaternion.identity);

        // 2. �������� ��������� � ��������� ����� � ������ ���
        SpriteRenderer sr = newFon.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = OldFonts;
        }
    }
    //void ChangeTextureBackground()
    //{
    //    SpriteRenderer spriteRenderer = Object.GetComponent<SpriteRenderer>();
    //    spriteRenderer.sprite = NewFonts;
    //}
}
