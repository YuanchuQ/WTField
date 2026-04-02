// 将 UI Text 绑定到本地化文本键
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
// 根据当前语言自动刷新静态 UI 文本
public class LocalizedText : MonoBehaviour
{
    // 本文本对应的本地化键
    [SerializeField] private LocalizedTextKey key;

    // 当前 Text 组件缓存
    private Text text;

    // 缓存 Text 组件
    private void Awake()
    {
        text = GetComponent<Text>();
    }

    // 注册语言变化事件
    private void OnEnable()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.LanguageChanged += Refresh;

        Refresh();
    }

    // 注销语言变化事件
    private void OnDisable()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.LanguageChanged -= Refresh;
    }

    // 由构建器设置本地化键
    public void Configure(LocalizedTextKey textKey)
    {
        key = textKey;
        Refresh();
    }

    // 刷新 Text 内容和字体
    public void Refresh()
    {
        if (text == null)
            text = GetComponent<Text>();

        LanguageManager manager = LanguageManager.Instance;
        if (manager == null || text == null)
            return;

        text.text = manager.GetText(key);
        Font font = manager.GetFont(key);
        if (font != null)
            text.font = font;
    }
}
