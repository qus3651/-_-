using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class TowerSquareController : MonoBehaviour
{
    public enum SquareState { Waiting, Falling, RollingLeft, RollingRight, GameOver }
    public enum Direction { Left, Right }

    [Header("Config")]
    public KeyCode toggleKey = KeyCode.Space;
    public Transform visualQuad;
    public float quadRollSpeed = 360f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip coinSound;

    [Header("Runtime")]
    public SquareState State { get; private set; } = SquareState.Waiting;
    public bool IsGameOver => State == SquareState.GameOver;
    public Direction NextDirection { get; private set; } = Direction.Right;

    private TowerGameManager gm;
    private Rigidbody rb;
    private BoxCollider boxCol;
    private BoxCollider currentPlatform;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        boxCol = GetComponent<BoxCollider>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void Initialize(TowerGameManager manager, bool startFalling = true)
    {
        gm = manager;
        State = startFalling ? SquareState.Falling : SquareState.Waiting;
        currentPlatform = null;
        NextDirection = Direction.Right;

        if (visualQuad != null)
        {
            visualQuad.localRotation = Quaternion.identity;
        }
    }

    public void StartFalling()
    {
        if (State == SquareState.Waiting)
        {
            State = SquareState.Falling;
        }
    }

    public void UpdateQuadRollSpeed(float newSpeed)
    {
        quadRollSpeed = newSpeed;
    }

    // ★ 텔레포트용: 현재 플랫폼 제거 및 낙하 상태로 전환
    public void ClearCurrentPlatform()
    {
        currentPlatform = null;
        State = SquareState.Falling;
        Debug.Log("[TowerSquareController] Platform cleared - now falling");
    }

    public void ReverseDirection()
    {
        if (State == SquareState.RollingLeft)
        {
            State = SquareState.RollingRight;
            NextDirection = Direction.Right;
        }
        else if (State == SquareState.RollingRight)
        {
            State = SquareState.RollingLeft;
            NextDirection = Direction.Left;
        }

        TowerUIManager.Instance?.UpdateNextDirOnly(NextDirection);
    }

    void Update()
    {
        if (IsGameOver) return;

        if (State == SquareState.Waiting)
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleNextDirection();
            }
            return;
        }

        if (Input.GetKeyDown(toggleKey))
        {
            ToggleNextDirection();
        }

        if (visualQuad != null)
        {
            if (State == SquareState.RollingLeft)
            {
                visualQuad.Rotate(Vector3.forward, quadRollSpeed * Time.deltaTime, Space.World);
            }
            else if (State == SquareState.RollingRight)
            {
                visualQuad.Rotate(Vector3.back, quadRollSpeed * Time.deltaTime, Space.World);
            }
        }
    }

    void FixedUpdate()
    {
        if (IsGameOver || State == SquareState.Waiting) return;

        Vector3 move = Vector3.zero;
        switch (State)
        {
            case SquareState.Falling:
                move = Vector3.down * gm.GetFallSpeed() * Time.fixedDeltaTime;
                break;
            case SquareState.RollingLeft:
                move = Vector3.left * gm.currentHorizontalSpeed * Time.fixedDeltaTime;
                break;
            case SquareState.RollingRight:
                move = Vector3.right * gm.currentHorizontalSpeed * Time.fixedDeltaTime;
                break;
        }

        rb.MovePosition(transform.position + move);

        if (State == SquareState.RollingLeft || State == SquareState.RollingRight)
        {
            CheckPlatformEdge();
        }
    }

    public void ToggleNextDirection()
    {
        NextDirection = (NextDirection == Direction.Left) ? Direction.Right : Direction.Left;
        TowerUIManager.Instance?.UpdateNextDirOnly(NextDirection);
    }

    private void CheckPlatformEdge()
    {
        if (currentPlatform == null) return;

        Bounds b = currentPlatform.bounds;
        float halfWidth = boxCol.bounds.extents.x;
        float centerX = transform.position.x;

        if (State == SquareState.RollingLeft)
        {
            float tailX = centerX + halfWidth;
            if (tailX <= b.min.x - 0.01f)
            {
                currentPlatform = null;
                State = SquareState.Falling;
            }
        }
        else if (State == SquareState.RollingRight)
        {
            float tailX = centerX - halfWidth;
            if (tailX >= b.max.x + 0.01f)
            {
                currentPlatform = null;
                State = SquareState.Falling;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsGameOver) return;

        

        if (other.CompareTag("Coin"))
        {
            // ★ 코인 사운드 설정 확인
            if (audioSource != null && coinSound != null && TowerUIManager.Instance != null)
            {
                if (TowerUIManager.Instance.IsCoinSoundEnabled())
                {
                    audioSource.PlayOneShot(coinSound);
                }
            }

            gm?.CollectCoin();
            Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag("Spring"))
        {
            SpringTrap spring = other.GetComponent<SpringTrap>();
            if (spring != null)
            {
                spring.OnSpringHit();
                ReverseDirection();
            }
            return;
        }

        if (other.CompareTag("TriggerButton"))
        {
            TriggerButton trigger = other.GetComponent<TriggerButton>();
            if (trigger != null)
            {
                trigger.OnTriggerActivated();
            }
            return;
        }

        if (other.CompareTag("TriggerObstacle"))
        {
            TriggerObstacle obstacle = other.GetComponent<TriggerObstacle>();
            if (obstacle != null)
            {
                obstacle.OnObstacleHit();
            }
            SetGameOver();
            return;
        }

        if (other.CompareTag("Wall"))
        {
            SetGameOver();
            return;
        }

        if (other.CompareTag("Platform") && State == SquareState.Falling)
        {
            BoxCollider plat = other as BoxCollider;
            if (plat != null)
            {
                Bounds b = plat.bounds;
                float topY = b.max.y + boxCol.bounds.extents.y + 0.01f;
                Vector3 p = transform.position;
                p.y = topY;
                transform.position = p;

                currentPlatform = plat;
                State = (NextDirection == Direction.Left) ? SquareState.RollingLeft : SquareState.RollingRight;
            }
        }
    }

    public void SetGameOver()
    {
        State = SquareState.GameOver;
        gm?.OnGameOver();
    }
}