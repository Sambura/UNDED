using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public List<Dialogue> dialogues;
    public static DialogueManager Instance;
    public TMPro.TextMeshProUGUI nameText;
    public TMPro.TextMeshProUGUI sentenceText;
    public GameObject dialogueObject;

    public bool dialogueInProcess;

    public void StartConversation(int index)
    {
        dialogueInProcess = true;
        dialogueObject.GetComponent<Animator>().SetTrigger("Start");
        StartCoroutine(Conversation(index));
    }

    private IEnumerator Conversation(int index)
    {
        var sentences = dialogues[index].sentences;
        foreach (var i in dialogues[index].units)
        {
            i.name = LocalizationManager.Instance.locData["Dialogues"][i.tag];
        }
        for (var i = 0; i < sentences.Count; i++)
        {
            nameText.text = dialogues[index].units[sentences[i].unitIndex].name.Replace("{playerName}", NameSelector.selectedName); ;
            sentenceText.text = "";
            var text = LocalizationManager.Instance.locData["Dialogues"][sentences[i].tag];
            text = text.Replace("{playerName}", NameSelector.selectedName);
            yield return new WaitForSeconds(0.2f);
            foreach (char c in text)
            {
                yield return null;
                sentenceText.text += c;
                if (Input.anyKey)
                {
                    sentenceText.text = text;
                    break;
                }
            }
            yield return new WaitUntil(() => Input.anyKey);
        }
        dialogueInProcess = false;
        dialogueObject.GetComponent<Animator>().SetTrigger("End");
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    [System.Serializable]
    public class Dialogue
    {
        public List<DialogueUnit> units;
        public List<Sentence> sentences;
    }

    [System.Serializable]
    public class DialogueUnit
    {
        public GameObject unit;
        public string name;
        public string tag;
    }

    [System.Serializable]
    public class Sentence
    {
        public int unitIndex;
        [TextArea(4, 10)]
        public string tag;
    }
}
