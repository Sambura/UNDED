using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class SideShield : MonoBehaviour
{
    public int spriteCount;
    public Animator animator;
    public float initiaLight;
    [SerializeField] private new Light2D light;

    private Transform parent;
    private float deathTime = 25f / 60f;
    private float startTime;

    private void Awake()
    {
        parent = transform.parent;
        transform.parent = null;
        animator.SetInteger("spriteIndex", Random.Range(0, spriteCount));
        animator.SetBool("isLoaded", true);
        startTime = Time.time;
        light.intensity = initiaLight;
    }

    private void Update()
    {
        transform.position = parent.position;
        light.intensity = Mathf.Lerp(initiaLight, 0, (Time.time - startTime) / deathTime);
    }
}
