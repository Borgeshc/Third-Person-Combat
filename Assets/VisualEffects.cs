using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualEffects : MonoBehaviour
{
    public GameObject combo1Effect;
    public GameObject combo2Effect;
    public GameObject combo3Effect;
    public GameObject blockStrikeEffect;
    public GameObject chargeEffect;

    public void Combo1Effect()
    {
        combo1Effect.SetActive(true);
    }

    public void Combo2Effect()
    {
        combo2Effect.SetActive(true);
    }

    public void Combo3Effect()
    {
        combo3Effect.SetActive(true);
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
