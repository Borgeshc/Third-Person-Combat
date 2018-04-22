using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Movement : NetworkBehaviour
{
 [Header("Movement Variables")]
    public float movementSpeed;
    public float sprintSpeed;
    public float rollSpeed;
    public float rotationSpeed;
    public float animationSmoothing = 10f;

    [Space, Header("Jump Variables")]
    public float jumpForce = 8f;
    public float gravity = 20f;
    public float distToGround = 1.1f;
    public LayerMask groundLayer;
    public Camera myCamera;

    float speed;

    float moveX;
    float moveY;

    float xRotationValue;

    float jump;

    float ccVelocity;

    [HideInInspector]
    public bool isMoving;
    [HideInInspector]
    public bool isJumping;
    [HideInInspector]
    public bool isRolling;
    bool isSprinting;
    bool waiting;
    bool landing;
    bool isCrouching;

    Vector3 movement;
    Quaternion rotation;
    CharacterController cc;
    Animator anim;

    Vector3 current_pos;
    Vector3 last_pos;

    Attack attack;

    float colliderHeight;
    Vector3 colliderCenter;
    CapsuleCollider capsuleCollider;

    private void Start()
    {
        attack = GetComponent<Attack>();
        anim = GetComponentInChildren<Animator>();
        speed = movementSpeed;

        cc = GetComponent<CharacterController>();

        current_pos = transform.position;
        last_pos = transform.position;

        capsuleCollider = GetComponent<CapsuleCollider>();
        colliderHeight = capsuleCollider.height;
        colliderCenter = capsuleCollider.center;
    }

    public void RecieveInput()
    {
        moveX = Input.GetAxis("Horizontal");
        moveY = Input.GetAxis("Vertical");
        isSprinting = Input.GetKey(KeyCode.LeftShift);

        if(isSprinting)
        {
            CancelCrouch();
        }

        if (Input.GetKeyDown(KeyCode.Space) && !attack.attacking && !isRolling && !landing)
        {
            CancelCrouch();
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) && !attack.attacking && !isJumping)
        {
            attack.CancelBlock();
            isRolling = true;
            CmdRoll();
        }

        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
            CmdCrouching(isCrouching);

            if(isCrouching)
            {
                capsuleCollider.height = colliderHeight / 2;
                capsuleCollider.center = new Vector3(colliderCenter.x, colliderCenter.y / 2, colliderCenter.z);
            }
            else
            {
                capsuleCollider.height = colliderHeight;
                capsuleCollider.center = colliderCenter;
            }
        }
    }

    [Command]
    void CmdCrouching(bool condition)
    {
        anim.SetBool("IsCrouching", condition);
    }

    [Command]
    void CmdJumping(bool condition)
    {
        anim.SetBool("IsJumping", condition);
    }

    [Command]
    void CmdRoll()
    {
        anim.SetTrigger("Roll");
    }

    public void CancelCrouch()
    {
        isCrouching = false;
        CmdCrouching(false);
        capsuleCollider.height = colliderHeight;
        capsuleCollider.center = colliderCenter;
    }

    [Command]
    void CmdAnimate()
    {
        CalculateCCSpeed();
        anim.SetFloat("Speed", ccVelocity, animationSmoothing, Time.deltaTime);
        anim.SetFloat("AirVelocity", cc.velocity.y);
   
        if (movement == Vector3.zero)
            anim.SetBool("IsMoving", false);
        else
            anim.SetBool("IsMoving", true);
    }


    public void StopRolling()
    {
        isRolling = false;
    }

    private void Update()
    {
        if (attack.attacking) return;
        Jumping();

        RecieveInput();
        CmdAnimate();
        if ( attack.blocking) return;// landing ||
         Move();

        if(movement != Vector3.zero)
        {
            if (isSprinting)
                speed = sprintSpeed;
            else
                speed = movementSpeed;
        }

        if (isJumping && Grounded() && !waiting)
        {
            CmdJumping(false);

            isJumping = false;
            landing = true;
        }

        if(isRolling)
        {
            if(movement != Vector3.zero)
                cc.Move(movement * rollSpeed * Time.deltaTime);
            else
                cc.Move(transform.forward * rollSpeed * Time.deltaTime);
        }
    }

    public void Landed()
    {
        landing = false;
    }

    IEnumerator JumpWait()
    {
        yield return new WaitForSeconds(.1f);
        waiting = false;
    }

    void Move()
    {
        movement = new Vector3(moveX, 0, moveY);
        movement = Vector3.ClampMagnitude(movement, 1);
        movement = myCamera.transform.TransformDirection(movement);
        movement.y = 0;
        if (movement != Vector3.zero)
        {
            isMoving = true;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(movement), rotationSpeed * Time.deltaTime);
        }
        else
            isMoving = false;

        if(!isRolling)
        cc.Move(movement * speed * Time.deltaTime);
    }

    void Jumping()
    {
        cc.Move(new Vector3(0, jump, 0));
        jump -= gravity * Time.deltaTime;
    }

    public void Jump()
    {
        if (!Grounded()) return;

        attack.CancelBlock();
        CmdJumping(true);
        jump = jumpForce;
        isJumping = true;

        waiting = true;
        StartCoroutine(JumpWait());
    }

    bool Grounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, distToGround, groundLayer);
    }

    void CalculateCCSpeed()
    {
        current_pos = transform.position;
        ccVelocity = (current_pos - last_pos).magnitude / Time.deltaTime;
        last_pos = current_pos;
    }
}
