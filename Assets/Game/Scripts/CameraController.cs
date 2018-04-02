using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float speed;

    GameObject player;
    Vector3 velocity;

    private void Start()
    {
        player = GameObject.Find("Player");
    }

    private void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position, player.transform.position, ref velocity, speed);
    }
}
