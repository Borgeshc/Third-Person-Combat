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

    Attack attack;

    private void Start()
    {
        attack = transform.root.GetComponent<Attack>();
    }

    public void HitEffect()
    {
        if(TargetManager.target)
            CameraShaker.Instance.ShakeOnce(2f, 2f, .1f, .5f);
        attack.HitTarget();
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
