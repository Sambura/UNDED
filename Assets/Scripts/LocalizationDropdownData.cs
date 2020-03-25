using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationDropdownData : Localizable
{
    [SerializeField] private bool autoApply;
    [SerializeField] private string category;
    public string[] keys;

    public List<string> text;

    public override void UpdateData()
    {
        text = new List<string>();
        for (var i = 0; i < keys.Length; i++) {
            if (!LocalizationManager.Instance.locData[category].TryGetValue(keys[i], out string temp))
            {
                text.Add(keys[i]);
                Debug.Log("Missing localization key: " + category + '\\' + keys[i]);
            } 
            text.Add(temp);
        }
        if (autoApply)
        {
            var dropDown = GetComponent<Dropdown>();
            if (dropDown != null)
            {
                var value = dropDown.value;
                dropDown.ClearOptions();
                dropDown.AddOptions(text);
                dropDown.SetValueWithoutNotify(value);
            } else
            {
                var dropDownMesh = GetComponent<TMPro.TMP_Dropdown>();
                if (dropDownMesh != null)
                {
                    var value = dropDownMesh.value;
                    dropDownMesh.ClearOptions();
                    dropDownMesh.AddOptions(text);
                    dropDownMesh.SetValueWithoutNotify(value);
                }
            }
        }
    }
}
