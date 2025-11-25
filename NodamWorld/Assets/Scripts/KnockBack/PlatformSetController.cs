using UnityEngine;

public class PlatformSetController : MonoBehaviour
{
    [Header("Trap Settings")]
    public int trapActivationFloorOffset = 3; // 공이 트랩보다 몇 층 위에 있을 때 트랩 활성화

    private TrapController[] traps;
    private bool[] trapActivated;
    private Transform[] platformFloors;
    private bool isInitialized = false;

    public void Initialize()
    {
        if (isInitialized) return;

        traps = GetComponentsInChildren<TrapController>(true);
        trapActivated = new bool[traps.Length];

        platformFloors = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            platformFloors[i] = transform.GetChild(i);
        }

        isInitialized = true;
    }

    public void CheckAndActivateTraps(float ballYPosition, float floorStepY)
    {
        if (!isInitialized) Initialize();

        for (int i = 0; i < traps.Length; i++)
        {
            if (trapActivated[i]) continue;

            // 트랩의 Y 위치
            float trapY = traps[i].transform.position.y;

            // ★ 수정: 공이 트랩보다 위에 있는지 확인
            // ballY가 더 크면(위에 있으면) 양수, trapY가 더 크면(아래에 있으면) 음수
            float floorDifference = (ballYPosition - trapY) / Mathf.Abs(floorStepY);

            // ★ 공이 트랩보다 N개 층 위에 있으면 활성화
            if (floorDifference >= trapActivationFloorOffset)
            {
                traps[i].ActivateTrap();
                trapActivated[i] = true;
                Debug.Log($"Trap activated: {traps[i].gameObject.name}, Ball Y: {ballYPosition}, Trap Y: {trapY}, Floor Diff: {floorDifference}");
            }
        }
    }

    public void ResetAllTraps()
    {
        if (traps == null) return;

        for (int i = 0; i < traps.Length; i++)
        {
            if (traps[i] != null)
            {
                traps[i].ResetTrap();
            }
            trapActivated[i] = false;
        }
    }
}