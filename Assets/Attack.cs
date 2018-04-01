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

    Coroutine combo;

    Animator anim;
    Movement movement;
    CharacterController cc;

    bool isCharging;

    private void Start()
    {
        movement = GetComponent<Movement>();
        anim = GetComponentInChildren<Animator>();

        cc = GetComponent<CharacterController>();
    }

    void Update ()
    {
		if(Input.GetKeyDown(KeyCode.Mouse0) && !attacking && !movement.isJumping)
        {
            attacking = true;
            StartCoroutine(Attacking());
        }

        if(attacking)
        {
            if(TargetManager.target)
            {
                Vector3 targetPosition = TargetManager.target.position - transform.position;

                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(targetPosition), movement.rotationSpeed * Time.deltaTime);

                if (targetPosition.magnitude > attackRange)
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
	}

    IEnumerator Attacking()
    {
        if (TargetManager.target)
        {
            Vector3 targetPosition = TargetManager.target.position - transform.position;

            if (targetPosition.magnitude > attackRange)
                comboCount = 2;
        }

        anim.SetBool("Attacking", true);
        anim.SetFloat("Combo", comboCount);

        if (combo != null)
            StopCoroutine(combo);

        if (comboCount < maxCombo)
            comboCount++;
        else
            comboCount = 0;

        combo = StartCoroutine(Combo());


        yield return new WaitForSeconds(attackFrequency);
        anim.SetBool("Attacking", false);
        attacking = false;
    }

    public void StopAttackAnimation()
    {
        anim.SetBool("Attacking", false);
    }

    IEnumerator Combo()
    {

        yield return new WaitForSeconds(3);
        comboCount = 0;
    }
}
