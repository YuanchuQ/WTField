// 兼容不同 Unity 版本的 Rigidbody2D 速度 API
using UnityEngine;

// 提供统一的 2D 刚体速度扩展方法
public static class Rigidbody2DVelocity
{
    // 设置 Rigidbody2D 的线速度
    public static void SetVelocity2D(this Rigidbody2D rb, Vector2 velocity)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = velocity;
#else
        rb.velocity = velocity;
#endif
    }

    // 获取 Rigidbody2D 的线速度
    public static Vector2 GetVelocity2D(this Rigidbody2D rb)
    {
#if UNITY_6000_0_OR_NEWER
        return rb.linearVelocity;
#else
        return rb.velocity;
#endif
    }
}
