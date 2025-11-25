using UnityEngine;

public class TowerCameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public float offsetY = 1f; // ★ 1로 변경

    private float targetY;
    private float initialY;

    void Start()
    {
        if (target == null)
            target = Object.FindFirstObjectByType<TowerSquareController>().transform;

        targetY = transform.position.y;
        initialY = transform.position.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 pos = transform.position;
        pos.y = Mathf.Lerp(pos.y, targetY, smoothSpeed * Time.deltaTime);
        transform.position = pos;
    }

    public void MoveDownToFloor(int floor, float floorStepY)
    {
        float desiredY = floor * floorStepY + offsetY;
        if (desiredY < targetY)
        {
            targetY = desiredY;
        }
    }

    public void ResetCamera()
    {
        targetY = initialY;
        Vector3 pos = transform.position;
        pos.y = initialY;
        transform.position = pos;
    }
}