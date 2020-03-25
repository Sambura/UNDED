using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoBoard : MonoBehaviour
{
    public TMPro.TextMeshProUGUI titleText;
    public TMPro.TextMeshProUGUI textText;
    public Image image;
    public bool excludeTags;
    public InfoBoardData[] builtIn;

    private Animator animator;
    private bool visible;
    private int isTyping;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void StartDisplay(int dataIndex)
    {
        StartCoroutine(StartDisplayCoroutine(builtIn[dataIndex]));
    }

    public void StartDisplay(InfoBoardData data)
    {
        StartCoroutine(StartDisplayCoroutine(data));
    }

    private IEnumerator StartDisplayCoroutine(InfoBoardData data)
    {
        titleText.text = data.title;
        textText.text = "";
        if (!visible)
        {
            animator.SetTrigger("Start");
            visible = true;
            image.sprite = data.sprite;
            yield return new WaitForSeconds(0.75f);
        } else
        {
            StartCoroutine(ChangeImage(data.sprite));
        }
        StartCoroutine(Typer(data.text));
    }

    private IEnumerator ChangeImage(Sprite sprite)
    {
        float startTime = Time.time;
        Color color = new Color(1, 1, 1, 1);
        while (Time.time - startTime < 0.5f)
        {
            color.a = Mathf.Lerp(1, 0, 2 * (Time.time - startTime));
            image.color = color;
            yield return null;
        }
        image.sprite = sprite;
        startTime = Time.time;
        while (Time.time - startTime < 0.5f)
        {
            color.a = Mathf.Lerp(0, 1, 2 * (Time.time - startTime));
            image.color = color;
            yield return null;
        }
        image.color = new Color(1, 1, 1, 1);
    }

    public void StopDisplay()
    {
        visible = false;
        animator.SetTrigger("End");
    }

    private IEnumerator Typer(string text)
    {
       // string prevText = textText.text;
       // textText.text += text;
       // float fontSize = textText.fontSize;
        //textText.text = prevText;
        //textText.enableAutoSizing = false;
        //textText.fontSize = fontSize;

        if (isTyping == 1)
        {
            isTyping = -1;
            yield return new WaitWhile(() => isTyping == -1);
        }
        isTyping = 1;

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '<' && excludeTags)
            {
                int endIndex = text.IndexOf('>', i) + 1;
                textText.text += text.Substring(i, endIndex + 1 - i);
                i = endIndex;
            }
            else
                textText.text += text[i];
            yield return null;
            if (isTyping == -1)
            {
                break;
            }
        }
       // textText.enableAutoSizing = true;
        isTyping = 0;
    }

    [System.Serializable]
    public class InfoBoardData
    {
        public string title;
        [TextArea(3, 5)]
        public string text;
        public Sprite sprite;
    }
}
