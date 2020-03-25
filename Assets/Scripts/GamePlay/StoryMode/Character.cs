using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float walkSpeed = 30;
    public Transform targetPoint;
    public float targetTolerance = 0.5f;
    public bool targetReached = true;
    public bool ignoreY;

    private Animator animator;

    public void StartWalking(float duration)
    {
        targetReached = false;
        if (ignoreY)
            walkSpeed = DistanceX(transform.position, targetPoint.position) / duration; else
            walkSpeed = Vector2.Distance(transform.position, targetPoint.position) / duration;
    }

    private float DistanceX(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x - b.x);
    }

    private float DirectionX(Vector2 self, Vector2 target)
    {
        return (self.x > target.x) ? -1 : 1;
    }

    public void ChangeLookDirection(int direction)
    {
        transform.rotation = Quaternion.Euler(0, 90 - direction * 90, 0);
    }

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (!targetReached)
        {
            if ((DistanceX(targetPoint.position, transform.position) > targetTolerance && ignoreY)
                || (!ignoreY && Vector2.Distance(targetPoint.position, transform.position) > targetTolerance))
            {
                if (animator != null)
                    animator.SetBool("isWalking", true);
                transform.Translate(new Vector2(DistanceX(targetPoint.position, transform.position) > targetTolerance ? walkSpeed * Time.fixedDeltaTime : 0, ignoreY ? 0 : walkSpeed * Time.fixedDeltaTime * Mathf.Sign(targetPoint.position.y - transform.position.y)));
                if (ignoreY)
                    transform.rotation = Quaternion.Euler(0, 90 - DirectionX(transform.position, targetPoint.position) * 90, 0);
            }
            else
            {
                if (animator != null) animator.SetBool("isWalking", false);
                targetReached = true;       
            }
        }
    }
}
