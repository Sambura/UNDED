using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublicDestroyer : MonoBehaviour
{
    public void PublicDestroy()
    {
        Destroy(gameObject);
    }
}
