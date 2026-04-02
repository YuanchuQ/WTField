// 控制主摄像机平滑跟随玩家并保留地图边缘
using UnityEngine;

[RequireComponent(typeof(Camera))]
// 让摄像机在地图可视范围内跟随目标
public class CameraFollow : MonoBehaviour
{
    // 需要跟随的目标
    [SerializeField] private Transform target;
    // 平滑跟随时间
    [SerializeField] private float smoothTime = 0.15f;
    // 地图可视区域最小边界
    [SerializeField] private Vector2 minBounds = new Vector2(-20.4f, -12.2f);
    // 地图可视区域最大边界
    [SerializeField] private Vector2 maxBounds = new Vector2(20.4f, 12.8f);

    // SmoothDamp 使用的速度缓存
    private Vector3 velocity;
    // 当前摄像机组件缓存
    private Camera cameraCache;

    // 缓存摄像机组件
    private void Awake()
    {
        cameraCache = GetComponent<Camera>();
    }

    // 在所有移动完成后更新摄像机位置
    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);
        GetCenterLimits(out Vector2 centerMin, out Vector2 centerMax);
        desired.x = ClampAxis(desired.x, centerMin.x, centerMax.x);
        desired.y = ClampAxis(desired.y, centerMin.y, centerMax.y);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }

    // 由构建器配置跟随目标和可视边界
    public void Configure(Transform followTarget, Vector2 min, Vector2 max, float smooth)
    {
        target = followTarget;
        minBounds = min;
        maxBounds = max;
        smoothTime = Mathf.Max(0.01f, smooth);
    }

    // 根据相机视口计算中心边界
    private void GetCenterLimits(out Vector2 centerMin, out Vector2 centerMax)
    {
        if (cameraCache == null)
            cameraCache = GetComponent<Camera>();

        float halfHeight = cameraCache != null && cameraCache.orthographic ? cameraCache.orthographicSize : 0f;
        float halfWidth = cameraCache != null ? halfHeight * cameraCache.aspect : 0f;
        Vector2 halfExtents = new Vector2(halfWidth, halfHeight);

        centerMin = minBounds + halfExtents;
        centerMax = maxBounds - halfExtents;
    }

    // 在边界过窄时保持居中
    private static float ClampAxis(float value, float min, float max)
    {
        if (min > max)
            return (min + max) * 0.5f;

        return Mathf.Clamp(value, min, max);
    }
}
