using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AbilitySystem))]
public class PlayerCharacter : MonoBehaviour
{
    // Constant
    const float jumpCheckPreventionTime = 0.5f;

    // Public
    [Header("Physics Settings")]
    public LayerMask groundLayerMask;

    [Header("Move & Jump Setting")]
    public float moveSpeed = 10;
    public float fallWeight = 5.0f;
    public float jumpWeight = 0.5f;
    public float jumpVelocity = 100.0f;

    // Internal Data

    // State of the player (jumping or not)
    protected bool jumping = false;         // state of player (jumping or not )
    protected bool falling = false;

    //
    protected Vector3 moveVec = Vector3.zero; // movement speed of player
    protected float jumpTimestamp;          // start jump timestamp

    protected Animator mAnimator;               // reference to the mAnimator
    protected Rigidbody mRigidBody;         // reference to the mRigidBody
    protected CapsuleCollider mCapsule;
    protected AbilitySystem mAbilitySystem;

    // Start is called before the first frame update
    private void Awake()
    {
        mAnimator = GetComponentInChildren<Animator>();
        mRigidBody = GetComponent<Rigidbody>();
        mCapsule = GetComponent<CapsuleCollider>();
        mAbilitySystem = GetComponent<AbilitySystem>();
        mAbilitySystem.InitializeAbilitySystem(gameObject, gameObject);

        GetComponent<PlayerInput>().onActionTriggered += HandleInputAction;
    }

    private void HandleInputAction(InputAction.CallbackContext context)
    {
        if (context.action.name == "Move")
        {
            Vector2 stickValue = context.ReadValue<Vector2>();
            OnMove(stickValue);
        }
    }

    void UpdateWhenJumping()
    {
        bool isFalling = mRigidBody.velocity.y <= 0;

        float weight = isFalling ? fallWeight : jumpWeight;

        Vector3 localMovementVector = Fighter.GameplayStatics.ProjectInputVectorToCamera(Camera.main, transform, moveVec);

        // Assign new velocity
        mRigidBody.velocity = new Vector3(localMovementVector.x * moveSpeed, mRigidBody.velocity.y, localMovementVector.z * moveSpeed);
        mRigidBody.velocity += Vector3.up * Physics.gravity.y * weight * Time.deltaTime;

        if (isFalling && jumping)
        {
            jumping = false;
            falling = true;
        }

        GroundCheck();
    }

    void UpdateWhenGrounded()
    {
        Vector3 localMovementVector = Fighter.GameplayStatics.ProjectInputVectorToCamera(Camera.main, transform, moveVec);

        mRigidBody.velocity = localMovementVector * moveSpeed;

        if (moveVec.x != 0)
        {
            Vector3 lookAtDirection = new Vector3(moveVec.x, 0, 0);
            transform.LookAt(this.transform.position + lookAtDirection.normalized);
        }

        CheckShouldFall();
    }

    private void FixedUpdate()
    {
        if (!IsJumpingOrFalling())
        {
            UpdateWhenGrounded();
        }
        else
        {
            UpdateWhenJumping();
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();
    }

    #region Jump & Fall & Ground Logic

    public bool HandleJump()
    {
        if (jumping)
        {
            return false;
        }

        jumping = true;
        falling = false;
        jumpTimestamp = Time.time;
        mRigidBody.velocity = new Vector3(0, jumpVelocity, 0); // Set initial jump velocity

        return true;
    }


    void CheckShouldFall()
    {
        if (IsJumpingOrFalling())
        {
            return; // No need to check if in the air
        }

        bool hasHit = TraceForGroundUnderneath();

        if (hasHit == false)
        {
            jumping = false;
            falling = true;
        }
    }


    void GroundCheck()
    {
        if (!IsJumpingOrFalling())
        {
            return; // No need to check
        }


        if (Time.time < jumpTimestamp + jumpCheckPreventionTime)
        {
            return;
        }

        bool hasHit = TraceForGroundUnderneath();

        if (hasHit)
        {
            jumping = false;
            falling = false;
        }
    }

    bool TraceForGroundUnderneath()
    {
        const float traceRadius = 0.1f;
        float capsuleHalfHeight = Math.Max(mCapsule.radius, mCapsule.height * 0.5f);
        Vector3 traceStart = transform.position;
        traceStart.y -= capsuleHalfHeight + traceRadius;
        Vector3 traceEnd = new Vector3(traceStart.x, traceStart.y - traceRadius, traceStart.z);

        Debug.DrawLine(traceStart, traceEnd, Color.red, 0);
        return Physics.CheckSphere(traceStart, traceRadius, groundLayerMask);
    }

    public bool IsJumpingOrFalling()
    {
        return falling || jumping;
    }

    #endregion


    void UpdateAnimation()
    {
        if (mAnimator == null)
        {
            return;
        }

        mAnimator.SetBool("isJumping", jumping);
        mAnimator.SetBool("isFalling", falling);
        mAnimator.SetFloat("moveSpeed", moveVec.magnitude);
        mAnimator.SetFloat("verticalSpeed", mRigidBody.velocity.y);
    }

    public void OnMove(Vector2 inputVector)
    {
        moveVec = new Vector3(inputVector.x, 0, inputVector.y);
    }
}
