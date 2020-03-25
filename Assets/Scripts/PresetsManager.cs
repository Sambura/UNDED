using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PresetsManager : MonoBehaviour
{
    [System.Serializable]
    private struct StringToObject {
        public string key;
        public GameObject _object;
    }

    [System.Serializable]
    private struct StringToSprite
    {
        public string key;
        public Sprite sprite;
    }

    private struct NameReader
    {
        public string name;
    }

    public UnityEngine.UI.Text debugOutput;
    public GameObject errorPrefab;
    public GameObject shieldPrefab;
    public GameObject teleportPrefab;
    public GameObject throwerPrefab;
    public GameObject playerPrefab;
    public Dictionary<string, GameObject> weaponPrefab;
    public Dictionary<string, GameObject> grenadePrefab;
    public Dictionary<string, GameObject> blastPrefab;
    public Dictionary<string, GameObject> bulletPrefab;
    public Dictionary<string, GameObject> enemyPrefab;
    public Dictionary<string, GameObject> fireShotPrefab;
    [SerializeField] private List<StringToObject> weaponsPrefabs;
    [SerializeField] private List<StringToObject> grenadesPrefabs;
    [SerializeField] private List<StringToObject> blastsPrefabs;
    [SerializeField] private List<StringToObject> bulletsPrefabs;
    [SerializeField] private List<StringToObject> enemiesPrefabs;
    [SerializeField] private List<StringToObject> fireShotsPrefabs;

    [SerializeField] private List<StringToSprite> previewImages;

    public static Dictionary<string, string> throwers;
    public static Dictionary<string, string> teleports;
    public static Dictionary<string, string> shields;
    public static Dictionary<string, string> players;
    public static Dictionary<string, string> bullets;
    public static Dictionary<string, string> fireShots; 
    public static Dictionary<string, string> enemies;
    public static Dictionary<string, string> weapons;
    public static Dictionary<string, string> grenades;
    public static Dictionary<string, string> blasts;

    public static Dictionary<string, Dictionary<string, string>> gameObjects;
    public static Dictionary<string, Sprite> images;

    #region Singleton
    public static PresetsManager Instance;
    
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        } else
        {
            Destroy(this);
            return;
        }
        Debug.Log("Presets loaded");
        ReadAssets();
    }
	#endregion

    public GameObject InstantiatePrefab(string type, string key)
    {
        GameObject go = null;
        string prefabVariant = key.Split('~')[0];
        try
        {
            switch (type)
            {
                case "bullet":
                    go = Instantiate(bulletPrefab[prefabVariant]);
                    go.SetActive(false);
                    JsonUtility.FromJsonOverwrite(bullets[key], go.GetComponent<Bullet>());
                    break;
                case "blast":
                    go = Instantiate(blastPrefab[prefabVariant]);
                    go.SetActive(false);
                    var blast = go.GetComponent<Blast>();
                    if (blast is ElectricBlast)
                        JsonUtility.FromJsonOverwrite(blasts[key], blast as ElectricBlast);
                    else
                        if (blast is EffectorBlast)
                        JsonUtility.FromJsonOverwrite(blasts[key], blast as EffectorBlast);
                    else
                        JsonUtility.FromJsonOverwrite(blasts[key], blast);
                    break;
                case "fireShot":
                    go = Instantiate(fireShotPrefab[prefabVariant]);
                    go.SetActive(false);
                    JsonUtility.FromJsonOverwrite(fireShots[key], go.GetComponent<FireShot>());
                    break;
                case "thrower":
                    go = Instantiate(throwerPrefab);
                    go.SetActive(false);
                    JsonUtility.FromJsonOverwrite(throwers[key], go.GetComponent<Thrower>());
                    break;
                case "grenade":
                    go = Instantiate(grenadePrefab[prefabVariant]);
                    go.SetActive(false);
                    JsonUtility.FromJsonOverwrite(grenades[key], go.GetComponent<Grenade>());
                    break;
                case "player":
                    go = Instantiate(playerPrefab);
                    go.SetActive(false);
                    JsonUtility.FromJsonOverwrite(players[key], go.GetComponent<Player>());
                    break;
                case "teleport":
                    go = Instantiate(teleportPrefab);
                    go.SetActive(false);
                    JsonUtility.FromJsonOverwrite(teleports[key], go.GetComponent<Teleport>());
                    break;
                case "shield":
                    go = Instantiate(shieldPrefab);
                    go.SetActive(false);
                    JsonUtility.FromJsonOverwrite(shields[key], go.GetComponent<Shield>());
                    break;
                case "weapon":
                    go = Instantiate(weaponPrefab[prefabVariant]);
                    go.SetActive(false);
                    var weapon = go.GetComponent<Weapon>();
                    if (weapon is Gun)
                        JsonUtility.FromJsonOverwrite(weapons[key], weapon as Gun);
                    else
                    if (weapon is Knife)
                        JsonUtility.FromJsonOverwrite(weapons[key], weapon as Knife);
                    else
                    if (weapon is FireGun)
                        JsonUtility.FromJsonOverwrite(weapons[key], weapon as FireGun);
                    break;
                default:
                    var error = Instantiate(errorPrefab).GetComponent<ErrorProvider>();
                    error.key1 = key;
                    error.key2 = type;
                    error.key3 = "Default switch state";
                    Debug.LogError("Error on Instantiating: unknown tag " + key);
                    break;
            }
        }
        catch (System.NullReferenceException e)
        {
            var error = Instantiate(errorPrefab).GetComponent<ErrorProvider>();
            error.key1 = key;
            error.key2 = type;
            error.key3 = "Null reference exception";
            error.key4 = e.Source + '\n' + e.StackTrace;
        }
        catch (System.ArgumentException e)
        {
            var error = Instantiate(errorPrefab).GetComponent<ErrorProvider>();
            error.key1 = key;
            error.key2 = type;
            error.key3 = "Argument exception";
            error.key4 = e.Source + '\n' + e.StackTrace;
        }
        catch (KeyNotFoundException e)
        {
            var error = Instantiate(errorPrefab).GetComponent<ErrorProvider>();
            error.key1 = key;
            error.key2 = type;
            error.key3 = "Key not found exception";
            error.key4 = e.Source + '\n' + e.StackTrace;
        }
        catch (System.Exception e)
        {
            var error = Instantiate(errorPrefab).GetComponent<ErrorProvider>();
            error.key1 = key;
            error.key2 = type;
            error.key3 = "Unknown exception";
            error.key4 = e.Source + '\n' + e.StackTrace;
        }
        return go;
    }

	private string ConvertFromJson5(string json5)
    {
        bool stringValue = false;
        bool commentLine = false;
        int commentStartIndex = 0;
        // 13 10    Windows
        // 10       Unix
        for (int c = 0; c < json5.Length; c++)
        {
            if (!commentLine)
            {
                if (json5[c] == '\\' && stringValue)
                {
                    c++;
                    continue;
                }

                if (json5[c] == '"' && !stringValue)
                {
                    stringValue = true;
                    continue;
                }

                if (json5[c] == '"' && stringValue)
                {
                    stringValue = false;
                    continue;
                }

                if (!stringValue && json5[c] == '/')
                {
                    //if (c + 1 == json5.Length) throw new System.Exception("Unexpected symbol in the end of the json file");
                    if (json5[c + 1] == '/')
                    {
                        commentLine = true;
                        commentStartIndex = c;
                        c++;
                    }
                    continue;
                }
            }
            else
            {
                if (json5[c] == '\n' || c == json5.Length - 1)
                {
                    int commentEnd = c;
                    json5 = json5.Remove(commentStartIndex, commentEnd - commentStartIndex + 1);
                    c = commentStartIndex - 1;
                    commentLine = false;
                }
            }
        }

        return json5;
    }

    private void ReadAssets()
    {
        shields = new Dictionary<string, string>();
        teleports = new Dictionary<string, string>();
        throwers = new Dictionary<string, string>();
        players = new Dictionary<string, string>();
        weapons = new Dictionary<string, string>();
        bullets = new Dictionary<string, string>();
        grenades = new Dictionary<string, string>();
        blasts = new Dictionary<string, string>();
        fireShots = new Dictionary<string, string>();
        enemies = new Dictionary<string, string>();
        var prefixPath = Application.persistentDataPath + @"/";
#if UNITY_STANDALONE || UNITY_EDITOR
        prefixPath = "";
#endif
        if (debugOutput != null)
        {
            debugOutput.text = prefixPath;
        }
        GetAssets(prefixPath + @"GamePresets\Shields\", shields);
        GetAssets(prefixPath + @"GamePresets\Teleports\", teleports);
        GetAssets(prefixPath + @"GamePresets\GrenadeThrowers\", throwers);
        GetAssets(prefixPath + @"GamePresets\Players\", players);

        gameObjects = new Dictionary<string, Dictionary<string, string>>
        {
            { "shields", shields },
            { "players", players },
            { "bullets", bullets },
            { "teleports", teleports },
            { "throwers", throwers },
            { "weapons", weapons },
            { "fireShots", fireShots },
            { "enemies", enemies },
            { "grenades", grenades },
            { "blasts", blasts }
        };

        weaponPrefab = new Dictionary<string, GameObject>();
        foreach (var i in weaponsPrefabs)
        {
            weaponPrefab.Add(i.key, i._object);
            GetAssets(prefixPath + @"GamePresets\Weapons\" + i.key + @"\", weapons, i.key);
        }

        grenadePrefab = new Dictionary<string, GameObject>();
        foreach (var i in grenadesPrefabs)
        {
            grenadePrefab.Add(i.key, i._object);
            GetAssets(prefixPath + @"GamePresets\Grenades\" + i.key + @"\", grenades, i.key);
        }

        bulletPrefab = new Dictionary<string, GameObject>();
        foreach (var i in bulletsPrefabs)
        {
            bulletPrefab.Add(i.key, i._object);
            GetAssets(prefixPath + @"GamePresets\Bullets\" + i.key + @"\", bullets, i.key);
        }

        fireShotPrefab = new Dictionary<string, GameObject>();
        foreach (var i in fireShotsPrefabs)
        {
            fireShotPrefab.Add(i.key, i._object);
            GetAssets(prefixPath + @"GamePresets\FireShots\" + i.key + @"\", fireShots, i.key);
        }

        blastPrefab = new Dictionary<string, GameObject>();
        foreach (var i in blastsPrefabs)
        {
            blastPrefab.Add(i.key, i._object);
            GetAssets(prefixPath + @"GamePresets\Blasts\" + i.key + @"\", blasts, i.key);
        }
/*
        enemyPrefab = new Dictionary<string, GameObject>();
        foreach (var i in enemiesPrefabs)
        {
            enemyPrefab.Add(i.key, i._object);
            GetAssets(prefixPath + @"GamePresets\Enemies\" + i.key + @"\", enemies, i.key);
        }
*/
        images = new Dictionary<string, Sprite>();
        foreach (var i in previewImages)
        {
            images.Add(i.key, i.sprite);
        }
    }

    private void GetAssets(string path, Dictionary<string, string> reference, string prefix = null)
    {
		#if UNITY_ANDROID
		path = path.Replace(@"\", "/");
		#endif
        var dir = new DirectoryInfo(path);

        if (dir.Exists)
        {
            foreach (var i in dir.GetFiles("*.json", SearchOption.TopDirectoryOnly))
            {
                var reader = i.OpenText();
                var jsonContent = ConvertFromJson5(reader.ReadToEnd());
                reader.Close();
                try
                {
                    var objectName = JsonUtility.FromJson<NameReader>(jsonContent).name;
                    if (objectName == "[DoNotLoadInGame]") continue;
                    reference.Add((prefix == null) ? objectName : (prefix + "~" + objectName), jsonContent);
                }
                catch (System.ArgumentException e)
                {
                    var error = Instantiate(errorPrefab).GetComponent<ErrorProvider>();
                    error.key1 = "File: " + i.FullName;
                    error.key2 = "Reading stage";
                    error.key3 = "Argument exception";
                    error.key4 = e.Source + '\n' + e.StackTrace;
                }
                catch (System.Exception e)
                {
                    var error = Instantiate(errorPrefab).GetComponent<ErrorProvider>();
                    error.key1 = "File: " + i.FullName;
                    error.key2 = "Reading stage";
                    error.key3 = "Unknown exception";
                    error.key4 = e.Source + '\n' + e.StackTrace;
                }
            }
        }
	else {
			Debug.Log($"Directory not found ({path})");
			if (debugOutput != null) debugOutput.text = path + " was not found";
		 }
    }
}