using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // 移动速度
    private Animator animator; // Animator组件
    private Rigidbody rb; // Rigidbody组件（可选，如果有的话）

    void Start()
    {
        animator = GetComponent<Animator>(); // 获取Animator组件

        // 检查是否有Rigidbody，如果有就锁定旋转
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 锁定Rigidbody的旋转
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                            RigidbodyConstraints.FreezeRotationY |
                            RigidbodyConstraints.FreezeRotationZ;
        }
    }

    void Update()
    {
        // 获取输入
        float moveHorizontal = Input.GetAxis("Horizontal"); // A和D键或左/右箭头
        float moveVertical = Input.GetAxis("Vertical"); // W和S键或上/下箭头（如果需要）

        // 创建移动向量，不考虑移动方向时的旋转
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical).normalized;

        // 移动角色
        MoveCharacter(movement);

        // 更新动画参数
        animator.SetFloat("MoveX", movement.x); // 设置水平移动参数
        animator.SetFloat("MoveZ", movement.z); // 设置垂直移动参数

        // 镜像角色
        if (moveHorizontal < 0) // 向左移动
        {
            transform.localScale = new Vector3(-10f, 10f, 10f); // 镜像角色
        }
        else if (moveHorizontal > 0) // 向右移动
        {
            transform.localScale = new Vector3(10f, 10f, 10f); // 恢复正常方向
        }

        // 每帧都确保旋转被锁定
        LockRotation();
    }

    void MoveCharacter(Vector3 direction)
    {
        // 使用Transform进行移动
        transform.position += direction * moveSpeed * Time.deltaTime;

        // 不建议使用LookRotation，这会导致角色旋转
        // 保持角色的Y轴旋转为固定值（通常是0）
    }

    // 锁定旋转的方法
    void LockRotation()
    {
        // 方法1：直接设置旋转为固定值
        transform.rotation = Quaternion.Euler(0, 0, 0);

        // 方法2：只锁定X和Z轴，允许Y轴旋转（如果需要面向移动方向的话）
        // transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }

    // 如果使用物理系统，可以添加这些方法
    void FixedUpdate()
    {
        if (rb != null)
        {
            // 确保物理更新时旋转也被锁定
            rb.angularVelocity = Vector3.zero;
            rb.rotation = Quaternion.identity;
        }
    }

    // 可选：防止与其他物体碰撞时旋转
    void OnCollisionEnter(Collision collision)
    {
        // 碰撞时立即重置旋转
        transform.rotation = Quaternion.identity;

        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    // 可选：防止与其他物体持续碰撞时旋转
    void OnCollisionStay(Collision collision)
    {
        // 持续碰撞时也保持旋转锁定
        LockRotation();
    }
}
