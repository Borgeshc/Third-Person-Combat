using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffAfter : MonoBehaviour
{
    public float turnOffAfter;

    void OnEnable()
    {
        StartCoroutine(TurnOff());
    }

    IEnumerator TurnOff()
    {
        yield return new WaitForSeconds(turnOffAfter);
        gameObject.SetActive(false);
    }
}
