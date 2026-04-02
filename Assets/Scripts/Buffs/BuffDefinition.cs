using UnityEngine;

[CreateAssetMenu(menuName = "WT Field/Buff Definition")]
public class BuffDefinition : ScriptableObject
{
    public string displayName = "Buff";
    [TextArea] public string description;
    public BuffType type;
    public Sprite icon;
    public float duration = 8f;
    public float multiplier = 1.5f;
    public float secondaryMultiplier = 1f;
    public int flatValue = 1;
    public Color pickupColor = Color.white;
}
