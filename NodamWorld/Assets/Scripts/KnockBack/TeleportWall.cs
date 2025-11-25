using UnityEngine;

public class TeleportWall : MonoBehaviour
{
    public enum TeleportSide
    {
        Left,   // 왼쪽 텔레포트
        Right   // 오른쪽 텔레포트
    }

    [Header("Teleport Settings")]
    public TeleportSide side = TeleportSide.Left;
    public TeleportWall pairedTeleport; // ★ 연결된 반대편 텔레포트
    public float teleportOffset = 0.2f; // ★ 텔레포트 후 약간 안쪽으로 이동

    private bool isTeleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTeleporting) return;

        if (other.CompareTag("Player"))
        {
            TowerSquareController square = other.GetComponent<TowerSquareController>();
            if (square != null && pairedTeleport != null)
            {
                PerformTeleport(square);
            }
        }
    }

    private void PerformTeleport(TowerSquareController square)
    {
        // ★ 텔레포트 시작
        isTeleporting = true;
        pairedTeleport.isTeleporting = true;

        // ★ 현재 위치 저장
        Vector3 currentPos = square.transform.position;

        // ★ 반대편 텔레포트의 위치를 목적지로 사용
        float targetX = pairedTeleport.transform.position.x;
        
        // ★ 약간 안쪽으로 오프셋 적용
        if (side == TeleportSide.Left)
        {
            targetX -= teleportOffset;
        }
        else
        {
            targetX += teleportOffset;
        }

        Vector3 newPos = new Vector3(targetX, currentPos.y, currentPos.z);
        square.transform.position = newPos;

        // ★ 텔레포트 후 현재 플랫폼 해제 및 낙하 상태로 전환
        square.ClearCurrentPlatform();

        Debug.Log($"[TeleportWall] Teleported from {side} (X: {transform.position.x}) to {pairedTeleport.side} (X: {targetX})");

        // TODO: 텔레포트 사운드
        // TODO: 텔레포트 이펙트

        // ★ 잠시 후 텔레포트 플래그 해제
        Invoke(nameof(ResetTeleportFlag), 0.1f);
    }

    private void ResetTeleportFlag()
    {
        isTeleporting = false;
        if (pairedTeleport != null)
        {
            pairedTeleport.isTeleporting = false;
        }
    }
}