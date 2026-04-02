using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LanguageToggleButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(Toggle);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(Toggle);
    }

    private void Toggle()
    {
        LanguageManager.Instance?.ToggleLanguage();
    }
}
