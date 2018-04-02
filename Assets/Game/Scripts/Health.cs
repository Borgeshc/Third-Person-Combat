using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth;
    public GameObject hitEffect;

    float health;

    bool isDead;
    bool takingDamage;

    Animator anim;

    Attack attack;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        attack = GetComponent<Attack>();
        Revive();
    }

    public void Revive()
    {
        isDead = false;
        health = maxHealth;
        anim.SetBool("IsDead", false);
    }

    public void TookDamage(int damage, bool heavyHit)
    {
        if (isDead) return;
        if (attack && attack.blocking) return;
        health -= damage;

        if(!takingDamage)
        {
            takingDamage = true;
            StartCoroutine(TakingDamage(heavyHit));
        }

        if(health <= 0 && !isDead)
        {
            isDead = true;
            Died();
        }
    }

    IEnumerator TakingDamage(bool heavyHit)
    {
        if (!heavyHit)
            anim.SetTrigger("LightHit");
        else
            anim.SetTrigger("HeavyHit");

        yield return new WaitForSeconds(.25f);
        
        takingDamage = false;
    }

    public IEnumerator HitEffect()
    {
        if (hitEffect)
            hitEffect.SetActive(true);

        yield return new WaitForSeconds(.25f);

        if (hitEffect)
            hitEffect.SetActive(false);
    }

    void Died()
    {
        anim.SetBool("IsDead", true);

        if(transform.tag.Equals("Enemy"))
        {
            GetComponent<Targetable>().RemoveAsTarget();
            gameObject.layer = LayerMask.NameToLayer("Default");

            if (TargetManager.target == transform)
                TargetManager.target = null;
        }
    }
}
