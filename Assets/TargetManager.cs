using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public static Transform target;
    public float searchFrequency;
    public float targetRadius;
    public LayerMask targetLayer;
    
    bool searchingForTarget;

    private void Update()
    {
        if(!searchingForTarget)
        {
            searchingForTarget = true;
            StartCoroutine(SearchForTarget());
        }
    }

    IEnumerator SearchForTarget()
    {
        print("Searching for target...");
        Collider[] nearbyTargets = Physics.OverlapSphere(transform.position, targetRadius, targetLayer);

        if (nearbyTargets.Length <= 0)
            target = null;

        Transform bestTarget = null;
        bestTarget = FindClosestTarget(nearbyTargets);

        if(bestTarget)
        {
            print("Target found! " + bestTarget.name);
            target = bestTarget;
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
