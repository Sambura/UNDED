using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    public float maxEnergy = 150f;
    public float damagePerSecond = 25f;
    public float energyRestore = 2f;
    public float rebootThreshold = 0;
    public float rebootTime = 4f;
    public float rebootBooster = 1f;
    public DamageType[] toBlock;
    public GameObject sideShield;
    public HealthBar shieldBar;
    public AudioClip[] hitSound;
    public AudioClip rebootStart;
    public AudioClip rebootEnd;
    public AudioClip rebootLoop;
    public float soundDelta = 0.03f;
    [HideInInspector] public SortedSet<DamageType> toBlockSet;

    private float currentEnergy;
    private float rebootEnding;
    private Animator animator;
    private float damageNow;
    private AudioSource audioSource;

    void Start()
    {
        currentEnergy = maxEnergy;
        rebootEnding = Time.time;
        animator = GetComponent<Animator>();
        shieldBar.Init(1);
        toBlockSet = new SortedSet<DamageType>(toBlock);
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(rebootEnd);
        audioSource.pitch = 1 + Random.Range(-soundDelta, soundDelta);
    }

    public float GetHit(float damage, float x)
    {
        if (rebootEnding <= Time.time && Mathf.Min(damageNow, currentEnergy) > 0)
        {
            float delta = Mathf.Min(damage, damageNow, currentEnergy);
            currentEnergy -= delta;
            damageNow -= delta;
            damage -= delta;
            var sideShieldInstance = Instantiate(sideShield, transform);
            sideShieldInstance.transform.position = transform.position;
            audioSource.PlayOneShot(hitSound[Random.Range(0, hitSound.Length)]);
            audioSource.pitch = 1 + Random.Range(-soundDelta, soundDelta);
            if (transform.position.x > x ^ transform.rotation.eulerAngles.y != 0)
            {
                sideShieldInstance.transform.Rotate(0, 180, 0);
            }
            if (currentEnergy <= rebootThreshold)
            {
                rebootEnding = Time.time + rebootTime;
                animator.SetBool("Rebooting", true);
                shieldBar.ChangeState(1, 1);
                audioSource.PlayOneShot(rebootStart);
                audioSource.pitch = 1 + Random.Range(-soundDelta, soundDelta); audioSource.clip = rebootLoop;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        return damage;
    }

    void Update()
    {
        currentEnergy = Mathf.Clamp(currentEnergy + energyRestore * Time.deltaTime * (rebootEnding != -1 ? rebootBooster : 1), 0, maxEnergy);
        damageNow = Mathf.Clamp(damageNow + Time.deltaTime * damagePerSecond, 0, damagePerSecond);
        shieldBar.UpdateHealth(currentEnergy / maxEnergy);
        if (rebootEnding != -1)
        {
            if (rebootEnding <= Time.time)
            {
                animator.SetBool("Rebooting", false);
                shieldBar.ChangeState(0, 1);
                rebootEnding = -1;
                audioSource.PlayOneShot(rebootEnd);
                audioSource.pitch = 1 + Random.Range(-soundDelta, soundDelta);
                StartCoroutine(LowerVolume());
            }
        }
    }

    private IEnumerator LowerVolume()
    {
        //yield return new WaitForSeconds(rebootEnd.length * 0.6f / audioSource.pitch);
        yield return new WaitForSeconds(0.75f / audioSource.pitch);
        //audioSource.Stop();
        audioSource.clip = null;
        audioSource.loop = false;
    }
}
