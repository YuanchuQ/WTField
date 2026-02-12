// 处理菜单中的中英文切换按钮
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
// 点击按钮时切换当前界面语言
public class LanguageToggleButton : MonoBehaviour
{
    // 关联的按钮组件
    private Button button;

    // 缓存按钮组件
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    // 注册按钮点击事件
    private void OnEnable()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(Toggle);
    }

    // 注销按钮点击事件
    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(Toggle);
    }

    // 调用语言管理器切换语言
    private void Toggle()
    {
        LanguageManager.Instance?.ToggleLanguage();
    }
}
