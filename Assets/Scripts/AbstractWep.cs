﻿using UnityEngine;

public abstract class TestAbstractWep : MonoBehaviour 
{
	public Transform Muzzle;
	public Player player;

	public string WeaponName = "gun";
	public float SpeedModifier = 1f;
	public int AmmoCount = 0;
	public int MagazineSize = 0;
	[SerializeField] public int DamageAmount = 1;
	public Transform LeftHandIKTarget;
	public Transform RightHandIKTarget;
	[SerializeField] public LineRenderer bulletTracer;
	[SerializeField]  public GameObject BulletHole;
	[SerializeField] public LayerMask layerMask = new LayerMask();

	//Event Broadcasting
	public delegate void ValidHitOccurredEvent(ValidHit NewHit);
	public event ValidHitOccurredEvent OnValidHitOccurred;
	
	public virtual void PullTrigger(Player player){}
	public virtual void ReleaseTrigger(Player player){}
	public virtual void CheckForValidHitscan(Vector3 muzzle, Vector3 weaponDir, LayerMask layerMask){
		Ray ray = new Ray();
		RaycastHit rayHit = new RaycastHit();
		ray.origin = muzzle;
        ray.direction = transform.forward;

        var didHit = Physics.Raycast(ray, out rayHit, Mathf.Infinity, layerMask);
		        
        bulletTracer.SetPosition(0, muzzle);
        bulletTracer.SetPosition(1, rayHit.point);
        bulletTracer.enabled = true;

        if (!didHit)
            return;

        var isPlayer = rayHit.collider.CompareTag("PlayerHitbox");
		var isNPC = rayHit.collider.CompareTag("NPCHitbox");
		ValidHit NewHit = new ValidHit();

		//Can prob refactor if player / npc have common baseclass
        if (isPlayer || isNPC)
        {
			AbstractCharacter target;
			if (isNPC)
				target = rayHit.collider.GetComponent<BossMonster>();
			else
				target = rayHit.collider.GetComponentInParent<PlayerHitbox>().player;
			
			NewHit.OriginatingEntityType = player.ENTITY_TYPE;
			NewHit.OriginatingEntityIdentifier = player.ID;
			NewHit.VictimEntityType = target.ENTITY_TYPE;
			NewHit.VictimEntity = target; 
			NewHit.DamageAmount = DamageAmount;
			
        }
		// if not player or npc then it hit terrain
		else {
			// if bullethole graphic create one at the impact point
			if(BulletHole != null){
				GameObject bulletHole = Instantiate(BulletHole, rayHit.point, Quaternion.FromToRotation(Vector3.up, rayHit.normal));
            	var particleSystems = bulletHole.GetComponentsInChildren<ParticleSystem>();
            	Destroy(bulletHole, 3f);
			}
		}
		if(OnValidHitOccurred != null) OnValidHitOccurred(NewHit);

	}
}