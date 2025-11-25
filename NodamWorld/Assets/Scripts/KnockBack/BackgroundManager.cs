using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [Header("Background Settings")]
    public Renderer backgroundRenderer; // ★ 배경 Quad의 Renderer

    [Header("Stage Materials")]
    public Material skyMaterial;        // 1~2 스테이지
    public Material groundMaterial;     // 3~4 스테이지
    public Material undergroundMaterial; // 5~6 스테이지

    private int currentStage = 0;

    public void UpdateBackground(int stage)
    {
        if (currentStage == stage || backgroundRenderer == null) return;

        currentStage = stage;

        Material newMaterial = GetStageMaterial(stage);
        if (newMaterial != null)
        {
            backgroundRenderer.material = newMaterial;
            Debug.Log($"[BackgroundManager] Changed background to stage {stage}");
        }
    }

    private Material GetStageMaterial(int stage)
    {
        if (stage >= 1 && stage <= 2)
        {
            return skyMaterial;
        }
        else if (stage >= 3 && stage <= 4)
        {
            return groundMaterial;
        }
        else if (stage >= 5)
        {
            return undergroundMaterial;
        }

        return skyMaterial; // 기본값
    }

    public void ResetBackground()
    {
        currentStage = 0;
        if (backgroundRenderer != null && skyMaterial != null)
        {
            backgroundRenderer.material = skyMaterial;
        }
    }
}