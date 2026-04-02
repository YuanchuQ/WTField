// 处理场景中 Buff 拾取物的触发逻辑
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
// 让玩家碰到后获得指定 Buff
public class BuffPickup : MonoBehaviour
{
    // 拾取后应用的 Buff 数据
    [SerializeField] private BuffDefinition buff;

    // 暴露当前拾取物绑定的 Buff
    public BuffDefinition Buff => buff;

    // 由构建器设置拾取物数据
    public void Configure(BuffDefinition definition)
    {
        buff = definition;
    }

    // 玩家进入触发器时应用 Buff 并销毁拾取物
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out BuffController controller))
        {
            controller.Apply(buff);
            Destroy(gameObject);
        }
    }
}
