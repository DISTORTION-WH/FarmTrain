using UnityEngine;
using System.Collections;

// ���� enum �������� ��� ���������
public enum AnimalProductionState
{
    WaitingForFeed,
    ReadyForProduct,
    ReadyForFertilizer
}

public class AnimalController : MonoBehaviour
{
    [Header("Data & Links")]
    public AnimalData animalData;
    public GameObject thoughtBubblePrefab;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private float minIdleTime = 2.0f;
    [SerializeField] private float maxIdleTime = 5.0f;
    [SerializeField] private float minWalkTime = 3.0f;
    [SerializeField] private float maxWalkTime = 6.0f;
    [SerializeField] private Vector3 thoughtBubbleOffset = new Vector3(1.4f, 0.9f, 0);

    private enum AnimalState { Idle, Walking, NeedsAttention }
    private AnimalState currentState = AnimalState.Idle;
    private AudioSource audioSource;
    private Coroutine soundCoroutine;

    private Bounds movementBounds;
    private bool boundsInitialized = false;

    private float cycleTimer;
    private AnimalProductionState productionState;
    private float stateChangeTimer;

    private bool needsFeeding = false;
    private bool hasProductReady = false;
    private bool hasFertilizerReady = false;
    private ItemData currentNeedIcon = null;

    private Transform myTransform;
    private ThoughtBubbleController activeThoughtBubble;
    private InventoryManager inventoryManager;
    private SpriteRenderer spriteRenderer;

    private Vector2 currentTargetPosition;
    private bool isMoving = false;

    private AnimalStateData currentStateData;

