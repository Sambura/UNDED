using UnityEngine;

public class SideShield : MonoBehaviour
{
    public int spriteCount;
    public Animator animator;

    private Transform parent;

    private void Awake()
    {
        parent = transform.parent;
        transform.parent = null;
        animator.SetInteger("spriteIndex", Random.Range(0, spriteCount));
        animator.SetBool("isLoaded", true);
    }

    private void Update()
    {
        transform.position = parent.position;
    }
}
