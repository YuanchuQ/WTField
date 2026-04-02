// 让临时特效在指定时间后自动销毁
using UnityEngine;

// 控制短生命周期对象的自动清理
public class AutoDestroy : MonoBehaviour
{
    // 对象存活时间
    [SerializeField] private float lifetime = 0.8f;

    // 启用时安排销毁
    private void OnEnable()
    {
        Destroy(gameObject, lifetime);
    }

    // 由构建器设置存活时间
    public void Configure(float seconds)
    {
        lifetime = Mathf.Max(0.05f, seconds);
    }
}
