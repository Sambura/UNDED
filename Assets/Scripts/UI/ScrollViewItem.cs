using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollViewItem : MonoBehaviour
{
	public string itemName;
	[TextArea(3, 10)] public string itemDescription;
}
