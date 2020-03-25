using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsSelector : MonoBehaviour
{
    public SelectorButton[] buttons;
    public TMPro.TextMeshProUGUI nameText;
    public TMPro.TextMeshProUGUI descriptionText;
    public Transform point;

    private ScrollViewItem[] itemsData;
    private int lastSelected;
    private int lastTrueSelected;

    void Start()
    {
        itemsData = new ScrollViewItem[buttons.Length];
        for (var i = 0; i < buttons.Length; i++)
        {
            itemsData[i] = buttons[i].GetComponent<ScrollViewItem>();
            buttons[i].enabled = true;
        }

        buttons[0].Select();
        buttons[0].trueSelected = true;
        lastSelected = 0;
        lastTrueSelected = 0;
        UpdateData(0);
    }

    void UpdateData(int index)
    {
        nameText.text = itemsData[index].itemName;
        descriptionText.text = itemsData[index].itemDescription;
    }

    public void OnAccept()
    {
        GetComponent<Animator>().SetTrigger("End");
        for (var i = 0; i < buttons.Length; i++)
        {
            //buttons[i].Deselect();
            buttons[i].enabled = false;
        }
        point.position = buttons[lastTrueSelected].transform.position - new Vector3(7, 0);
        buttons[lastTrueSelected].GetComponent<Animator>().SetBool("Selected", true);
        StartCoroutine(Deselect());
    }

    private IEnumerator Deselect()
    {
        yield return new WaitForSeconds(3);
        for (var i = 0; i < buttons.Length; i++)
        {
            buttons[i].enabled = true;
            buttons[i].Deselect();
            buttons[i].enabled = false;
        }
        Destroy(gameObject);
    }

    void Update()
    {
        for (var i = 0; i < buttons.Length; i++)
        {
            if (lastSelected == i)
            {
                if (!buttons[i].selected)
                {
                    lastSelected = -1;
                }
            }
            else
            {
                if (buttons[i].selected)
                {
                    for (var j = 0; j < buttons.Length; j++)
                    {
                        if (i != j && buttons[j].selected) buttons[j].Deselect();
                    }
                    lastSelected = i;
                    UpdateData(i);
                }
            }

            if (lastTrueSelected != i && buttons[i].trueSelected)
            {
                buttons[lastTrueSelected].trueSelected = false;
                buttons[lastTrueSelected].Deselect();
                lastTrueSelected = i;
            }
        }

        if (lastSelected == -1)
        {
            UpdateData(lastTrueSelected);
            buttons[lastTrueSelected].Select();
        }
    }
}
