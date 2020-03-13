using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorProvider : MonoBehaviour
{
    public Text key1Text, key2Text, key3Text, key4Text;
    public string key1, key2, key3, key4;

    void Start()
    {
        Camera.main.orthographicSize = 90;
        key1Text.text = key1;
        key2Text.text = key2;
        key3Text.text = key3;
        key4Text.text = key4;
    }
}
