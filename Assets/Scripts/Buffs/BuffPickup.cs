using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BuffPickup : MonoBehaviour
{
    [SerializeField] private BuffDefinition buff;

    public BuffDefinition Buff => buff;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out BuffController controller))
        {
            controller.Apply(buff);
            Destroy(gameObject);
        }
    }
}
