using UnityEngine;

public class TrapController : MonoBehaviour
{
    public enum TrapType
    {
        Obstacle,    // 생성되는 장애물
        FallTrap     // 회전하는 발판
    }

    [Header("Trap Settings")]
    public TrapType trapType = TrapType.Obstacle;
    public bool isActivated = false;

    [Header("Fall Trap Settings")]
    public float fallRotationSpeed = 180f; // 회전 속도 (도/초)
    public Vector3 rotationAxis = Vector3.back; // 회전 축 (왼쪽 축: Vector3.back)
    public float maxRotationAngle = 90f; // 최대 회전 각도

    [Header("Obstacle Settings")]
    public GameObject obstacleObject; // Obstacle인 경우 활성화할 오브젝트

    private bool isRotating = false;
    private float currentRotation = 0f;
    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.localRotation;

        // Obstacle 타입이면 시작 시 비활성화
        if (trapType == TrapType.Obstacle && obstacleObject != null)
        {
            obstacleObject.SetActive(false);
        }
    }

    void Update()
    {
        // FallTrap이 활성화되면 회전 시작
        if (trapType == TrapType.FallTrap && isActivated && !isRotating)
        {
            isRotating = true;
        }

        // 회전 처리
        if (isRotating && currentRotation < maxRotationAngle)
        {
            float rotationThisFrame = fallRotationSpeed * Time.deltaTime;
            currentRotation += rotationThisFrame;
            currentRotation = Mathf.Min(currentRotation, maxRotationAngle);

            // 왼쪽을 축으로 아래로 회전
            transform.localRotation = initialRotation * Quaternion.AngleAxis(currentRotation, rotationAxis);
        }
    }

    public void ActivateTrap()
    {
        if (isActivated) return;

        isActivated = true;

        if (trapType == TrapType.Obstacle)
        {
            // Obstacle 생성 (활성화)
            if (obstacleObject != null)
            {
                obstacleObject.SetActive(true);
            }
        }
        else if (trapType == TrapType.FallTrap)
        {
            // FallTrap은 Update에서 회전 시작
            Debug.Log($"FallTrap activated: {gameObject.name}");
        }
    }

    public void ResetTrap()
    {
        isActivated = false;
        isRotating = false;
        currentRotation = 0f;
        transform.localRotation = initialRotation;

        if (trapType == TrapType.Obstacle && obstacleObject != null)
        {
            obstacleObject.SetActive(false);
        }
    }
}