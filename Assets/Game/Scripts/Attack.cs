using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public float attackFrequency;
    public float attackRange;
    public float attackMoveSpeed;
    public int maxCombo;

    [Space]
    public int combo1Damage;
    public int combo2Damage;
    public int combo3Damage;
    public int blockStrikeDamage;

    int comboCount;
    int damage;

    [HideInInspector]
    public bool attacking;
    [HideInInspector]
    public bool blocking;

    Vector3 targetPosition;

    Coroutine combo;

    Animator anim;
    Movement movement;
    CharacterController cc;

    bool isCharging;
    bool inRange;

    private void Start()
    {
        movement = GetComponent<Movement>();
        anim = GetComponentInChildren<Animator>();

        cc = GetComponent<CharacterController>();
    }

    void Update ()
    {
		if(Input.GetKeyDown(KeyCode.Mouse0) && !attacking && !movement.isJumping && !movement.isRolling)
        {
            movement.CancelCrouch();
            attacking = true;
            StartCoroutine(Attacking());
        }

        if(Input.GetKey(KeyCode.Mouse1) && !attacking && !movement.isJumping && !movement.isRolling)
        {
            movement.CancelCrouch();
            blocking = true;
            anim.SetBool("IsBlocking", true);
        }

        if(Input.GetKeyUp(KeyCode.Mouse1))
        {
            blocking = false;
            anim.SetBool("IsBlocking", false);
        }

        if (TargetManager.target)
        {
            targetPosition = TargetManager.target.position - transform.position;

            if (targetPosition.magnitude > attackRange)
                inRange = false;
            else
                inRange = true;
        }

        if (attacking)
        {
            if (!TargetManager.target) return;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetPosition), movement.rotationSpeed * Time.deltaTime);

            if (!inRange)
            {
                cc.Move(targetPosition * attackMoveSpeed * Time.deltaTime);
                anim.SetBool("IsCharging", true);
                anim.SetLayerWeight(2, .5f);
            }
            else
            {
                anim.SetLayerWeight(2, 1);
                anim.SetBool("IsCharging", false);
            }
        }
	}

    IEnumerator Attacking()
    {
        if (TargetManager.target)
        {
            Vector3 targetPosition = TargetManager.target.position - transform.position;

            if (targetPosition.magnitude > attackRange)
                comboCount = 2;
        }

        if(!blocking)
        {
            DealDamage();
            RunCombo();
        }
        else if(blocking && !inRange)
        {
            blocking = false;
            CancelBlock();
            if(TargetManager.target)
                TargetManager.target.GetComponent<Health>().TookDamage(blockStrikeDamage, true);
            anim.SetBool("BlockStrike", true);
        }
        else if(blocking && inRange)
        {
            CancelBlock();
            DealDamage();
            RunCombo();
        }

        yield return new WaitForSeconds(attackFrequency);
        anim.SetBool("Attacking", false);
        anim.SetBool("BlockStrike", false);
        attacking = false;
    }

    void DealDamage()
    {
        if (!TargetManager.target) return;
            switch (comboCount)
        {
            case 0:
                TargetManager.target.GetComponent<Health>().TookDamage(combo1Damage, false);
                break;
            case 1:
                TargetManager.target.GetComponent<Health>().TookDamage(combo1Damage, false);
                break;
            case 2:
                TargetManager.target.GetComponent<Health>().TookDamage(combo1Damage, true);
                break;
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

    public void CancelBlock()
    {
        blocking = false;
        anim.SetBool("IsBlocking", false);
    }
}
