using UnityEngine;

public class CoinRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 180f; // Y축 회전 속도 (도/초)

    void Update()
    {
        // Y축 기준으로 빙글빙글 회전
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }
}