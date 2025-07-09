// AutoScrollParallax.cs

using UnityEngine;
using System.Collections.Generic; // <<< ��������� ��� List

public class AutoScrollParallax : MonoBehaviour
{
    [Tooltip("��������, � ������� ����� ��������� ���� ����.")]
    public float scrollSpeed = -2f;

    // --- ����� ���� ---
    [Tooltip("������ �������� ��� ������� ������ ����. Element 0 - ��� ������ 1, Element 1 - ��� ������ 2 � �.�.")]
    [SerializeField] private List<Sprite> levelSprites;

    private SpriteRenderer spriteRenderer;
    // --- ����� ����� ����� ---

    private float spriteWidth;
    private Vector3 startPosition;

    void Awake() // <<< ������ Start �� Awake
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("�� ���� ���������� ����������� SpriteRenderer!", gameObject);
            enabled = false;
            return;
        }

        // --- ����� ������ ������������� ---
        // ������������� ������ ��� �������� ������ ��� ������ ����
        if (ExperienceManager.Instance != null)
        {
            SetSpriteForLevel(ExperienceManager.Instance.CurrentLevel);
        }
        else
        {
            // ���� ExperienceManager ��� �� �����, ���������� ������ ��� ������� ������
            SetSpriteForLevel(1);
        }

        // ��������� ��������� ������� �������
        startPosition = transform.position;
        // ��������� ������ ������� � ������� �����������
        RecalculateBounds();
    }

    void Update()
    {
        if (spriteRenderer.sprite == null) return; // �� ���������, ���� ��� �������

        // ������� ������ �����
        transform.Translate(Vector3.right * scrollSpeed * Time.deltaTime);

        // ���������, ����� �� "���������������" ���
        // Mathf.Repeat ������ ���������� ������ �������� �������
        float newPositionX = startPosition.x + Mathf.Repeat(Time.time * scrollSpeed, spriteWidth);
        transform.position = new Vector3(newPositionX, startPosition.y, startPosition.z);
    }

    // --- ����� ������ ---

    // ���� ����� ����� ���������� ����� ��� ����� ������
    public void SetSpriteForLevel(int level)
    {
        int spriteIndex = level - 1; // ������� 1 -> ������ 0, ������� 2 -> ������ 1

        if (levelSprites == null || levelSprites.Count == 0)
        {
            Debug.LogWarning($"� ���� ���������� {gameObject.name} �� �������� ������ ��������.", gameObject);
            return;
        }

        if (spriteIndex >= 0 && spriteIndex < levelSprites.Count)
        {
            if (spriteRenderer.sprite != levelSprites[spriteIndex])
            {
                spriteRenderer.sprite = levelSprites[spriteIndex];
                RecalculateBounds(); // ����� ����������� ������ ����� ����� �������!
                Debug.Log($"���� {gameObject.name} ������ ������ �� {spriteRenderer.sprite.name} ��� ������ {level}.");
            }
        }
        else
        {
            Debug.LogError($"��� ���� {gameObject.name} �� ������ ������ ��� ������ {level} (������ {spriteIndex}).", gameObject);
        }
    }

    // ��������������� ����� ��� ��������� ������
    private void RecalculateBounds()
    {
        if (spriteRenderer.sprite != null)
        {
            spriteWidth = spriteRenderer.bounds.size.x;
        }
    }
}