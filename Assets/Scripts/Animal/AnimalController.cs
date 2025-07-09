using UnityEngine;
using System.Collections;

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

    private float feedTimer;
    private float productionTimer;
    private float fertilizerTimer;
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
        if (soundCoroutine == null)
        {
            soundCoroutine = StartCoroutine(RandomSoundCoroutine());
        }

    }

    public void InitializeWithState(AnimalStateData stateData, Bounds bounds)
    {
        currentStateData = stateData;
        animalData = stateData.animalData; // ����� AnimalData �� ���������

        // ��������� ������� �� ����������� ������
        feedTimer = stateData.feedTimer;
        productionTimer = stateData.productionTimer;
        fertilizerTimer = stateData.fertilizerTimer;

        // ��������� ����� ����� �������������
        InitializeMovementBounds(bounds, false);
        // ... � ��� �����. ��������� ��� Start() - ��������, ���-�� ������ ���� ��������� ����.
        // �����, ����� InitializeMovementBounds ��������� ����� ����, ��� animalData ��� �����������.
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
            currentStateData.feedTimer = this.feedTimer;
            currentStateData.productionTimer = this.productionTimer;
            currentStateData.fertilizerTimer = this.fertilizerTimer;

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

    private void UpdateTimers(float deltaTime)
    {
        // ���������, ���� �� � ��� ������������
        bool hasAutoFeeder = AnimalPenManager.Instance.HasAutoFeeder(this.animalData);

        // ��������� ������ ������, ������ ���� ������������ ���
        if (!hasAutoFeeder && feedTimer > 0)
        {
            feedTimer -= deltaTime;
        }

        // ��������� ������� �������� ��� ������
        if (productionTimer > 0) productionTimer -= deltaTime;
        if (fertilizerTimer > 0) fertilizerTimer -= deltaTime;
    }

    private void CheckNeeds()
    {
        bool needsAttentionNow = false;
        ItemData nextNeedIcon = null;
        bool didProductBecomeReady = false;

        // <<< �������� ��������� >>>
        // ���������, ���� �� � ��� ������������
        bool hasAutoFeeder = AnimalPenManager.Instance.HasAutoFeeder(this.animalData);

        // ��������� ����������� � ���, ������ ���� ������������ ���
        if (!hasAutoFeeder && !needsFeeding && feedTimer <= 0)
        {
            Debug.Log($"[CheckNeeds] ���������� �����������: ��� ({animalData.requiredFood.itemName})");
            needsFeeding = true;
            nextNeedIcon = animalData.requiredFood;
            needsAttentionNow = true;
        }
        else if (!hasProductReady && productionTimer <= 0)
        {
            Debug.Log($"[CheckNeeds] ���������� �����������: ������� ({animalData.productProduced.itemName})");
            hasProductReady = true;
            didProductBecomeReady = true;
            nextNeedIcon = animalData.productProduced;
            needsAttentionNow = true;
        }
        else if (!hasFertilizerReady && fertilizerTimer <= 0)
        {
            Debug.Log($"[CheckNeeds] ���������� �����������: ��������� ({animalData.fertilizerProduced.itemName})");
            hasFertilizerReady = true;
            nextNeedIcon = animalData.fertilizerProduced;
            needsAttentionNow = true;
        }

        if (didProductBecomeReady)
        {
            UpdateAppearance();
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
        else if (currentState == AnimalState.NeedsAttention)
        {
            Debug.LogWarning($"[CheckNeeds] ������������ �� �������, �� ��������� ���� NeedsAttention. ���������� � Idle.");
            HideThoughtBubble();
            currentState = AnimalState.Idle;
            SetNewStateTimer(currentState);
        }
    }

    private void ResetFeedTimer()
    {
        feedTimer = animalData.feedInterval;
        needsFeeding = false;
    }

    private void ResetProductionTimer()
    {
        productionTimer = animalData.productionInterval;
        hasProductReady = false;
        UpdateAppearance();
    }

    private void ResetFertilizerTimer()
    {
        fertilizerTimer = animalData.fertilizerInterval;
        hasFertilizerReady = false;
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
            Debug.Log($"������� ��������������: {animalData.speciesName} ��������� � {animalData.requiredFood.itemName}");
            itemInvolved = animalData.requiredFood;

            if (inventoryManager == null)
            {
                Debug.LogError($"�� ���� ��������� ��������� ��� ��������� {animalData.speciesName} - ������ �� InventoryManager �����������!");
                interactionSuccessful = false;
            }
            else
            {
                InventoryItem selectedItem = inventoryManager.GetSelectedItem();
                int selectedIndex = inventoryManager.SelectedSlotIndex;

                if (selectedItem != null && !selectedItem.IsEmpty && selectedItem.itemData == animalData.requiredFood)
                {
                    Debug.Log($"����� ������ {selectedItem.itemData.itemName} � ����� {selectedIndex}. �������� ������� 1 ��.");
                    inventoryManager.RemoveItem(selectedIndex, 1);
                    ResetFeedTimer();
                    needsFeeding = false;
                    interactionSuccessful = true;
                    Debug.Log($"������� ��������� {animalData.speciesName}");
                    if (QuestManager.Instance != null)
                    {
                        // �� �������� ��� ����, ID (��� ���� ���������) � ���������� (1)
                        QuestManager.Instance.AddQuestProgress(GoalType.FeedAnimal, animalData.speciesName, 1);
                    }
                }
                else
                {
                    string reason = "����������� �������";
                    if (selectedItem == null || selectedItem.IsEmpty)
                    {
                        reason = "� ��������� ����� (������) ��� ��������";
                    }
                    else if (selectedItem.itemData != animalData.requiredFood)
                    {
                        reason = $"������ �������� ������� ({selectedItem.itemData.itemName}), ����� {animalData.requiredFood.itemName}";
                    }
                    Debug.Log($"�� ������� ��������� {animalData.speciesName}: {reason}.");
                    interactionSuccessful = false;
                }
            }
        }
        else if (hasProductReady)
        {
            itemInvolved = animalData.productProduced;
            Debug.Log($"������� ������� {itemInvolved.itemName} c {animalData.speciesName}");

            bool toolCheckPassed = true;
            if (animalData.requiredToolForHarvest != null)
            {
                Debug.Log($"��� ����� {itemInvolved.itemName} ��������� {animalData.requiredToolForHarvest.itemName}");
                InventoryItem selectedItem = inventoryManager.GetSelectedItem();

                if (selectedItem == null || selectedItem.IsEmpty || selectedItem.itemData != animalData.requiredToolForHarvest)
                {
                    Debug.Log($"�� ������� �������: ����� �� ������ {animalData.requiredToolForHarvest.itemName}. �������: {selectedItem?.itemData?.itemName ?? "������"}");
                    toolCheckPassed = false;
                }
                else
                {
                    Debug.Log($"����� ������ ������ ����������: {selectedItem.itemData.itemName}");
                }
            }

            if (toolCheckPassed)
            {
                bool added = inventoryManager.AddItem(animalData.productProduced, animalData.productAmount);

                if (added)
                {
                    Debug.Log($"������� ������� {animalData.productAmount} {animalData.productProduced.itemName}");
                    CheckForAchievement();
                    hasProductReady = false;
                    ResetProductionTimer();
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
            Debug.Log($"������� ������� {animalData.fertilizerProduced.itemName} c {animalData.speciesName}");
            bool added = inventoryManager.AddItem(animalData.fertilizerProduced, animalData.fertilizerAmount);

            if (added)
            {
                Debug.Log($"������� ������� {animalData.fertilizerAmount} {animalData.fertilizerProduced.itemName}");
                
                ResetFertilizerTimer();
                hasFertilizerReady = false;
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

            Debug.Log("��������� ����� ����� ����� ��������� ��������������...");
            CheckNeeds();

            Debug.Log($"��������� ����� CheckNeeds: {currentState}. ������������� StateMachineCoroutine.");

            StopAllCoroutines();
            StartCoroutine(StateMachineCoroutine());

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
           
            if (AchievementManager.allTpyesPlant.Remove(name))
                GameEvents.TriggerAddedNewAnimal(1);
            else
            {
                Debug.LogWarning("This type of animal is undefind");
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
            float delay = Random.Range(5f, 100f); // �������� ����� �������
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