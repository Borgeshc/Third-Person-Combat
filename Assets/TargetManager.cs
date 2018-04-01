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
        foreach(Collider nearbyTarget in nearbyTargets)
        {
            bestTarget = CheckDistance(nearbyTarget.transform.gameObject, transform.position);
        }

        if(bestTarget)
        {
            print("Target found! " + bestTarget.name);
            target = bestTarget;
        }

        yield return new WaitForSeconds(searchFrequency);
        searchingForTarget = false;
    }

    Transform CheckDistance(GameObject potentialTarget, Vector3 searchPos)
    {
        if (!potentialTarget) return null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 directionToTarget = potentialTarget.transform.position - searchPos;
        float dSqrToTarget = directionToTarget.sqrMagnitude;
        if (dSqrToTarget < closestDistanceSqr)
        {
            closestDistanceSqr = dSqrToTarget;
            return potentialTarget.transform;
        }
        else
            return null;
    }
}
