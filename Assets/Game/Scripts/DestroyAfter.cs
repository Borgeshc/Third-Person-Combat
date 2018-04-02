using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    public float destroyAfter;

    private void Start()
    {
        Destroy(gameObject, destroyAfter);
    }
}
