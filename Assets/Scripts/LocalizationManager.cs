using System.Collections.Generic;
using UnityEngine;

// This script handles all the stuff around localization, it should be placed on an object that
// won't be destroyed and should be kept alive as long as you need your game to be localized
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance; // Singleton
    public static List<Localizable> awaiters = new List<Localizable>();

    [SerializeField] private string currentLanguage = "English"; // Adjust this field through editor

    public string CurrentLanguage
    {
        get
        {
            return currentLanguage;
        }
        set
        {
            if (currentLanguage == value) return;
            currentLanguage = value;
            LoadLocalization();
            OnLanguageChanged.Invoke();
        }
    } // Adjust this to automaticly switch language

    public List<string> languages; // List of all available languages that were in files
    public Dictionary<string, Dictionary<string, string>> locData;
   
    public event System.Action OnLanguageChanged; 

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this) Destroy(this);
            return;
        }
        
        LoadLocalization();
        HandleListeners();
    }

    private void LoadLocalization()
    {
        var assets = Resources.LoadAll<TextAsset>("Localization\\");
        locData = new Dictionary<string, Dictionary<string, string>>();
        languages = new List<string>();
        Debug.Log(assets.Length + " localization files detected");
        foreach (var i in assets)
        {
            var text = i.text;
            Debug.Log(text);

            var localData = new Dictionary<string, string>();

            var dataSheet = new List<List<string>>();
            dataSheet.Add(new List<string>());

            bool isQuotes = false;
            int rowIndex = 0;
            int startIndex = 0;
            for (int c = 0; c < text.Length; c++)
            {
                if (!isQuotes)
                {
                    if (text[c] == '"')
                    {
                        isQuotes = true;
                        continue;
                    }

                    if (text[c] == ';')
                    {
                        dataSheet[rowIndex].Add(text.Substring(startIndex, c - startIndex).Replace(@"\n", "\n"));
                        startIndex = c + 1;
                        continue;
                    }

                    if (text[c] == '\r')
                    {
                        dataSheet[rowIndex].Add(text.Substring(startIndex, c - startIndex).Replace(@"\n", "\n"));
                        rowIndex++;
                        dataSheet.Add(new List<string>());
                        startIndex = c + 1;
                        continue;
                    }
                }
                else
                {
                    if (text[c] == '"' && text[c + 1] == '"')
                    {
                        c++;
                        continue;
                    }

                    if (text[c] == '"' && text[c + 1] != '"')
                    {
                        isQuotes = false;
                        dataSheet[rowIndex].Add(text.Substring(startIndex + 1, c - startIndex - 1).Replace("\"\"", "\"").Replace(@"\n", "\n"));
                        c++;
                        startIndex = c + 1;
                        if (c == '\r')
                        {
                            rowIndex++;
                            dataSheet.Add(new List<string>());
                        }
                        continue;
                    }
                }

                continue;
            }

            string debug = "";
            foreach (var j in dataSheet)
            {
                foreach (var k in j)
                {
                    debug += k + "   ";
                }
                debug += '\n';
            }
            Debug.Log("Data read: \n" + debug);

            if (dataSheet[0].Count == 0)
            {
                Debug.Log(i.name + " is empty");
                continue;
            }

            string tag = dataSheet[0][0];

            int colLngIndex = -1;
            for (var j = 1; j < dataSheet[0].Count; j++)
            {
                if (!languages.Contains(dataSheet[0][j])) languages.Add(dataSheet[0][j]);
                if (dataSheet[0][j] == currentLanguage)
                {
                    colLngIndex = j;
                }
            }

            if (colLngIndex != -1)
            for (var j = 1; j < dataSheet.Count - 1; j++)
            {
                if (dataSheet[j][0].Length == 0) continue;
                if (dataSheet[j][0][0] == '#') continue;
                if (dataSheet[j].Count >= colLngIndex + 1)
                    localData.Add(dataSheet[j][0], dataSheet[j][colLngIndex]);
            }

            locData.Add(tag, localData);
        }
    }

    private void HandleListeners()
    {
        foreach (var localizable in awaiters)
        {
            localizable.UpdateData();
            OnLanguageChanged += localizable.UpdateData;
        }
        awaiters.Clear();
    }
}
