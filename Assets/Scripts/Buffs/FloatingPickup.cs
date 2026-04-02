using UnityEngine;

public class FloatingPickup : MonoBehaviour
{
    [SerializeField] private float bobHeight = 0.12f;
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField] private float rotateSpeed = 45f;

    private Vector3 startLocalPosition;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = startLocalPosition + Vector3.up * offset;
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }
}
