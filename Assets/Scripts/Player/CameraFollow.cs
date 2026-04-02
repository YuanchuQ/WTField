using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private Vector2 minBounds = new Vector2(-20.4f, -12.2f);
    [SerializeField] private Vector2 maxBounds = new Vector2(20.4f, 12.8f);

    private Vector3 velocity;
    private Camera cameraCache;

    private void Awake()
    {
        cameraCache = GetComponent<Camera>();
    }

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

    private static float ClampAxis(float value, float min, float max)
    {
        if (min > max)
            return (min + max) * 0.5f;

        return Mathf.Clamp(value, min, max);
    }
}
