using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 角色的Transform
    public Vector3 offset; // 摄像机与角色之间的偏移
    public float smoothSpeed = 0.125f; // 平滑跟随的速度

    void LateUpdate()
    {
        // 计算目标位置
        Vector3 desiredPosition = target.position + offset;
        // 平滑过渡到目标位置
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // 可选：让摄像机始终朝向角色
        transform.LookAt(target);
    }
}