    void Awake()
    {
        myTransform = transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
        {
            Debug.LogError($"InventoryManager �� ������! Awake() � AnimalController");
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Start()
    {
        if (animalData == null)
        {
            Debug.LogError($"AnimalData �� ��������� ��� {gameObject.name}! �������� �� ����� ��������.", gameObject);
            enabled = false;
            return;
        }
        if (thoughtBubblePrefab == null)
        {
            Debug.LogError($"ThoughtBubblePrefab �� �������� ��� {gameObject.name}! �� ������ �������� �����������.", gameObject);
        }
        if (inventoryManager == null)
        {
            Debug.LogError($"InventoryManager �� ������ �� �����! ���� ��������� �� ����� ��������.", gameObject);
        }

        currentState = AnimalState.Idle;
        SetNewStateTimer(AnimalState.Idle);
        UpdateAppearance();
        CheckForAchievement(animalData.speciesName);

        if (currentStateData == null)
        {
            this.productionState = AnimalProductionState.WaitingForFeed;
            this.cycleTimer = animalData.feedInterval;
        }

        if (soundCoroutine == null)
        {
            soundCoroutine = StartCoroutine(RandomSoundCoroutine());
        }

    }

    public void InitializeWithState(AnimalStateData stateData, Bounds bounds)
    {
        currentStateData = stateData;
        animalData = stateData.animalData;

        this.productionState = stateData.productionState;
        this.cycleTimer = stateData.cycleTimer;

        CheckNeeds();

        InitializeMovementBounds(bounds, false);
    }

    public void InitializeMovementBounds(Bounds bounds, bool setInitialPosition)
    {
        if (myTransform == null)
        {
            Debug.LogError($"������: myTransform ��� ��� null � InitializeMovementBounds! ��������� Awake() � {gameObject.name}", gameObject);
            myTransform = transform;
        }

        movementBounds = bounds;
        boundsInitialized = true;
        Debug.Log($"{animalData.speciesName} ({gameObject.name}) ������� ������� ��������: {movementBounds}");

        if (setInitialPosition)
        {
            myTransform.position = GetRandomPositionInBounds();
            Debug.Log($"������������ ��������� ��������� ������� ��� {animalData.speciesName}: {myTransform.position}");
        }
        PickNewWanderTarget();

        if (boundsInitialized)
        {
            StartCoroutine(StateMachineCoroutine());
            Debug.Log($"StateMachineCoroutine ������� ��� {animalData.speciesName} ({gameObject.name})");
        }
        else
        {
            Debug.LogError($"�� ������� ��������� StateMachineCoroutine ��� {animalData.speciesName}, �.�. ������� �� ����������������!", gameObject);
        }
    }

    public void SaveState()
    {
        Debug.Log("SaveState!!!");
        if (currentStateData != null)
        {
            currentStateData.productionState = this.productionState;
            currentStateData.cycleTimer = this.cycleTimer;

            currentStateData.lastPosition = transform.position;
            currentStateData.hasBeenPlaced = true;

            Debug.Log($"<color=orange>[AnimalController]</color> �������� ��������� ��� {animalData.speciesName}. " +
          $"�������: {currentStateData.lastPosition}. " +
          $"���� hasBeenPlaced ������: <color=yellow>{currentStateData.hasBeenPlaced}</color>");
        }
        else
        {
            Debug.Log($"<color=red>[AnimalController]</color> ������� ��������� ��������� ��� {gameObject.name}, �� currentStateData is null! ���������� �� �������.");
        }
    }

    void Update()
    {
        if (!boundsInitialized || animalData == null) return;

        if (currentState != AnimalState.NeedsAttention)
        {
            UpdateTimers(Time.deltaTime);
            CheckNeeds();
        }

        if (activeThoughtBubble != null && activeThoughtBubble.gameObject.activeSelf)
        {
            activeThoughtBubble.transform.position = myTransform.position + thoughtBubbleOffset;
        }

        if (isMoving && spriteRenderer != null)
        {
            float horizontalDifference = currentTargetPosition.x - myTransform.position.x;

            if (Mathf.Abs(horizontalDifference) > 0.01f)
            {
                spriteRenderer.flipX = (horizontalDifference > 0);
            }
        }
    }

    private IEnumerator StateMachineCoroutine()
    {
        Debug.Log($"StateMachineCoroutine ����� ������ ��� {gameObject.name}. ��������� ���������: {currentState}");

        while (boundsInitialized)
        {
            if (currentState == AnimalState.NeedsAttention)
            {
                isMoving = false;
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(stateChangeTimer);

            if (currentState == AnimalState.NeedsAttention)
            {
                Debug.Log($"{gameObject.name}: ��������� ���������� �� NeedsAttention �� ����� ��������. ��������� �����.");
                isMoving = false;
                yield return null;
                continue;
            }

            Debug.Log($"{gameObject.name}: ����� �������� ��� {currentState} �������. ������ ���������.");
            if (currentState == AnimalState.Idle)
            {
                currentState = AnimalState.Walking;
                PickNewWanderTarget();
                SetNewStateTimer(AnimalState.Walking);
                isMoving = true;
                Debug.Log($"{gameObject.name}: ������� � ��������� Walking. ����� ����: {currentTargetPosition}, �����: {stateChangeTimer} ���.");
            }
            else
            {
                currentState = AnimalState.Idle;
                SetNewStateTimer(AnimalState.Idle);
                isMoving = false;
                Debug.Log($"{gameObject.name}: ������� � ��������� Idle. �����: {stateChangeTimer} ���.");
            }
        }
        Debug.LogWarning($"StateMachineCoroutine �������� ������ ��� {gameObject.name} (boundsInitialized ���� false?)");
    }

    void FixedUpdate()
    {
        if (currentState == AnimalState.Walking && isMoving && boundsInitialized)
        {
            MoveTowardsTarget();
        }
    }

    // --- ���������: ������ ����-�������� ������ �������� ����� ����� �������� ---
    private void UpdateTimers(float deltaTime)
    {
        if (productionState == AnimalProductionState.WaitingForFeed && AnimalPenManager.Instance.HasAutoFeeder(this.animalData))
        {
            Debug.Log($"[Auto-Feeder] �������� {animalData.speciesName} ���������� �������������. ������� � ���������� �����.");
            TransitionToNextProductionState(); // ���������� ����� ����� ��� ��������
            return;
        }

        if (cycleTimer > 0)
        {
            cycleTimer -= deltaTime;
        }
    }

    private void CheckNeeds()
    {
        if (cycleTimer > 0)
        {
            if (currentState == AnimalState.NeedsAttention && !needsFeeding && !hasProductReady && !hasFertilizerReady)
            {
                Debug.LogWarning($"[CheckNeeds] In NeedsAttention state, but no active need flags. Reverting to Idle.");
                HideThoughtBubble();
                currentState = AnimalState.Idle;
                SetNewStateTimer(currentState);
            }
            return;
        }

        bool needsAttentionNow = false;
        ItemData nextNeedIcon = null;

        switch (productionState)
        {
            case AnimalProductionState.WaitingForFeed:
                if (!needsFeeding)
                {
                    Debug.Log($"[CheckNeeds] State: WaitingForFeed. Timer is up. Animal needs food.");
                    needsFeeding = true;
                    nextNeedIcon = animalData.requiredFood;
                    needsAttentionNow = true;
                }
                break;

            case AnimalProductionState.ReadyForProduct:
                if (!hasProductReady)
                {
                    Debug.Log($"[CheckNeeds] State: ReadyForProduct. Timer is up. Product is ready.");
                    hasProductReady = true;
                    UpdateAppearance();
                    nextNeedIcon = animalData.productProduced;
                    needsAttentionNow = true;
                }
                break;

            case AnimalProductionState.ReadyForFertilizer:
                if (!hasFertilizerReady)
                {
                    Debug.Log($"[CheckNeeds] State: ReadyForFertilizer. Timer is up. Fertilizer is ready.");
                    hasFertilizerReady = true;
                    nextNeedIcon = animalData.fertilizerProduced;
                    needsAttentionNow = true;
                }
                break;
        }

        if (needsAttentionNow)
        {
            if (currentState != AnimalState.NeedsAttention || currentNeedIcon != nextNeedIcon)
            {
                currentState = AnimalState.NeedsAttention;
                currentNeedIcon = nextNeedIcon;
                isMoving = false;
                ShowThoughtBubble(currentNeedIcon);
            }
        }
    }

    private void SetNewStateTimer(AnimalState forState)
    {
        if (forState == AnimalState.Idle)
        {
            stateChangeTimer = Random.Range(minIdleTime, maxIdleTime);
        }
        else
        {
            stateChangeTimer = Random.Range(minWalkTime, maxWalkTime);
        }
    }

    private void UpdateAppearance()
    {
        if (spriteRenderer == null || animalData == null) return;

        if (hasProductReady && animalData.productReadySprite != null)
        {
            spriteRenderer.sprite = animalData.productReadySprite;
        }
        else if (animalData.defaultSprite != null)
        {
            spriteRenderer.sprite = animalData.defaultSprite;
        }
        else
        {
            if (hasProductReady && animalData.productReadySprite == null && animalData.defaultSprite == null)
                Debug.LogWarning($"� {animalData.speciesName} ����� �������, �� �� ��������� 'productReadySprite' � 'defaultSprite' � AnimalData!", gameObject);
            else if (!hasProductReady && animalData.defaultSprite == null)
                Debug.LogWarning($"� {animalData.speciesName} ��� ��������, �� �� �������� 'defaultSprite' � AnimalData!", gameObject);
        }
    }

    private void PickNewWanderTarget()
    {
        currentTargetPosition = GetRandomPositionInBounds();
    }

    private Vector2 GetRandomPositionInBounds()
    {
        if (!boundsInitialized) return myTransform.position;

        float randomX = Random.Range(movementBounds.min.x, movementBounds.max.x);
        float randomY = Random.Range(movementBounds.min.y, movementBounds.max.y);

        return new Vector2(randomX, randomY);
    }

    private void MoveTowardsTarget()
    {
        if (!isMoving) return;

        Vector2 currentPosition = myTransform.position;
        Vector2 direction = (currentTargetPosition - currentPosition).normalized;
        Vector2 newPosition = Vector2.MoveTowards(currentPosition, currentTargetPosition, moveSpeed * Time.fixedDeltaTime);

        newPosition.x = Mathf.Clamp(newPosition.x, movementBounds.min.x, movementBounds.max.x);
        newPosition.y = Mathf.Clamp(newPosition.y, movementBounds.min.y, movementBounds.max.y);

        myTransform.position = newPosition;

        if (Vector2.Distance(currentPosition, currentTargetPosition) < 0.1f)
        {
            isMoving = false;
        }
    }

    private void ShowThoughtBubble(ItemData itemToShow)
    {
        Debug.Log($"[ShowThoughtBubble] ������. �������� ��������: {itemToShow?.itemName ?? "NULL"}");

        if (thoughtBubblePrefab == null)
        {
            Debug.LogError("��� ������� �������!");
            return;
        }

        if (activeThoughtBubble == null)
        {
            GameObject bubbleInstance = Instantiate(thoughtBubblePrefab, myTransform.position + thoughtBubbleOffset, Quaternion.identity, myTransform);
            activeThoughtBubble = bubbleInstance.GetComponent<ThoughtBubbleController>();
            if (activeThoughtBubble == null)
            {
                Debug.LogError("������ ������� �� �������� ������ ThoughtBubbleController!");
                Destroy(bubbleInstance);
                return;
            }
            BubbleYSorter bubbleSorter = bubbleInstance.GetComponent<BubbleYSorter>();
            if (bubbleSorter != null)
            {
                bubbleSorter.SetOwner(myTransform);
            }
            else
            {
                Debug.LogError($"�� ������� ������� {thoughtBubblePrefab.name} ����������� ������ BubbleYSorter!", bubbleInstance);
            }
        }

        if (itemToShow != null && itemToShow.itemIcon != null)
        {
            Debug.Log($"[ShowThoughtBubble] ������ ��� {itemToShow.itemName} �������. �������� activeThoughtBubble.Show().");
            activeThoughtBubble.Show(itemToShow.itemIcon);
        }
        else
        {
            Debug.LogWarning($"������� �������� ������� ��� {animalData.speciesName}, �� � �������� {itemToShow?.itemName} ��� ������!");
            activeThoughtBubble.Hide();
        }
    }

    private void HideThoughtBubble()
    {
        if (activeThoughtBubble != null)
        {
            activeThoughtBubble.Hide();
        }
        currentNeedIcon = null;
    }

    public string GetCurrentStateName()
    {
        return currentState.ToString();
    }

    // --- ����� �����: "�����" ������� � ���������� ��������� ---
    private void TransitionToNextProductionState()
    {
        // ����������, ����� ��������� ������ ���� ��������� � ������
        AnimalProductionState nextState;
        switch (productionState)
        {
            case AnimalProductionState.WaitingForFeed:
                nextState = AnimalProductionState.ReadyForProduct;
                break;
            case AnimalProductionState.ReadyForProduct:
                nextState = AnimalProductionState.ReadyForFertilizer;
                break;
            case AnimalProductionState.ReadyForFertilizer:
            default:
                nextState = AnimalProductionState.WaitingForFeed; // �������� ���� ������
                break;
        }

        // ���������, �� ����� �� ���������� ��������� ����
        // �� ������ ��� � �����, ����� ����� ���� ���������� ��������� ������ ������
        int safetyCounter = 0; // �� ������ ���� ��� ���������� = 0, ����� �������� ������������ �����
        while (safetyCounter < 4)
        {
            if (nextState == AnimalProductionState.ReadyForProduct && animalData.productAmount <= 0)
            {
                Debug.Log($"���������� ���� �������� ��� {animalData.speciesName} (���������� = 0).");
                nextState = AnimalProductionState.ReadyForFertilizer; // ������� ������� � ���������
            }
            else if (nextState == AnimalProductionState.ReadyForFertilizer && animalData.fertilizerAmount <= 0)
            {
                Debug.Log($"���������� ���� ��������� ��� {animalData.speciesName} (���������� = 0).");
                nextState = AnimalProductionState.WaitingForFeed; // ������� ��������� � ������ �����
            }
            else
            {
                break; // ������ ���������� ����, ������� �� �����
            }
            safetyCounter++;
        }

        // ������������� ����� ��������� � ��������������� ������
        productionState = nextState;
        switch (productionState)
        {
            case AnimalProductionState.WaitingForFeed:
                cycleTimer = animalData.feedInterval;
                break;
            case AnimalProductionState.ReadyForProduct:
                cycleTimer = animalData.productionInterval;
                break;
            case AnimalProductionState.ReadyForFertilizer:
                cycleTimer = animalData.fertilizerInterval;
                break;
        }
        Debug.Log($"������� � ��������� {productionState}. ������ ���������� �� {cycleTimer} ������.");
    }

    // --- ���������: AttemptInteraction ������ ���������� ����� ����� �������� ---
    public void AttemptInteraction()
    {
        if (inventoryManager == null)
        {
            Debug.LogError("��� ������ �� InventoryManager!");
            return;
        }

        bool interactionSuccessful = false;
        ItemData itemInvolved = null;

        if (needsFeeding)
        {
            itemInvolved = animalData.requiredFood;
            InventoryItem selectedItem = inventoryManager.GetSelectedItem();
            int selectedIndex = inventoryManager.SelectedSlotIndex;

            if (selectedItem != null && !selectedItem.IsEmpty && selectedItem.itemData == animalData.requiredFood)
            {
                inventoryManager.RemoveItem(selectedIndex, 1);

                needsFeeding = false;
                TransitionToNextProductionState(); // ���������� ����� �����

                interactionSuccessful = true;
                if (QuestManager.Instance != null)
                {
                    QuestManager.Instance.AddQuestProgress(GoalType.FeedAnimal, animalData.speciesName, 1);
                }
            }
            else
            {
                string reason = (selectedItem == null || selectedItem.IsEmpty) ? "� ��������� ����� (������) ��� ��������" : $"������ �������� ������� ({selectedItem.itemData.itemName}), ����� {animalData.requiredFood.itemName}";
                Debug.Log($"�� ������� ��������� {animalData.speciesName}: {reason}.");
                interactionSuccessful = false;
            }
        }
        else if (hasProductReady)
        {
            itemInvolved = animalData.productProduced;
            bool toolCheckPassed = true;
            if (animalData.requiredToolForHarvest != null)
            {
                InventoryItem selectedItem = inventoryManager.GetSelectedItem();
                if (selectedItem == null || selectedItem.IsEmpty || selectedItem.itemData != animalData.requiredToolForHarvest)
                {
                    toolCheckPassed = false;
                }
            }

            if (toolCheckPassed)
            {
                if (inventoryManager.AddItem(animalData.productProduced, animalData.productAmount))
                {
                    CheckForAchievement();

                    hasProductReady = false;
                    UpdateAppearance();
                    TransitionToNextProductionState(); // ���������� ����� �����

                    interactionSuccessful = true;
                }
                else
                {
                    Debug.Log("�� ������� ������� ������� - ��������� �����!");
                }
            }
        }
        else if (hasFertilizerReady)
        {
            itemInvolved = animalData.fertilizerProduced;
            if (inventoryManager.AddItem(animalData.fertilizerProduced, animalData.fertilizerAmount))
            {
                hasFertilizerReady = false;
                TransitionToNextProductionState(); // ���������� ����� �����

                interactionSuccessful = true;
            }
            else
            {
                Debug.Log("�� ������� ������� ��������� - ��������� �����!");
            }
        }

        if (interactionSuccessful)
        {
            Debug.Log($"�������������� � {animalData.speciesName} (�������: {itemInvolved?.itemName}) ���� ��������.");
            HideThoughtBubble();
            CheckNeeds();

            StopAllCoroutines();
            StartCoroutine(StateMachineCoroutine());
            StartCoroutine(RandomSoundCoroutine());

            Debug.Log($"{animalData.speciesName} ��������� �������� �������������� � ������������ StateMachine.");
        }
        else
        {
            Debug.Log($"�������������� � {animalData.speciesName} (�������: {itemInvolved?.itemName ?? "NULL"}) ���� ����������. ������� ���������: {currentState}. �����: Feed={needsFeeding}, Prod={hasProductReady}, Fert={hasFertilizerReady}");
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"<color=red>[AnimalController]</color> {gameObject.name} ������������ (OnDestroy). ������� SaveState().");
        SaveState();
        if (soundCoroutine != null)
        {
            StopCoroutine(soundCoroutine);
        }
    }

    void CheckForAchievement(string name)
    {
        if (AchievementManager.allTpyesAnimal.Contains(name))
        {
            if (AchievementManager.allTpyesAnimal.Remove(name))
            {
                GameEvents.TriggerAddedNewAnimal(1);
            }
            else
            {
                Debug.LogWarning($"This type of animal {name} is can not remove!");

            }
        }

    }
    void CheckForAchievement()
    {
        GameEvents.TriggerCollectAnimalProduct(1);
    }
    private IEnumerator RandomSoundCoroutine()
    {
        while (true)
        {
            float delay = Random.Range(5f, 100f);
            yield return new WaitForSeconds(delay);
            PlayRandomSound();
        }
    }
    private void PlayRandomSound()
    {
        if (animalData.animalSounds != null && animalData.animalSounds.Length > 0 && audioSource != null)
        {
            AudioClip clip = animalData.animalSounds[Random.Range(0, animalData.animalSounds.Length)];
            audioSource.PlayOneShot(clip);
            Debug.Log($"���� ��������� ({animalData.speciesName}): {clip.name}");
        }
    }
}
