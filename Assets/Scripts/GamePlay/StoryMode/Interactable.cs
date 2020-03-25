using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public GameObject labelPrefab;
    public GameObject worldCanvas;
    public string unitName;
    public Transform labelPoint;
    public bool defaultLockState;

    public bool IsLocked { get; set; } = true;
    public bool Interacted { get; set; }

    private GameObject label;

    private void Start()
    {
        IsLocked = defaultLockState;
    }

    public void OnInteractionEnter()
    {
        if (IsLocked) return;
        label = Instantiate(labelPrefab, worldCanvas.transform);
        label.transform.position = labelPoint.position;
        label.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = unitName;
        label.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(() => OnInteraction());
    }

    public void OnInteractionExit()
    {
        if (label != null)
            Destroy(label);
    }

    public virtual void OnInteraction()
    {
        Interacted = true;
        IsLocked = true;
        if (label != null)
            Destroy(label);
    }
}
