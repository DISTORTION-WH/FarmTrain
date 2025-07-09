using UnityEngine;

public class BedController : MonoBehaviour
{

    [Header("Growth")]
    // ������ �� data �������� 
    [SerializeField] BedData bedData;

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
        
    }

    public void ChangeStage(BedData.StageGrowthPlant stage, int idx)
    {
        
        Stagebed = stage;
        _spriteRenderer.sprite = bedData.bedSprites[idx];
    }
}
