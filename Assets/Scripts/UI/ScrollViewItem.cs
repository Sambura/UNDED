using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewItem : MonoBehaviour
{
	public string itemName;
	[TextArea(3, 10)] public string itemDescription;
	public string legacyName;
	public string nameTag;
	public string descTag;
	public string category;

	public Image itemPreview;
	public Button itemButton;

	private void OnEnable()
	{
		StartCoroutine(Wait());
	}

	private IEnumerator Wait()
	{
		yield return new WaitWhile(() => LocalizationManager.Instance == null);
		UpdateData();
		LocalizationManager.Instance.OnLanguageChanged += UpdateData;
	}

	private void UpdateData()
	{
		itemName = LocalizationManager.Instance.locData[category][nameTag];
		itemDescription = LocalizationManager.Instance.locData[category][descTag];
	}

	private void OnDestroy()
	{
		LocalizationManager.Instance.OnLanguageChanged -= UpdateData;
	}
}
