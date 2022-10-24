using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviour
{
    public PhotonView pv;
    public PlayerController playerController;
    public CharacterController controller;
    public float moveSpeed;
    [HideInInspector] public float defaultSpeed;
    public float jumpHeight;
    public float gravity;

    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool walking = false;
    public bool freeze = false;
    private Vector3 velocity;
    [SerializeField] private Vector3 groundCheckPosition;
    [SerializeField] private float groundCheckRadius;

    public string jumpClipKey;
    public float jumpSoundRange;

    public LayerMask groundCheckMask;

    private void Start()
    {
        if (pv.IsMine)
        {
            defaultSpeed = moveSpeed;
            OnMatchStateUpdated(BombAndDefuse.instance.state);
        }
    }

    private void OnEnable()
    {
        if (pv.IsMine)
        {
            BombAndDefuse.instance.OnMatchStateUpdated += OnMatchStateUpdated;
        }
    }

    private void OnDisable()
    {
        if (pv.IsMine)
        {
            BombAndDefuse.instance.OnMatchStateUpdated -= OnMatchStateUpdated;
        }
    }

    private void OnMatchStateUpdated(MatchState newState)
    {
        if(newState == MatchState.WarmupEnding || newState == MatchState.PreRound || newState == MatchState.MatchEnding)
        {
            freeze = true;
        }
        else
        {
            freeze = false;
        }
    }

    private void Update()
    {
        if (playerController.IsOwner)
        {
            walking = Input.GetKey(KeyCode.LeftShift);

            isGrounded = false;
            var objs = Physics.OverlapSphere(transform.position + groundCheckPosition, groundCheckRadius, groundCheckMask);
            foreach(var obj in objs)
            {
                if(obj.gameObject == gameObject)
                {
                    continue;
                }
                isGrounded = true;
            }

            if(isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            if (PlayerController.AllowMovementInput())
            {
                var horizontalInput = Input.GetAxisRaw("Horizontal") * Time.deltaTime;
                var verticalInput = Input.GetAxisRaw("Vertical") * Time.deltaTime;
                if (freeze)
                {
                    horizontalInput = 0;
                    verticalInput = 0;
                }

                if(isGrounded == false)
                {
                    horizontalInput = horizontalInput * 0.3f;
                    verticalInput = verticalInput * 0.3f;
                }

                Vector3 moveDirection = (transform.forward * verticalInput + transform.right * horizontalInput).normalized;

                float speed = walking ? moveSpeed / 2 : moveSpeed;
                controller.Move(moveDirection * (speed * Time.deltaTime));
                

                if (Input.GetButtonDown("Jump"))
                {
                    Jump();
                }
            }
        }
    }

    private void Jump()
    {
        if (isGrounded && !freeze)
        {
            velocity.y = Mathf.Sqrt(-2f * jumpHeight * gravity);
            PlayerAudioManager.instance.PlaySpatialSoundOnPlayer(jumpClipKey, jumpSoundRange);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position + groundCheckPosition, groundCheckRadius);
    }
}
