using UnityEngine;

public class TriggerObstacle : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public bool hasBeenHit = false;

    [Header("Fade Settings")]
    public float fadeDuration = 1.0f; // ★ 사라지는 데 걸리는 시간 (초)
    public bool disableColliderWhenFading = true; // ★ 페이드 시작 시 충돌체 비활성화 여부

    private Renderer objectRenderer;
    private Material objectMaterial;
    private Color originalColor;
    private bool isFading = false;
    private float fadeTimer = 0f;
    private Collider objectCollider;

    void Awake()
    {
        // ★ Renderer 가져오기
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            // ★ Material 복사 (원본 Material을 수정하지 않도록)
            objectMaterial = objectRenderer.material;
            originalColor = objectMaterial.color;

            // ★ Material을 Transparent 모드로 설정 (Standard Shader 기준)
            SetMaterialTransparent(objectMaterial);
        }

        // ★ Collider 가져오기
        objectCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (isFading)
        {
            fadeTimer += Time.deltaTime;
            float fadeProgress = Mathf.Clamp01(fadeTimer / fadeDuration);

            // ★ 투명도 조절 (1 → 0)
            if (objectMaterial != null)
            {
                Color newColor = originalColor;
                newColor.a = Mathf.Lerp(1f, 0f, fadeProgress);
                objectMaterial.color = newColor;
            }

            // ★ 완전히 사라지면 비활성화
            if (fadeProgress >= 1f)
            {
                isFading = false;
                gameObject.SetActive(false);
            }
        }
    }

    // ★ 페이드 아웃 시작
    public void StartFading()
    {
        if (isFading) return;

        isFading = true;
        fadeTimer = 0f;

        // ★ 페이드 시작 시 충돌체 비활성화 (옵션)
        if (disableColliderWhenFading && objectCollider != null)
        {
            objectCollider.enabled = false;
        }

        Debug.Log($"[TriggerObstacle] Started fading: {gameObject.name}");
    }

    // ★ Material을 Transparent 모드로 설정
    private void SetMaterialTransparent(Material mat)
    {
        // Standard Shader의 Rendering Mode를 Transparent로 변경
        mat.SetFloat("_Mode", 3); // 0: Opaque, 1: Cutout, 2: Fade, 3: Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    // ★ TriggerObstacle과 충돌 시 호출 (게임오버)
    public void OnObstacleHit()
    {
        if (hasBeenHit) return;

        hasBeenHit = true;

        // TODO: 충돌 사운드 재생
        // TODO: 충돌 애니메이션 재생

        Debug.Log($"[TriggerObstacle] Obstacle hit: {gameObject.name} - Game Over!");
    }
}