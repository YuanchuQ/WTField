using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private LocalizedTextKey key;

    private Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    private void OnEnable()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.LanguageChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.LanguageChanged -= Refresh;
    }

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
