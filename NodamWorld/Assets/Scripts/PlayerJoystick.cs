using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerJoystick : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;
    public VariableJoystick variableJoystick;

    [Header("점프 설정")]
    public float jumpForce = 5f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.2f;

    [Header("애니메이션 설정")]
    public Animator animator;                  // 캐릭터의 Animator
    public string paramIsMoving = "IsMoving";  // Bool 파라미터 이름
    public string paramJump = "Jump";          // Trigger 파라미터 이름(선택)
    public float moveThreshold = 0.1f;         // 이동 판정 임계값(조이스틱 입력 크기)
    public float animSpeedDamp = 0.1f;         // 필요 시 Speed 파라미터 쓸 때 감쇠용(현재는 Bool만 사용)

    private Rigidbody rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 모든 회전 고정(충돌 시 빙글빙글 방지). 회전은 코드로만 제어.
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Animator가 비어있다면 자식에서 자동 탐색 시도(선택)
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void FixedUpdate()
    {
        // 조이스틱 입력
        Vector3 inputDir = new Vector3(variableJoystick.Horizontal, 0f, variableJoystick.Vertical);
        bool hasMoveInput = inputDir.magnitude > moveThreshold;

        // 이동/회전
        if (hasMoveInput)
        {
            // 바라보는 방향(월드 기준 입력 방향)
            Quaternion targetRot = Quaternion.LookRotation(inputDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 0.2f);

            // 수평 속도 지정(즉시 반응, 관성 최소화)
            Vector3 v = inputDir.normalized * moveSpeed;
            v.y = rb.linearVelocity.y; // 수직 속도는 중력/점프 유지
            rb.linearVelocity = v;
        }
        else
        {
            // 정지: 수평 속도 0, 수직(중력)은 유지
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        // 착지 판정
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.1f, groundLayer);

        // 애니메이션 전환
        if (animator != null)
        {
            animator.SetBool(paramIsMoving, hasMoveInput);

            // 참고: Speed 파라미터를 쓰고 싶다면 다음과 같이 추가 가능
            // float planarSpeed = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude;
            // animator.SetFloat("Speed", planarSpeed, animSpeedDamp, Time.fixedDeltaTime);
        }
    }

    // UI 버튼 등에서 이 메서드를 연결해 점프를 호출
    public void Jump()
    {
        if (!isGrounded) return;

        // 점프 직전 수직 속도 초기화
        Vector3 v = rb.linearVelocity;
        v.y = 0f;
        rb.linearVelocity = v;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        // 점프 애니메이션 트리거(선택)
        if (animator != null && !string.IsNullOrEmpty(paramJump))
        {
            animator.ResetTrigger(paramJump);
            animator.SetTrigger(paramJump);
        }
    }
}
