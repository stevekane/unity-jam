﻿using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class Player : MonoBehaviour 
{
    public enum Status { Alive, Dying, Dead, Invincible}
    // N.B. This must be applied to the character every frame they are grounded to keep them grounded
    const float GROUNDED_DOWNWARD_VELOCITY = -10f;

    [SerializeField]
    Balloon BalloonPrefab;
    [SerializeField]
    PlayerIndicator PlayerIndicatorPrefab;

    PlayerIndicator playerIndicator;
    public Rigidbody head;
    public SkinnedMeshRenderer meshRenderer;
    public CharacterController controller;
    public AbstractWeapon Weapon;
    public Animator animator;
    public Color color = Color.red;

    public float MoveSpeed = 2f;
    public float JumpStrength = 2f;
    public float JumpPadStrength = 3f;
    public float CrouchMovementModifier = 0.5f;

    public string HorizontalInput = "";
    public string VerticalInput = "";
    public string FireInput = "";
    public string JumpInput = "";
    public float MaxHorizontalAimAngle = 60;
    public float MaxVerticalAimAngle = 60;
    public bool InvertAimVertical = false;
    public int PlayerNumber = 0;
    public int Health = 1;
    public bool canMove = true;
    public bool canRotate = true;
    public float aerialHeight = 0f;
    public float VerticalVelocity = 0f;
    public bool isGrounded = true;
    public bool IsDead = false;
    public int lastAttackerIndex;

    //reinput
    private Rewired.Player player;
    private float mouseVertDelta = 0f;
    private float mouseHorzDelta = 0f;
    //state
    float standingHeight;
    Vector3 standingCenter;
    List<Balloon> balloons = new List<Balloon>();

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 10f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.down * 10f);
        Gizmos.color = controller.isGrounded ? Color.green : Color.gray;
        Gizmos.DrawCube(transform.position + transform.up * 3f, Vector3.one * .2f);
    }

    void Start()
    {
        // Get the Rewired Player object for this player and keep it for the duration of the character's lifetime
        player = ReInput.players.GetPlayer(PlayerNumber);
        playerIndicator = Instantiate(PlayerIndicatorPrefab, transform);
        standingHeight = controller.height;
        standingCenter = controller.center;

    }

    void Update() 
    {
        var ray = new Ray(transform.position, Vector3.down);
        var rayHit = new RaycastHit();
        var jumpDown = player.GetButtonDown("Jump");
        var fireDown = player.GetButtonDown("Fire");
        var fireUp = player.GetButtonUp("Fire");
        var fireHold = player.GetButtonTimedPress("Fire", .01f);
        var crouch = player.GetButtonTimedPress("Crouch", .01f);
        var horizontalAxis = player.GetAxis(0);
        var verticalAxis = player.GetAxis(1);
        var didHit = Physics.Raycast(ray, out rayHit, 1000f);
        var input = new Vector3(horizontalAxis, 0, verticalAxis);
        var moveDelta = Vector3.zero;
        var totalMovementModifier = 1f;
        var aimHorizontal = player.GetAxis(5);
        var aimVertical = player.GetAxis(6);
        var mouseFire = player.GetButtonTimedPress("MouseFire", .01f);
        var mouseFireUp = player.GetButtonUp("mouseFire");
        var mouseHorizontal = player.GetAxis("MouseHorizontal");
        var mouseVertical = player.GetAxis("MouseVertical");
        

        if(!mouseFire){
            if (aimVertical != 0.0f || aimHorizontal != 0.0f) {

                //Atan2 gives values of -45 to 45
                var VerticalAngle = (InvertAimVertical == false ? -1 : 1 ) * Mathf.Atan2(aimVertical, 1) * Mathf.Rad2Deg * MaxVerticalAimAngle / 45;
                var HorizontalAngle = Mathf.Atan2(aimHorizontal, 1) * Mathf.Rad2Deg * MaxHorizontalAimAngle / 45;       
        
                Weapon.transform.localRotation = Quaternion.Euler(VerticalAngle, HorizontalAngle,0f);

            }
            // return to centered position
            else if (aimHorizontal < .01f && aimVertical < .01f){

                //Weapon.transform.forward = transform.forward;
            }
        } else if(mouseFire){

            
            if(player.id == 0 && (mouseHorizontal != 0 || mouseVertical != 0)){

                var fwd = Weapon.transform.localRotation;
                mouseHorzDelta += mouseHorizontal;
                mouseVertDelta += mouseVertical;


                var newVert = Mathf.Clamp(fwd.x +  mouseVertDelta,-MaxVerticalAimAngle, MaxVerticalAimAngle);
                var newHorz = Mathf.Clamp(fwd.y + mouseHorzDelta, -MaxHorizontalAimAngle, MaxHorizontalAimAngle);
                
                Weapon.transform.localRotation = Quaternion.Euler(-newVert, newHorz, 0f);
            }
        }
        
        if(mouseFireUp){
            Weapon.transform.forward = transform.forward;
            mouseHorzDelta = 0f;
            mouseVertDelta = 0f;
        }

        isGrounded = controller.isGrounded;
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
                var strength = didHit && rayHit.collider.CompareTag("JumpPad") ? JumpPadStrength : JumpStrength;

                VerticalVelocity = strength;
            }
            else
            {
                VerticalVelocity = GROUNDED_DOWNWARD_VELOCITY;
                if (crouch == true)
                {
                    totalMovementModifier *= CrouchMovementModifier;
                    controller.height = standingHeight / 2;
                    controller.center = new Vector3(standingCenter.x, standingCenter.y / 2, standingCenter.z);
                }
                else {
                    controller.height = standingHeight;
                    controller.center = standingCenter;
                }
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
            moveDelta.x += horizontalAxis * Time.deltaTime * MoveSpeed * totalMovementModifier;
            moveDelta.y += VerticalVelocity * Time.deltaTime;
            moveDelta.z += verticalAxis * Time.deltaTime * MoveSpeed * totalMovementModifier;
        }

        // Weapon inputs
        if (Weapon != null && (fireDown || fireHold || mouseFire))
        {
            Weapon.PullTrigger(this);
        }
        if (Weapon != null && fireUp || mouseFireUp)
        {
            Weapon.ReleaseTrigger(this);
        }
        
        controller.Move(moveDelta);
        
        //Animation stuff
        if(animator != null)
        {
            if (MoveSpeed != 0f)
            {
                float move = 0f;
                Vector3 twodmove = new Vector3(moveDelta.x, 0, moveDelta.z);

                if (Mathf.Abs(horizontalAxis) > 0 || Mathf.Abs(verticalAxis) > 0)
                {
                    move = Vector3.Magnitude(twodmove) * (MoveSpeed / Time.deltaTime);
                }
                animator.SetFloat("Forward", move);
                animator.SetFloat("Jump", VerticalVelocity + (GROUNDED_DOWNWARD_VELOCITY * -1));
            }

            animator.SetBool("OnGround", isGrounded);
            animator.SetBool("Crouch", crouch);
            if (Health <= 0) 
            {
                animator.SetBool("PlayDeathAnimation", true);
                canMove = false;
                canRotate = false;
            }
        }

        playerIndicator.transform.position = didHit ? rayHit.point : transform.position;
        playerIndicator.meshRenderer.material.color = color;
        
        meshRenderer.material.color = color;      
        // TODO: pretty overkill to do this every frame....
        for (var i = 0; i < balloons.Count; i++)
        {
            balloons[i].meshRenderer.material.color = color;
        }
    }

    public void DeathAnimationFinished() 
    {
        animator.SetBool("PlayDeathAnimation", false);
        IsDead = true;
        canMove = true;
        canRotate = true;
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

    public void Damage(int amountOfDamage, int attackerIndex)
    {
        if (Health <= 0)
            return;

        Health -= amountOfDamage;
        lastAttackerIndex = attackerIndex;

        for (var i = Health; i < balloons.Count; i++)
        {
            balloons[i].Cut();
        }
        balloons.RemoveRange(Health, balloons.Count - Health);
        animator.SetTrigger("Hit");
    }

	public void Respawn(Vector3 position, Quaternion rotation)
	{
		transform.SetPositionAndRotation(position, rotation);
		Health = 3;
        IsDead = false;
		canMove = true;
		canRotate = true;
		VerticalVelocity = 0f;

        // Destroy any remaining balloons
        foreach(var balloon in balloons)
        {
            Destroy(balloon.gameObject);
        }
        balloons.Clear();

        // spawn new balloons
        for (var i = 0; i < Health; i++)
        {
            var balloon = Instantiate(BalloonPrefab);

            balloon.springJoint.connectedBody = head;
            balloons.Add(balloon);
        }
	}
}