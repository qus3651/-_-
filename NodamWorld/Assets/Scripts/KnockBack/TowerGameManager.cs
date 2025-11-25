using System.Collections.Generic;
using UnityEngine;

public class TowerGameManager : MonoBehaviour
{
    [Header("Prefabs & Refs")]
    public TowerSquareController squarePrefab;
    public Transform platformParent;
    public GameObject[] platformSetPrefabs;
    public TowerCameraFollow camFollow;
    public BackgroundManager backgroundManager;

    [Header("Tower & Floors")]
    public float floorStepY = -1f;
    public int floorsPerSet = 9;
    public int spawnAheadSetCount = 7;
    public float startTopY = 2f;
    public Vector3 startPosition = new Vector3(-2f, 2f, 0f);
    public float fallSpeed = 3.0f;

    [Header("Stage Materials")]
    public Material stageQuadImage1;
    public Material stageQuadImage2;
    public Material stageQuadImage3;
    public Material stageQuadImage4;
    public Material stageQuadImage5;
    public Material stageQuadImage6;

    [Header("Coin Settings")]
    public int collectedCoins = 0;

    [Header("Runtime State")]
    public int currentFloor = 1;
    public int currentStage = 1;
    public float currentHorizontalSpeed = 1.0f;
    public float distance;
    public float playTime;

    private TowerSquareController square;
    private float lowestSpawnedY;
    private Queue<GameObject> spawnedSets = new Queue<GameObject>();
    private bool isGameStarted = false;
    private bool isGamePlaying = false;
    private int spawnedFloorCount = 0;

    public float GetStageSpeed(int floor)
    {
        if (floor <= 10) return 1.0f;
        if (floor <= 20) return 1.5f;
        if (floor <= 40) return 2.0f;
        if (floor <= 70) return 2.3f;
        if (floor <= 120) return 2.3f;
        if (floor <= 300) return 2.8f;
        return 3.1f;
    }

    public int GetStageByFloor(int floor)
    {
        if (floor <= 10) return 1;
        if (floor <= 20) return 2;
        if (floor <= 40) return 3;
        if (floor <= 70) return 4;
        if (floor <= 120) return 5;
        if (floor <= 300) return 6;
        return 7;
    }

    public float GetStageQuadRollSpeed(int stage)
    {
        float baseSpeed = 360f;
        return baseSpeed * Mathf.Pow(1.3f, stage - 1);
    }

    public Material GetStageMaterial(int stage)
    {
        switch (stage)
        {
            case 1: return stageQuadImage1;
            case 2: return stageQuadImage2;
            case 3: return stageQuadImage3;
            case 4: return stageQuadImage4;
            case 5: return stageQuadImage5;
            case 6: return stageQuadImage6;
            default: return stageQuadImage6;
        }
    }

    void Start()
    {
        // 로그인 완료 대기 - TowerUIManager에서 호출
    }

    public void PrepareGame()
    {
        if (isGameStarted) return;
        isGameStarted = true;
        isGamePlaying = false;

        lowestSpawnedY = startTopY;
        spawnedFloorCount = 0;
        collectedCoins = 0;
        currentFloor = 1;

        for (int i = 0; i < spawnAheadSetCount; i++)
        {
            SpawnNextSet();
        }

        Vector3 spawnPos = startPosition;
        square = Instantiate(squarePrefab, spawnPos, Quaternion.identity);
        square.Initialize(this, false);

        currentStage = GetStageByFloor(1);
        currentHorizontalSpeed = GetStageSpeed(1);

        UpdateSquareMaterial();
        
        if (backgroundManager != null)
        {
            backgroundManager.UpdateBackground(currentStage);
        }

        TowerUIManager.Instance?.UpdateHUD(currentFloor, currentStage, distance, playTime, collectedCoins, square.NextDirection);
    }

    public void StartGame()
    {
        if (!isGameStarted || isGamePlaying) return;
        
        isGamePlaying = true;

        if (square != null)
        {
            square.StartFalling();
        }
    }

