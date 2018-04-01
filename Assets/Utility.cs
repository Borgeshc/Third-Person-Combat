using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour
{
    public static float CheckDistance(Vector3 point1, Vector3 point2)
    {
        Vector3 heading;
        float distance;
        Vector3 direction;
        float distanceSquared;

        heading.x = point1.x - point2.x;
        heading.y = point1.y - point2.y;
        heading.z = point1.z - point2.z;

        distanceSquared = heading.x * heading.x + heading.y * heading.y + heading.z * heading.z;
        distance = Mathf.Sqrt(distanceSquared);

        direction.x = heading.x / distance;
        direction.y = heading.y / distance;
        direction.z = heading.z / distance;
        return distance;
    }

    public static List<GameObject> CullList(List<GameObject> listToCull, List<string> exclusionTags)
    {
        List<GameObject> culledList = new List<GameObject>();
        foreach(GameObject go in listToCull)
        {
            if(!exclusionTags.Contains(go.tag))
            {
                culledList.Add(go);
            }
        }

        return culledList;
    }
}