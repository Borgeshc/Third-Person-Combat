using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class VisualEffects : MonoBehaviour
{
    public GameObject combo1Effect;
    public GameObject combo2Effect;
    public GameObject combo3Effect;
    public GameObject blockStrikeEffect;
    public GameObject chargeEffect;

    public Collider weaponCollider;

    Attack attack;
    AI ai;

    private void Start()
    {
        attack = transform.root.GetComponent<Attack>();
        ai = transform.root.GetComponent<AI>();
    }

    public void HitEffect()
    {
        if(transform.root.tag.Equals("Player"))
        {
            if (TargetManager.target)
                CameraShaker.Instance.ShakeOnce(2f, 2f, .1f, .5f);

            attack.HitTarget();
        }
        else if(transform.root.tag.Equals("Enemy"))
        {
            ai.HitTarget();
        }
    }

    public void Combo1Effect()
    {
        combo1Effect.SetActive(true);
        HitEffect();
    }

    public void Combo2Effect()
    {
        combo2Effect.SetActive(true);
        HitEffect();
    }

    public void Combo3Effect()
    {
        combo3Effect.SetActive(true);
        HitEffect();
    }

    public void BlockStrikeEffect()
    {
        blockStrikeEffect.SetActive(true);
    }

    public void ChargeEffect()
    {
        chargeEffect.SetActive(true);
    }
}
