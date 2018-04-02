using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targetable : MonoBehaviour
{
    public GameObject targetArrow;

    [HideInInspector]
    public bool isTargeted;

	public void SetAsTarget()
    {
        isTargeted = true;
        targetArrow.SetActive(true);
    }

    public void RemoveAsTarget()
    {
        isTargeted = false;
        targetArrow.SetActive(false);
    }
}
