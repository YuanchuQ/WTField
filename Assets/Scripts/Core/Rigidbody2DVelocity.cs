using UnityEngine;

public static class Rigidbody2DVelocity
{
    public static void SetVelocity2D(this Rigidbody2D rb, Vector2 velocity)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = velocity;
#else
        rb.velocity = velocity;
#endif
    }

    public static Vector2 GetVelocity2D(this Rigidbody2D rb)
    {
#if UNITY_6000_0_OR_NEWER
        return rb.linearVelocity;
#else
        return rb.velocity;
#endif
    }
}
