using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("대상 설정")]
    public Transform target;                 // 따라갈 캐릭터(플레이어) Transform

    [Header("오프셋/속도")]
    public Vector3 offset = new Vector3(0f, 8f, -8f); // 캐릭터 기준 세계좌표 오프셋
    public float followSmooth = 10f;                  // 위치 추적 보간 속도 (값이 클수록 더 단단히 붙음)

    [Header("회전 고정 옵션")]
    public bool lockCameraRotation = true;  // 카메라 회전을 고정할지 여부 (기본 true)
    public bool useLookAt = false;          // 캐릭터를 바라보도록 회전할지 여부 (기본 false)

    [Header("장애물 간단 대응(선택)")]
    public bool avoidObstacles = true;      // 카메라-타깃 사이 장애물에 밀착 방지
    public LayerMask obstacleMask;          // 장애물 레이어
    public float minDistance = 2.0f;        // 장애물 회피 시 최소 거리
    public float maxDistance = 20.0f;       // 기본 오프셋 거리의 상한

    private Quaternion initialCamRotation;  // 시작 시 카메라 회전(고정용)
    private Vector3 initialWorldOffset;     // 시작 시 세계좌표 오프셋(옵션)

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("[CameraFollow] target이 비어 있습니다. 플레이어 Transform을 할당하세요.");
            enabled = false;
            return;
        }

        // 시작 시점의 회전과 오프셋을 기록
        initialCamRotation = transform.rotation;

        // 인스펙터에서 지정한 offset이 0,0,0이면 현재 카메라-타깃 간 벡터를 사용
        if (offset == Vector3.zero)
            offset = transform.position - target.position;

        // 세계좌표 오프셋 유지(카메라 회전 변동 없음)
        initialWorldOffset = offset;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 원하는 카메라 위치(세계좌표 오프셋 유지)
        Vector3 desiredPos = target.position + initialWorldOffset;

        // 장애물 회피(선택)
        if (avoidObstacles)
        {
            // 타깃에서 카메라로 레이를 쏴서 충돌 시 카메라를 충돌 지점 앞쪽으로 당김
            Vector3 dir = (desiredPos - target.position);
            float desiredDist = dir.magnitude;
            if (desiredDist > 0.001f)
            {
                dir /= desiredDist;

                // 최대/최소 거리 제한
                desiredDist = Mathf.Clamp(desiredDist, minDistance, maxDistance);

                if (Physics.SphereCast(target.position, 0.25f, dir, out RaycastHit hit, desiredDist, obstacleMask))
                {
                    // 충돌하면 충돌 지점보다 약간 앞(타깃 쪽)으로
                    float safeDist = Mathf.Clamp(hit.distance - 0.2f, minDistance, desiredDist);
                    desiredPos = target.position + dir * safeDist;
                }
                else
                {
                    // 충돌 없으면 원래 거리 유지
                    desiredPos = target.position + dir * desiredDist;
                }
            }
        }

        // 부드럽게 위치 보간
        transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));

        // 회전 처리
        if (lockCameraRotation && !useLookAt)
        {
            // 시작 회전 고정: 캐릭터가 어떻게 돌든 카메라는 같은 각도 유지
            transform.rotation = initialCamRotation;
        }
        else if (useLookAt)
        {
            // 캐릭터를 바라보도록 회전(필요할 때만 사용)
            Vector3 lookDir = (target.position - transform.position);
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));
            }
        }
        // 둘 다 false라면, 인스펙터에서 수동으로 둔 카메라 회전을 유지
    }
}
