using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform visualRoot;   // 拖你的 character 进来

    void Start()
    {
        if (animator == null && visualRoot != null)
        {
            animator = visualRoot.GetComponent<Animator>();
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationY |
                             RigidbodyConstraints.FreezeRotationZ;
        }

        if (visualRoot == null)
        {
            Debug.LogError("PlayerMovement: visualRoot is not assigned.");
        }
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;

        MoveCharacter(movement);

        if (animator != null)
        {
            animator.SetFloat("MoveX", movement.x);
            animator.SetFloat("MoveZ", movement.z);
        }

        // 只翻 visualRoot，不翻整个玩家根物体
        if (visualRoot != null)
        {
            Vector3 scale = visualRoot.localScale;

            if (moveHorizontal < 0)
            {
                scale.x = -Mathf.Abs(scale.x);
            }
            else if (moveHorizontal > 0)
            {
                scale.x = Mathf.Abs(scale.x);
            }

            visualRoot.localScale = scale;
        }

        LockRotation();
    }

    void MoveCharacter(Vector3 direction)
    {
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void LockRotation()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
            rb.rotation = Quaternion.identity;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        transform.rotation = Quaternion.identity;

        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        LockRotation();
    }
}