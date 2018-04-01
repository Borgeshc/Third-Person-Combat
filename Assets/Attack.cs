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

    int comboCount;

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
        print("InRange = " + inRange);
		if(Input.GetKeyDown(KeyCode.Mouse0) && !attacking && !movement.isJumping)
        {
            attacking = true;
            StartCoroutine(Attacking());
        }

        if(Input.GetKeyDown(KeyCode.Mouse1) && !attacking && !movement.isJumping)
        {
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
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetPosition), movement.rotationSpeed * Time.deltaTime);

            if (!inRange)
            {
                cc.Move(targetPosition * attackMoveSpeed * Time.deltaTime);
                anim.SetBool("IsSprinting", true);
                anim.SetLayerWeight(2, .5f);
            }
            else
            {
                anim.SetLayerWeight(2, 1);
                anim.SetBool("IsSprinting", false);
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
            print("Not blocking");
            RunCombo();
        }
        else if(blocking && !inRange)
        {
            print("Am blocking,  and not in range");
            blocking = false;
            CancelBlock();
            anim.SetBool("BlockStrike", true);
        }
        else if(blocking && inRange)
        {
            print("Am blocking,  and i'm in range");
            CancelBlock();
            RunCombo();
        }

        yield return new WaitForSeconds(attackFrequency);
        anim.SetBool("Attacking", false);
        anim.SetBool("BlockStrike", false);
        attacking = false;
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
        print("Cancel block");
        blocking = false;
        anim.SetBool("IsBlocking", false);
    }
}
