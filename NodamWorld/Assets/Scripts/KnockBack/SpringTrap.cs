using UnityEngine;

public class SpringTrap : MonoBehaviour
{
    [Header("Spring Settings")]
    public bool isActivated = false;

    // ★ Spring과 충돌 시 호출 (추후 사운드/애니메이션 추가)
    public void OnSpringHit()
    {
        if (isActivated) return;

        isActivated = true;

        // TODO: 사운드 재생
        // TODO: 애니메이션 재생
        
        Debug.Log($"[SpringTrap] Spring hit: {gameObject.name}");

        // 일정 시간 후 다시 활성화 가능 (옵션)
        Invoke(nameof(ResetSpring), 0.5f);
    }

    private void ResetSpring()
    {
        isActivated = false;
    }
}