using UnityEngine;

public class PickupMagnet : MonoBehaviour
{
    [SerializeField] private float radius = 10f;
    [SerializeField] private float pullSpeed = 8f;

    private bool active;

    private void Update()
    {
        if (!active)
            return;

        BuffPickup[] pickups = FindObjectsByType<BuffPickup>(FindObjectsSortMode.None);
        foreach (BuffPickup pickup in pickups)
        {
            if (pickup == null)
                continue;

            float distance = Vector2.Distance(transform.position, pickup.transform.position);
            if (distance > radius)
                continue;

            pickup.transform.position = Vector3.MoveTowards(
                pickup.transform.position,
                transform.position,
                pullSpeed * Time.deltaTime);
        }

    }

    public void SetActive(bool value)
    {
        active = value;
    }
}
