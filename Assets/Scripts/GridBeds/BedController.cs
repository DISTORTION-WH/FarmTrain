using UnityEngine;

public class BedController : MonoBehaviour
{

    [Header("Growth")]
    // ������ �� data �������� 
    [SerializeField] BedData bedData;
   public  bool isRaked = false;
    public bool isFertilize = false;    
    // 
    SpriteRenderer _spriteRenderer;
    BedData.StageGrowthPlant Stagebed;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
           
            if (bedData != null)
            {
               _spriteRenderer.sprite = bedData.bedSprites[0];
                Stagebed = BedData.StageGrowthPlant.DrySoil;

            }
            else
            {
                Debug.LogError("����������� ������ �� bedData , ������ ������");
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogError("����������� ������ �� _spriteRenderer , ������ ������");
            Destroy(gameObject);
        }
       if(isRaked)
        {
           
            ChangeStage(BedData.StageGrowthPlant.Raked, 1);
           
        }
        if (isFertilize)
        {
            ChangeStage(BedData.StageGrowthPlant.WithFertilizers, 3);
        }
        if (!isRaked && !isFertilize) { 
        
             ChangeStage(BedData.StageGrowthPlant.DrySoil, 0);
        }
    }

    public void ChangeStage(BedData.StageGrowthPlant stage, int idx)
    {
        // --- ������ ����������� ---
        // ���� _spriteRenderer ��� �� ��� ������� (��������, ������ ��� Awake ��� �� ��������),
        // �� �������� ��� ����� ������.
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
        // --- ����� ����������� ---

        // ������ ����� ���� ���������, ��� _spriteRenderer �� null (���� ��������� ������ ���� �� �������)
        if (bedData != null && bedData.bedSprites.Count > idx)
        {
            // ��� ���� ������ 44
            _spriteRenderer.sprite = bedData.bedSprites[idx];
            Debug.Log($"������ ������ ������� �� ������ {stage}");
        }
        else
        {
            Debug.LogError("�� ������� �������� ������ ������: bedData �� �������� ��� ������ ������� ������� �� ������� �������.");
        }
    }
}
