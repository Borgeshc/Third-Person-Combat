using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetachOnStart : MonoBehaviour
{
	void Awake ()
    {
        transform.parent = null;
        Destroy(GameObject.Find("Camera"));
	}
}
