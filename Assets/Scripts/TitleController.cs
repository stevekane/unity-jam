﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour 
{
	[SerializeField] Text startText = null;
	[SerializeField] SpriteRenderer TitleText = null;
	[SerializeField] ParticleSystem BulletTrail = null;
	public Gradient TitleColor = null;
	private bool IsFlashing = false;
	public float textFlashTime = .25f;
	public float buildingFadeInTime = 2f;
	public AudioSource StartPressedSound= null;
	private Image Buildings = null;
	private float titleRunTime = 0f;

    public float strobeDuration = 2f;


	
	void Start() 
	{
		Buildings = gameObject.transform.Find("Buildings").GetComponent<Image>();
		Buildings.color = new Color(1,1,1,0);
	}
	
	// Update is called once per frame
	void Update() 
	{
		titleRunTime += Time.deltaTime;

		if (titleRunTime <= buildingFadeInTime)
		{
			Buildings.color = new Color(1,1,1, titleRunTime / buildingFadeInTime);
		}

		if (!IsFlashing)
		{
            StartCoroutine(FlashTextOff());       
        }

		float t = Mathf.PingPong(Time.time / strobeDuration, 1f);
        TitleText.color = TitleColor.Evaluate(t);


		//poll players controllers for start button
		var players = ReInput.players;
		for(int i = 0; i <= players.playerCount - 1 ; i++){
			var player = players.GetPlayer(i);
			if(player.controllers.joystickCount > 0){
				var startPressed = player.GetButtonDown("Start");
				if(startPressed){
					StartPressed();
				}
			}
		}
		
		//for mouse/keyboard
		var AnyKeyOrMouseClick = Input.anyKeyDown;
		if(AnyKeyOrMouseClick){
			StartPressed();
		}
	}

	void StartPressed(){
		StartPressedSound.Play();
		BulletTrail.Play();
		textFlashTime = .1f;
		StartCoroutine(GoToStageSelect());
	}

	IEnumerator GoToStageSelect(){
		yield return new WaitForSeconds(2f);
		SceneManager.LoadScene("StageSelect");
	}

    IEnumerator FlashTextOff()
	{
		IsFlashing = true;
        startText.enabled = true;
        yield return new WaitForSeconds(textFlashTime);
        StartCoroutine(FlashTextOn());

    }
     IEnumerator FlashTextOn()
	 {
        startText.enabled = false;
        yield return new WaitForSeconds(textFlashTime);
        IsFlashing = false;
     }
}