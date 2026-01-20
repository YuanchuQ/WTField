// 定义单个 Buff 的数值、图标和展示信息
using UnityEngine;

[CreateAssetMenu(menuName = "WT Field/Buff Definition")]
// 存储可拾取 Buff 的配置数据
public class BuffDefinition : ScriptableObject
{
    // Buff 显示名称
    public string displayName = "Buff";
    // Buff 描述文本
    [TextArea] public string description;
    // Buff 类型
    public BuffType type;
    // HUD 和拾取物使用的图标
    public Sprite icon;
    // Buff 持续时间
    public float duration = 8f;
    // Buff 主倍率
    public float multiplier = 1.5f;
    // Buff 副倍率
    public float secondaryMultiplier = 1f;
    // Buff 整数参数
    public int flatValue = 1;
    // 拾取物显示颜色
    public Color pickupColor = Color.white;
}