    void Update()
    {
        if (!isGameStarted || !isGamePlaying || square == null || square.IsGameOver) return;

        playTime += Time.deltaTime;

        if (square.State == TowerSquareController.SquareState.RollingLeft ||
            square.State == TowerSquareController.SquareState.RollingRight)
        {
            distance += currentHorizontalSpeed * Time.deltaTime;
        }

        int floorIndex = Mathf.FloorToInt((startTopY - square.transform.position.y) / Mathf.Abs(floorStepY)) + 1;
        floorIndex = Mathf.Max(1, floorIndex);

        if (floorIndex > currentFloor)
        {
            for (int i = currentFloor + 1; i <= floorIndex; i++)
            {
                currentFloor = i;
                int newStage = GetStageByFloor(currentFloor);
                
                if (newStage != currentStage)
                {
                    currentStage = newStage;
                    UpdateSquareMaterial();
                    
                    if (backgroundManager != null)
                    {
                        backgroundManager.UpdateBackground(currentStage);
                    }
                }
                else
                {
                    currentStage = newStage;
                }

                currentHorizontalSpeed = GetStageSpeed(currentFloor);

                TowerUIManager.Instance?.UpdateHUD(currentFloor, currentStage, distance, playTime, collectedCoins, square.NextDirection);
                if (camFollow != null) camFollow.MoveDownToFloor(currentFloor, floorStepY);
            }
        }

        while (lowestSpawnedY > square.transform.position.y - (floorsPerSet * Mathf.Abs(floorStepY) * 2))
        {
            SpawnNextSet();
        }

        TowerUIManager.Instance?.UpdateHUD(currentFloor, currentStage, distance, playTime, collectedCoins, square.NextDirection);
    }

    private void SpawnNextSet()
    {
        if (platformSetPrefabs.Length == 0) return;

        var setPrefab = platformSetPrefabs[Random.Range(0, platformSetPrefabs.Length)];
        Vector3 spawnPosition = new Vector3(0f, lowestSpawnedY, 0f);
        var setInstance = Instantiate(setPrefab, spawnPosition, Quaternion.identity, platformParent);
        
        spawnedSets.Enqueue(setInstance);
        spawnedFloorCount += floorsPerSet;

        float setHeight = CalculateSetHeight(setInstance);
        lowestSpawnedY -= setHeight;

        if (spawnedSets.Count > spawnAheadSetCount + 3)
        {
            var oldSet = spawnedSets.Dequeue();
            if (oldSet) Destroy(oldSet);
        }
    }

    private float CalculateSetHeight(GameObject setInstance)
    {
        return Mathf.Abs(floorStepY) * floorsPerSet;
    }

    private void UpdateSquareMaterial()
    {
        if (square != null && square.visualQuad != null)
        {
            Material newMaterial = GetStageMaterial(currentStage);
            if (newMaterial != null)
            {
                Renderer renderer = square.visualQuad.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = newMaterial;
                }
            }

            square.UpdateQuadRollSpeed(GetStageQuadRollSpeed(currentStage));
        }
    }

    public void CollectCoin()
    {
        collectedCoins++;
        TowerUIManager.Instance?.UpdateHUD(currentFloor, currentStage, distance, playTime, collectedCoins, square.NextDirection);
    }

    public float GetFallSpeed() => fallSpeed;

    public void OnGameOver()
    {
        // 코인 서버 저장
        if (TossGameLoginManager.Instance != null && TossGameLoginManager.Instance.isLoggedIn)
        {
            TossGameLoginManager.Instance.SaveCoins(collectedCoins);
        }

        TowerUIManager.Instance?.ShowGameOver(currentFloor, distance, playTime, collectedCoins);
    }

    public void ResetGame()
    {
        if (square != null) Destroy(square.gameObject);
        
        while (spawnedSets.Count > 0)
        {
            var set = spawnedSets.Dequeue();
            if (set) Destroy(set);
        }

        currentFloor = 1;
        currentStage = 1;
        currentHorizontalSpeed = 1.0f;
        distance = 0f;
        playTime = 0f;
        collectedCoins = 0;
        isGameStarted = false;
        isGamePlaying = false;
        lowestSpawnedY = startTopY;
        spawnedFloorCount = 0;

        if (backgroundManager != null)
        {
            backgroundManager.ResetBackground();
        }

        if (camFollow != null)
        {
            camFollow.ResetCamera();
        }
    }
}