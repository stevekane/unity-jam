﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deagle : AbstractWeapon
{
    [Header("Cached references")]
    [SerializeField]    GameObject muzzleFlash = null;
    [SerializeField]    Light muzzleFlashLight = null;
    [Header("State")]
    float timeTillNextShot = 0f;
	private GameObject FlashInstance;

    void Awake(){
        MagazineSize = 7;
        AmmoCount = MagazineSize;   
        FlashInstance = Instantiate(muzzleFlash, transform);     
        WeaponName = "D. Eagle";
        LeftHandIKTarget = IKTarget_L;
        RightHandIKTarget = IKTarget_R;
    }

    
    void LateUpdate()
    {
        timeTillNextShot -= Time.deltaTime;   
    }

    public override void PullTrigger(Player player)
    {
        base.PullTrigger(player);
        if (timeTillNextShot > 0 || isReloading || AmmoCount == 0) return;

        AmmoCount -= 1;        
        var muzzle = transform.position + transform.forward * muzzleOffset;

        StartCoroutine(PostShotCleanup());
        timeTillNextShot = fireRate;
        FlashInstance.transform.position = Muzzle.transform.position;
		FlashInstance.GetComponentInChildren<ParticleSystem>().Stop();
        FlashInstance.GetComponentInChildren<ParticleSystem>().Play();
        muzzleFlashLight.enabled = true;
        fireSound.Play();
        if(!aimAssistOn)
            CheckForValidHitscan(muzzle, transform.forward, layerMask);
        else {
            var tArea = GetComponentInChildren<WeaponTargettingArea>();
            CheckForValidTargetInRange(tArea, transform.forward, layerMask);
        }
        if (AmmoCount == 0)
            Reload();
    }

    IEnumerator PostShotCleanup()
    {
        yield return new WaitForSeconds(shotTime);
        bulletTracer.enabled = false;
        muzzleFlashLight.enabled = false;
    }
}