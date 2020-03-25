using UnityEngine;

public class LocalizationData : Localizable
{
    [SerializeField] private bool autoApply;
    [SerializeField] private string category;
    public string key;

    public string text;

    public override void UpdateData()
    {
        if (!LocalizationManager.Instance.locData[category].TryGetValue(key, out text))
        {
            Debug.Log("Missing localization key: " + category + '\\' + key);
            return;
        }
        if (autoApply)
        {
            var textMesh = GetComponent<TMPro.TextMeshProUGUI>();
            if (textMesh != null)
            {
                textMesh.text = text;
            } else
            {
                var textUi = GetComponent<UnityEngine.UI.Text>();
                if (textUi != null)
                {
                    textUi.text = text;
                }
            }
        }
    }
}

public class Localizable : MonoBehaviour
{
    public virtual void UpdateData()
    {
        Debug.Log($"Data on {name} has been updated!");
    }

    protected void Awake()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateData;
            UpdateData();
        }
        else
        {
            LocalizationManager.awaiters.Add(this);
        }
    }

    protected void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= UpdateData;
    }
}