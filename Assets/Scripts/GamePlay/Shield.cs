using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Shield : MonoBehaviour
{
    public float maxEnergy = 150f;
    public float damagePerSecond = 25f;
    public float energyRestore = 2f;
    public float rebootThreshold = 0;
    public float rebootDuration = 4f;
    public float rebootBoost = 1f;
    public float shieldScale = 0.8f;
    public Color shieldColor = Color.blue;
    public Color rebootColor = Color.red;
    public DamageType[] damageToBlock;
    public float damageToLight = 0.1f;

    [SerializeField] private float shieldNormalWidth = 50;
    [SerializeField] private GameObject sideShield;
    [SerializeField] private AudioClip[] hitSound;
    [SerializeField] private AudioClip rebootStart;
    [SerializeField] private AudioClip rebootEnd;
    [SerializeField] private AudioClip rebootLoop;
    [SerializeField] private float soundDelta = 0.07f;
    [SerializeField] private float rebootSoundStopDelay = 0.75f;

    public SortedSet<DamageType> toBlockSet;

    private HealthBar shieldBar;
    private Animator animator; 
    private AudioSource audioSource;
    private Light2D shieldLight;
    private float currentEnergy;
    private float rebootEnding;
    private float energyLeft;
    private float shieldWidth;

    private void Start()
    {
        shieldLight = GetComponent<Light2D>();
        animator = GetComponent<Animator>();
        shieldBar = GameObject.FindGameObjectWithTag("ShieldBar").GetComponent<HealthBar>();
        audioSource = GetComponent<AudioSource>();
        currentEnergy = maxEnergy;
        rebootEnding = Time.time;
        shieldBar.Init(1);
        toBlockSet = new SortedSet<DamageType>(damageToBlock);
        audioSource.PlayOneShot(rebootEnd);
        audioSource.pitch = 1 + Random.Range(-soundDelta, soundDelta);
        transform.localScale = new Vector3(shieldScale, shieldScale, 1);
        shieldWidth = shieldNormalWidth * shieldScale;
        shieldLight.color = shieldColor;
        sideShield.GetComponent<Light2D>().color = shieldColor;
    }

    public float GetHit(float damage, float x)
    {
        if (rebootEnding <= Time.time && Mathf.Min(energyLeft, currentEnergy) > 0)
        {
            var sideShieldInstance = Instantiate(sideShield, transform);
            sideShieldInstance.transform.position = transform.position;
            sideShieldInstance.GetComponent<SideShield>().initiaLight = damage * damageToLight;
            if (Mathf.Abs(transform.position.x - x) < shieldWidth / 3)
            {
                sideShieldInstance = Instantiate(sideShield, transform);
                sideShieldInstance.GetComponent<SideShield>().initiaLight = damage * damageToLight;
                sideShieldInstance.transform.position = transform.position;
                sideShieldInstance.transform.Rotate(0, 180, 0);
            }
            else
            if (transform.position.x > x ^ transform.rotation.eulerAngles.y != 0)
            {
                sideShieldInstance.transform.Rotate(0, 180, 0);
            }
            float delta = Mathf.Min(damage, energyLeft, currentEnergy);
            currentEnergy -= delta;
            energyLeft -= delta;
            damage -= delta;
            audioSource.PlayOneShot(hitSound[Random.Range(0, hitSound.Length)]);
            audioSource.pitch = 1 + Random.Range(-soundDelta, soundDelta);
            if (currentEnergy <= rebootThreshold)
            {
                rebootEnding = Time.time + rebootDuration - rebootSoundStopDelay;
                shieldLight.color = rebootColor;
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

    private void Update()
    {
        currentEnergy = Mathf.Clamp(currentEnergy + energyRestore * Time.deltaTime * (rebootEnding != -1 ? rebootBoost : 1), 0, maxEnergy);
        energyLeft = Mathf.Clamp(energyLeft + Time.deltaTime * damagePerSecond, 0, damagePerSecond);
        shieldBar.UpdateHealth(currentEnergy / maxEnergy);
        if (rebootEnding != -1)
        {
            if (rebootEnding <= Time.time)
            {
                animator.SetBool("Rebooting", false);
                shieldBar.ChangeState(0, 1);
                rebootEnding = Time.time + rebootSoundStopDelay * 2;
                audioSource.PlayOneShot(rebootEnd);
                audioSource.pitch = 1 + Random.Range(-soundDelta, soundDelta);
                StartCoroutine(LowerVolume());
            }
        }
    }

    private IEnumerator LowerVolume()
    {
        float duration = rebootSoundStopDelay / audioSource.pitch;
        float startTime = Time.time;
        for (Color color = rebootColor; Time.time - startTime < duration; color = Color.Lerp(rebootColor, shieldColor, (Time.time - startTime) / duration))
        {
            shieldLight.color = color;
            yield return null;
        }
        rebootEnding = -1;
        audioSource.clip = null;
        audioSource.loop = false;
    }
}
