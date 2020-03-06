using UnityEngine;
using System.Collections.Generic;

public class Decorations : MonoBehaviour
{
    public Decoration[] decorations;

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        foreach (var i in decorations)
        {
            int count = Random.Range(i.minCount, i.maxCount + 1);
            List<Vector2> objects = new List<Vector2>();
            for (int j = 0; j < count; j++)
            {
                Vector2 position;
                int z = 0;
                while (true)
                {
                    z++;
                    position = new Vector2(Random.Range(i.xMin, i.xMax), Random.Range(i.yMin, i.yMax));

                    if (!objects.Exists(new System.Predicate<Vector2>((Vector2 x) =>
                    {
                        return (Mathf.Abs(position.x - x.x) <= i.width) && (Mathf.Abs(position.y - x.y) <= i.height);
                    }))) break;

                    if (z > 1000) break;
                }
                if (z > 1000) break;

                Instantiate(i.prefabs[Random.Range(0, i.prefabs.Length)], position, Quaternion.identity);
                objects.Add(position);
            }
        }
    }
}

[System.Serializable]
public struct Decoration
{
    public GameObject[] prefabs;
    public int minCount;
    public int maxCount;
    public float xMax;
    public float xMin;
    public float yMax;
    public float yMin;
    public float width;
    public float height;
}