using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    public float attackFrequency;
    public float chargeRange;
    public float attackRange;
    public float attackMoveSpeed;
    public int maxCombo;
    public float rotationSpeed;

    [Space]
    public int combo1Damage;
    public int combo2Damage;
    public int combo3Damage;

    public GameObject chargeEffect;

    int comboCount;
    int damage;

    [HideInInspector]
    public bool attacking;

    Vector3 targetPosition;

    Coroutine combo;

    Animator anim;
    NavMeshAgent agent;
    VisualEffects visualEffects;

    bool isCharging;
    bool inRange;

    Transform target;
    Health health;

    private void Start()
    {
        health = GetComponent<Health>();
        target = GameObject.Find("Player").transform;
        anim = GetComponentInChildren<Animator>();
        visualEffects = GetComponentInChildren<VisualEffects>();

        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = attackRange;
    }

    void Update()
    {
        if (health.isDead) return;
        if(!inRange)
            anim.SetBool("IsMoving", true);
        else
            anim.SetBool("IsMoving", false);

        anim.SetFloat("Speed", agent.speed);
        if (!inRange && target)
        {
            agent.SetDestination(target.position);
        }

        if (!attacking && inRange)
        {
            attacking = true;
            StartCoroutine(Attacking());
        }

        if (target)
        {
            targetPosition = target.position - transform.position;

            if (targetPosition.magnitude > attackRange)
                inRange = false;
            else
                inRange = true;
        }

        if (attacking)
        {
            if (!target) return;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetPosition), rotationSpeed * Time.deltaTime);

            //if (!inRange)
            //{
            //    if (targetPosition.magnitude < chargeRange)
            //        agent.Move(targetPosition * attackMoveSpeed * Time.deltaTime);
            //    anim.SetBool("IsCharging", true);
            //    anim.SetLayerWeight(2, .5f);
            //}
            //else
            //{
            //    anim.SetLayerWeight(2, 1);
            //    anim.SetBool("IsCharging", false);
            //}
        }
    }

    IEnumerator Attacking()
    {
        if (target)
        {
            Vector3 targetPosition = target.position - transform.position;

            if (targetPosition.magnitude > attackRange)
            {
                comboCount = 2;
                visualEffects.ChargeEffect();
            }
        }

        DealDamage();
        RunCombo();

        yield return new WaitForSeconds(attackFrequency);
        anim.SetBool("Attacking", false);
        attacking = false;
    }

    void DealDamage()
    {
        if (!target) return;
        switch (comboCount)
        {
            case 0:
                target.GetComponent<Health>().TookDamage(combo1Damage, false);
                break;
            case 1:
                target.GetComponent<Health>().TookDamage(combo1Damage, false);
                break;
            case 2:
                target.GetComponent<Health>().TookDamage(combo1Damage, true);
                break;
        }
    }

    public void HitTarget()
    {
        if (target)
        {
            StartCoroutine(target.GetComponent<Health>().HitEffect());
        }
    }

    public void StopAttackAnimation()
    {
        anim.SetBool("Attacking", false);
    }

    void RunCombo()
    {
        anim.SetBool("Attacking", true);
        anim.SetFloat("Combo", comboCount);

        if (combo != null)
            StopCoroutine(combo);

        if (comboCount < maxCombo)
            comboCount++;
        else
            comboCount = 0;

        combo = StartCoroutine(Combo());
    }

    IEnumerator Combo()
    {

        yield return new WaitForSeconds(3);
        comboCount = 0;
    }
}
