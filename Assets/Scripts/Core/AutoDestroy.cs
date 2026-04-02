using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.8f;

    private void OnEnable()
    {
        Destroy(gameObject, lifetime);
    }
}
