﻿using UnityEngine;

public class Player : MonoBehaviour 
{
    // N.B. This must be applied to the character every frame they are grounded to keep them grounded
    const float GROUNDED_DOWNWARD_VELOCITY = -10f;

    [SerializeField]
    Collider groundCollider;

    public CharacterController controller;
    public AbstractWeapon Weapon;

    public float MoveSpeed = 2f;
    public float JumpStrength = 2f;
    public LayerMask layerMask;
    string HorizontalInput = "";
    string VerticalInput = "";
    string FireInput = "";
    string JumpInput = "";

    public int PlayerNumber = 0;
    public int Health = 1;
    public bool canMove = true;
    public bool canRotate = true;
    public bool isGrounded = false;
    public float aerialHeight = 0f;
    public float VerticalVelocity = 0f;

    [Header("Development options")]
    public bool useCustomGrounding = false;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 10f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.down * 10f);
        if (useCustomGrounding)
        {
            Gizmos.color = isGrounded ? Color.green : Color.gray;
            Gizmos.DrawCube(groundCollider.bounds.center, groundCollider.bounds.extents * 2f);
        }
        else
        {
            Gizmos.color = isGrounded ? Color.green : Color.gray;
            Gizmos.DrawCube(transform.position + transform.up, Vector3.one);
        }
    }

    void Start()
    {
        HorizontalInput = "Horizontal_" + PlayerNumber;
        VerticalInput = "Vertical_" + PlayerNumber;
        FireInput = "Fire_" + PlayerNumber;
        JumpInput = "Jump_" + PlayerNumber;
    }

    void Update() 
    {
        var ray = new Ray(transform.position, Vector3.down);
        var rayHit = new RaycastHit();
        var jumpDown = Input.GetButtonDown(JumpInput);
        var fireDown = Input.GetButton(FireInput);
        var fireUp = Input.GetButtonUp(FireInput);
        var horizontalAxis = Input.GetAxis(HorizontalInput);
        var verticalAxis = Input.GetAxis(VerticalInput);
        var didHit = Physics.Raycast(ray, out rayHit, 1000f);
        var contacts = Physics.OverlapBox(groundCollider.bounds.center, groundCollider.bounds.extents, groundCollider.transform.rotation, layerMask);
        var input = new Vector3(horizontalAxis, 0, verticalAxis);
        var moveDelta = Vector3.zero;

        if (useCustomGrounding)
        {
            isGrounded = contacts.Length > 0;
        }
        else
        {
            isGrounded = controller.isGrounded;
        }
        aerialHeight = didHit ? rayHit.distance : 0f;

        // look in direction
        if (canRotate && (horizontalAxis != 0 || verticalAxis != 0))
        {
            transform.forward = input.normalized;
        }

        // Check/Jump
        if (isGrounded)
        {
            if (jumpDown)
            {
                VerticalVelocity = JumpStrength;
            }
            else
            {
                VerticalVelocity = GROUNDED_DOWNWARD_VELOCITY;
            }
        }
        else
        {
            if (canMove)
            {
                VerticalVelocity += Physics.gravity.y * Time.deltaTime;
            }
            else
            {
                VerticalVelocity = 0f;
            }
        }

        // move if not rooted
        if (canMove)
        {
            moveDelta.x += horizontalAxis * MoveSpeed;
            moveDelta.y += VerticalVelocity * Time.deltaTime;
            moveDelta.z += verticalAxis * MoveSpeed;
        }

        // Weapon inputs
        if (Weapon != null && fireDown)
        {
            Weapon.PullTrigger(this);
        }
        if (Weapon != null && fireUp)
        {
            Weapon.ReleaseTrigger(this);
        }

        controller.Move(moveDelta);
    }

    // TODO: Call some kind of reset on the weapon to clear modifiers to the player?
    public void SetWeapon(AbstractWeapon newWeapon)
    {
        var oldWeapon = Weapon;

        Weapon = Instantiate(newWeapon, transform);
        Weapon.player = this;

        if (oldWeapon != null)
        {
            oldWeapon.player = null;
            Destroy(oldWeapon.gameObject);
        }
    }
}