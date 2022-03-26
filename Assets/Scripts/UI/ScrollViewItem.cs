using UnityEngine;
using UnityEngine.UI;

public class ScrollViewItem : Localizable
{
	public string itemName;
	public string itemDescription;
	public string legacyName;
	public string nameKey;
	public string descKey;
	public string category;

	public Image itemPreview;
	public Button itemButton;

	public override void UpdateData()
	{
		if (LocalizationManager.Instance.locData.ContainsKey(category))
		{
			if (!LocalizationManager.Instance.locData[category].TryGetValue(nameKey, out itemName))
				Debug.Log("Missing localization key: " + category + '\\' + nameKey);
			if (!LocalizationManager.Instance.locData[category].TryGetValue(descKey, out itemDescription))
				Debug.Log("Missing localization key: " + category + '\\' + descKey);
		}
		else Debug.Log("Wrong localization category: " + category);
	}
}
