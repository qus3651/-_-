using UnityEngine;

public class TriggerButton : MonoBehaviour
{
    [Header("Trigger Settings")]
    public GameObject triggerObstacle; // ★ 사라질 TriggerObstacle (B 오브젝트)
    public bool isActivated = false;

    // ★ 트리거 버튼과 충돌 시 호출
    public void OnTriggerActivated()
    {
        if (isActivated) return;

        isActivated = true;

        // ★ TriggerObstacle이 사라지기 시작
        if (triggerObstacle != null)
        {
            TriggerObstacle obstacle = triggerObstacle.GetComponent<TriggerObstacle>();
            if (obstacle != null)
            {
                obstacle.StartFading(); // ★ 페이드 아웃 시작
                Debug.Log($"[TriggerButton] Started fading obstacle: {triggerObstacle.name}");
            }
            else
            {
                Debug.LogWarning($"[TriggerButton] No TriggerObstacle component on {triggerObstacle.name}!");
            }
        }
        else
        {
            Debug.LogWarning($"[TriggerButton] No obstacle assigned on {gameObject.name}!");
        }

        // TODO: 버튼 눌림 사운드
        // TODO: 버튼 눌림 애니메이션
    }
}