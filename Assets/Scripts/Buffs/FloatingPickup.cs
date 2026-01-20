// 给拾取物添加上下漂浮和旋转表现
using UnityEngine;

// 控制拾取物的简单待机动画
public class FloatingPickup : MonoBehaviour
{
    // 上下漂浮高度
    [SerializeField] private float bobHeight = 0.12f;
    // 上下漂浮速度
    [SerializeField] private float bobSpeed = 3f;
    // 自转速度
    [SerializeField] private float rotateSpeed = 45f;

    // 初始本地位置
    private Vector3 startLocalPosition;

    // 记录初始位置
    private void Awake()
    {
        startLocalPosition = transform.localPosition;
    }

    // 每帧更新漂浮和旋转
    private void Update()
    {
        float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = startLocalPosition + Vector3.up * offset;
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }
}
