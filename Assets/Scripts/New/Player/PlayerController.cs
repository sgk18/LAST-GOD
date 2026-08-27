using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7;

    [Header("Jump N Dash")]
    public float jumpForce = 10;
    public float dashSpeed = 25;
    public float dashTime = 0.2f;
    public float dashCoolDown = 1;
    private float dashSide = 1;
    private bool isDashing;
    private bool canDash = true;
    private TrailRenderer dashTrail;

    [Header("Phy")]
    public float gravity = 30;
    private float g;
    private CharacterController characterController;
    private float yVel;

    [Header("input")]
    private float moveX;

    void Awake()
    {
        dashTrail = GetComponent<TrailRenderer>();
    }
    void Start()
    {
        g = gravity;
        characterController = GetComponent<CharacterController>();
    }
    void OnEnable()
    {
        GetComponent<PlayerInput>().actions.Enable();
    }
    void Update()
    {
        DashNMoveUpdate();
    }

    private void DashNMoveUpdate()
    {
        //gravity and y controls
        if(characterController.isGrounded && yVel < 0)
        {
            yVel = -1;
        }

        yVel -= g * Time.deltaTime;

        //turning
        if(moveX < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if(moveX > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        //move n dash
        Vector3 movement = new Vector3(moveX * moveSpeed, yVel, 0);

        if (isDashing)
        {
            characterController.Move(dashSpeed * transform.right * Time.deltaTime);
        }
        else
        {
            if(dashTrail.enabled) dashTrail.enabled = false;
            characterController.Move(movement * Time.deltaTime);
        }
    }



    //input
    void OnMove(InputValue val)
    {
        moveX = val.Get<Vector2>().x;
    }
    void OnJump()
    {
        if (characterController.isGrounded)
        {
            yVel = jumpForce;
        }
    }

    void OnDash()
    {
        Debug.Log("Dash key pressed");
        if (canDash)
        {
            Debug.Log("Can dash");
            StartCoroutine(Dash());
        }
        else
        {
            Debug.Log("Cant dash");
        }
    }

    void OnAttack()
    {
        Debug.Log("Attacked");
    }

    //dashing
    IEnumerator Dash()
    {
        //trailing
        dashTrail.Clear();
        dashTrail.enabled = true;

        //dashing
        isDashing = true;
        g = 0;
        StartCoroutine(DashCoolDown());
        yield return new WaitForSeconds(dashTime);
        g = gravity;
        isDashing = false;
    }
    IEnumerator DashCoolDown()
    {
        canDash = false;
        yield return new WaitForSeconds(dashCoolDown);
        canDash = true;
    }
}
