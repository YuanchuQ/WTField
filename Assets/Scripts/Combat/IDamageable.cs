// 定义可以受到伤害的对象接口
using UnityEngine;

// 统一玩家和敌人的受击入口
public interface IDamageable
{
    // 处理一次带方向和击退的伤害
    void TakeDamage(int amount, Vector2 hitDirection, float knockback);
}
