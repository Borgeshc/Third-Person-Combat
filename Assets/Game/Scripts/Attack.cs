using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Attack : NetworkBehaviour
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

    public GameObject chargeEffect;

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
    VisualEffects visualEffects;

    bool isCharging;
    bool inRange;

    private void Start()
    {
        movement = GetComponent<Movement>();
        anim = GetComponentInChildren<Animator>();
        visualEffects = GetComponentInChildren<VisualEffects>();

        cc = GetComponent<CharacterController>();
    }

    void Update ()
    {
		if(Input.GetKeyDown(KeyCode.Mouse0) && !attacking && !movement.isJumping && !movement.isRolling)
        {
            movement.CancelCrouch();
            attacking = true;
            TriggerAttack();
        }

        if(Input.GetKey(KeyCode.Mouse1) && !attacking && !movement.isJumping && !movement.isRolling)
        {
            movement.CancelCrouch();
            blocking = true;
            CmdBlocking(true);
        }

        if(Input.GetKeyUp(KeyCode.Mouse1))
        {
            blocking = false;
            CmdBlocking(false);
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
                CmdCharging(true);
            
            }
            else
            {
               
                CmdCharging(false);
            }
        }
	}

    [Client]
    void TriggerAttack()
    {
        StartCoroutine(Attacking());
    }

    [Command]
    void CmdDealtDamage() //Tell the server a player dealt damage to an enemy
    {

    }

    [Command]
    void CmdBlocking(bool condition)    //Tell the server to change the blocking animation
    {
        anim.SetBool("IsBlocking", condition);
    }

    [Command]
    void CmdCharging(bool condition)    //Tell the server to change the charging animation
    {
        if (condition)
            anim.SetLayerWeight(2, .5f);
        else
            anim.SetLayerWeight(2, 1);

        anim.SetBool("IsCharging", condition);
    }

    [Command]
    void CmdBlockStrike(bool condition)    //Tell the server to change the BlockStrike animation
    {
        anim.SetBool("BlockStrike", condition);
    }

    [Command]
    void CmdAttacking(bool condition)    //Tell the server to change the Attacking animation
    {
        anim.SetBool("Attacking", condition);
    }

    [Command]
    void CmdCombo(float condition)    //Tell the server to change the Combo animation
    {
        anim.SetFloat("Combo", condition);
    }

    IEnumerator Attacking()
    {
        if (TargetManager.target)
        {
            Vector3 targetPosition = TargetManager.target.position - transform.position;

            if (targetPosition.magnitude > attackRange)
            {
                comboCount = 2;
                visualEffects.ChargeEffect();
            }
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
            CmdBlockStrike(true);
        }
        else if(blocking && inRange)
        {
            CancelBlock();
            DealDamage();
            RunCombo();
        }

        yield return new WaitForSeconds(attackFrequency);
        CmdAttacking(false);
        CmdBlockStrike(false);
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

    public void HitTarget()
    {
        if(TargetManager.target)
        {
            StartCoroutine(TargetManager.target.GetComponent<Health>().HitEffect());
        }
    }

    public void StopAttackAnimation()
    {
        CmdAttacking(false);
    }

    void RunCombo()
    {
        CmdAttacking(true);
        CmdCombo(comboCount);

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
        CmdBlocking(false);
    }
}
