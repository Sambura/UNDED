using UnityEngine;
using UnityEngine.UI;

public class ScrollViewItem : MonoBehaviour
{
	public string itemName;
	[TextArea(3, 10)] public string itemDescription;
	public string legacyName;

	public Image itemPreview;
	public Button itemButton;
}
