using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameSelector : MonoBehaviour
{
    public static string selectedName;

    public Button acceptButton;
    public TMPro.TextMeshProUGUI infoText;
    public int minLength = 1;
    public int maxLength = 20;
    public Color okColor;
    public Color errorColor;

    private void Start()
    {
        acceptButton.interactable = false;
    }

    public void OnTextChanged(string text)
    {
        selectedName = text;
        if (text.Length == Mathf.Clamp(text.Length, minLength, maxLength))
        {
            acceptButton.interactable = true;
            infoText.color = okColor;
        } else
        {
            acceptButton.interactable = false;
            infoText.color = errorColor;
        }
    }

    public void Destroy(float delay) 
    {
        Destroy(gameObject, delay);
    }
}
