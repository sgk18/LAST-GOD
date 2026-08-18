using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 2f;
    public float gravity = 9.8f;

    private CharacterController characterController;
    private float moveX;
    private float yVel;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }
    void OnEnable()
    {
        
    }
    void Update()
    {
        if(characterController.isGrounded && yVel < 0)
        {
            yVel = -1;
        }

        yVel -= gravity * Time.deltaTime;

        Vector3 movement = new Vector3(moveX * moveSpeed, yVel, 0);

        characterController.Move(movement * Time.deltaTime);
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
}
