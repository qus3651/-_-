using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("이동 속도 (양수=오른쪽, 음수=왼쪽)")]
    public float scrollSpeed = 20f;

    [Header("최대 이동 범위 (왼쪽, 오른쪽)")]
    public float moveRange = 200f;

    private RectTransform rectTransform;
    private Vector2 startPos;
    private float direction = 1f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
    }

    void Update()
    {
        // 좌우 이동
        rectTransform.anchoredPosition += new Vector2(scrollSpeed * direction * Time.deltaTime, 0);

        // 좌우 끝에 도달하면 방향 반전
        if (rectTransform.anchoredPosition.x > startPos.x + moveRange)
            direction = -1f;
        else if (rectTransform.anchoredPosition.x < startPos.x - moveRange)
            direction = 1f;
    }
}
