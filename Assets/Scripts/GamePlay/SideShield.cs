using UnityEngine;

public class SideShield : MonoBehaviour
{
    public Sprite[] hitSprites;
    public SpriteRenderer spriteRenderer;
    public UnityEngine.Experimental.Rendering.Universal.Light2D lightRenderer;

    private Transform parent;

    private void Awake()
    {
        spriteRenderer.sprite = hitSprites[Random.Range(0, hitSprites.Length)];
        //lightRenderer.spr = spriteRenderer.sprite;
        parent = transform.parent;
        transform.parent = null;
    }

    private void Update()
    {
        transform.position = parent.position;
    }
}
