// 在磁力 Buff 生效时把拾取物拉向玩家
using UnityEngine;

// 控制拾取物磁吸效果
public class PickupMagnet : MonoBehaviour
{
    // 磁吸检测半径
    [SerializeField] private float radius = 10f;
    // 拾取物被拉向玩家的速度
    [SerializeField] private float pullSpeed = 8f;

    // 当前是否启用磁吸
    private bool active;

    // 启用时持续拉近范围内的拾取物
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

    // 设置磁吸开关
    public void SetActive(bool value)
    {
        active = value;
    }
}
