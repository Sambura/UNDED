using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DebugMenuController : MonoBehaviour
{
	[System.Serializable]
	private struct ItemData
	{
		public string name;
		public string previewImage;
		public string description;
		public Color previewImageColor;
	}

	public ScrollViewScript scrollView;
	public string objectKey;
	public GameObject listItemPrefab;
	public Vector2 itemOffset;

	void Start()
	{
		var reference = PresetsManager.gameObjects[objectKey];

		var itemPosition = Vector2.zero;
		foreach (var i in reference)
		{
			var data = JsonUtility.FromJson<ItemData>(i.Value);
			var item = Instantiate(listItemPrefab, scrollView.transform);
			item.transform.localPosition = itemPosition;
			item.transform.localScale = new Vector2(0.5f, 0.5f);
			itemPosition += itemOffset;
			var itemScript = item.GetComponent<ScrollViewItem>();
			if (PresetsManager.images.TryGetValue(data.previewImage, out Sprite sprite))
				itemScript.itemPreview.sprite = sprite;
			itemScript.itemName = data.name;
			itemScript.legacyName = i.Key;
			itemScript.itemDescription = data.description;
			if (data.previewImageColor != Color.clear)
				itemScript.itemPreview.color = data.previewImageColor;
		}

		scrollView.UpdateContent();
	}
}
