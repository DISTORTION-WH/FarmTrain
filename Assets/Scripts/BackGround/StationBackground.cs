// StationBackgroundManager.cs

using UnityEngine;
using System.Collections.Generic;

// ����������, ��� �� ������� ���� SpriteRenderer
[RequireComponent(typeof(SpriteRenderer))]
public class StationBackgroundManager : MonoBehaviour
{
    [Tooltip("������ �������� ��� ���� ������ �������. Element 0 - ��� ������� 1, Element 1 - ��� ������� 2 � �.�.")]
    [SerializeField] private List<Sprite> stationSprites;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // �������� ������ �� ��������� SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("�� ������� ���� ������� ����������� ��������� SpriteRenderer!", gameObject);
            enabled = false; // ��������� ������, ����� �������� ������
        }
    }

    void Start()
    {
        // ���������, ��� ExperienceManager ��������
        if (ExperienceManager.Instance == null)
        {
            Debug.LogError("ExperienceManager �� ������! ��� ������� �� ����� ���������� ���� �������.", gameObject);
            return;
        }

        // �������� ������� ������� ���� (������� ������������� ID �������)
        int currentLevel = ExperienceManager.Instance.CurrentLevel;

        // ������������� ��������������� ���
        SetBackgroundForLevel(currentLevel);
    }

    /// <summary>
    /// ������������� ������ ���� � ������������ � ��������� �������.
    /// </summary>
    /// <param name="level">������� ������� (1, 2, 3...)</param>
    public void SetBackgroundForLevel(int level)
    {
        // ������� 1 ������������� ������� 0 � ������, ������� 2 -> ������� 1, � �.�.
        int spriteIndex = level - 1;

        if (stationSprites == null || stationSprites.Count == 0)
        {
            Debug.LogWarning($"� ���� ������� ({gameObject.name}) �� �������� ������ �������� � ����������.", gameObject);
            return;
        }

        // ���������, ��� ��� ������� ������ ���� ������ � ������
        if (spriteIndex >= 0 && spriteIndex < stationSprites.Count)
        {
            // ������������� ������ ������
            spriteRenderer.sprite = stationSprites[spriteIndex];
            Debug.Log($"��� ������� ������� �� ������ '{stationSprites[spriteIndex].name}' ��� ������ {level}.");
        }
        else
        {
            Debug.LogError($"��� ���� ������� ({gameObject.name}) �� ������ ������ ��� ������ {level} (��������� ������: {spriteIndex}). ��������� ������ stationSprites.", gameObject);
        }
    }
}