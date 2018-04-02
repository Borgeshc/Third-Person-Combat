using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TargetManager : MonoBehaviour
{
    public static Transform target;
    public float searchFrequency;
    public float meleeTargetRadius;
    public float tabTargetRadius;
    public LayerMask targetLayer;

    float targetRadius;

    bool searchingForTarget;

    public Collider[] nearbyTargets;
    Collider targetCollider;

    bool tabTargetActive = true;

    private void Update()
    {
        if (!searchingForTarget)
        {
            searchingForTarget = true;
            StartCoroutine(SearchForTarget());
        }

        if(Input.GetKeyDown(KeyCode.E))
            tabTargetActive = !tabTargetActive;

        if(tabTargetActive && target != null)
        {
            targetRadius = tabTargetRadius;
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                int currentTargetIndex = Array.IndexOf(nearbyTargets, targetCollider);

                if (currentTargetIndex >= nearbyTargets.Length - 1)
                    currentTargetIndex = 0;
                else
                    currentTargetIndex++;

                target.GetComponent<Targetable>().RemoveAsTarget();

                target = nearbyTargets[currentTargetIndex].transform;
                targetCollider = nearbyTargets[currentTargetIndex];

                target.GetComponent<Targetable>().SetAsTarget();
            }
        }
        else
        {
            targetRadius = meleeTargetRadius;
        }
    }

    IEnumerator SearchForTarget()
    {
        nearbyTargets = Physics.OverlapSphere(transform.position, targetRadius, targetLayer);

        if (nearbyTargets.Length <= 0 && target)
        {
            target.GetComponent<Targetable>().RemoveAsTarget();
            target = null;
            targetCollider = null;
        }

        if(target == null || !tabTargetActive)  //If no target, then target the closest
        {
            Transform bestTarget = null;
            bestTarget = FindClosestTarget(nearbyTargets);

            if (bestTarget)
            {

                if (bestTarget != target)
                {
                    if (target)
                        target.GetComponent<Targetable>().RemoveAsTarget();
                    target = bestTarget;
                    targetCollider = target.GetComponent<Collider>();
                    target.GetComponent<Targetable>().SetAsTarget();
                }
            }
        }

        yield return new WaitForSeconds(searchFrequency);
        searchingForTarget = false;
    }

    Transform FindClosestTarget(Collider[] potentialTargets)
    {
        Transform closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (Collider go in potentialTargets)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;

            if (curDistance < distance)
            {
                closest = go.transform;
                distance = curDistance;
            }
        }

        return closest;
    }
}
