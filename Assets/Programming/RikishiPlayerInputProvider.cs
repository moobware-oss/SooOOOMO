﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(RikishiController))]
public class RikishiPlayerInputProvider : MonoBehaviour
{
    public float secondsTilInflation = 1f;
    public float rateOfInflation = 0.1f;

    public float doubleTapDelayTime = 0.25f;
    //private KeyCode lastKeyPressed;
    //private float lastKeyPressTime;

    public float DebugPlayerAimRayHeight = 0.75f;
    public float DebugPlayerAimRayLength = 1f;
    Transform mainCameraTransform;
    Transform playerTransform;

    RikishiController rikishiController;
    ParameterizedFlabbiness zeFlabben;
    Scoreboard scoreboard;

    Dictionary<KeyCode, float> LastTimeKeyTapped = new Dictionary<KeyCode, float> {
        { KeyCode.D, -1000f}
    };

    private bool shouldDodgeRight;

    void Start()
    {
        //mainCameraTransform = Camera.main.transform;
        playerTransform = GameObject.FindWithTag("Player").transform;
        rikishiController = GetComponent<RikishiController>();
        zeFlabben = GetComponent<ParameterizedFlabbiness>();
        scoreboard = FindObjectOfType<Scoreboard>();
        //zeFlabben.Flabbiness = scoreboard.GetPlayerScore() * 34f;
        //rikishiController.shoveForce = zeFlabben.Flabbiness * 3;

        StartCoroutine(DoTheFlab());

    }

    //void FixedUpdate()
    //{
    //    //var fromCameraToPlayer = mainCameraTransform.transform.position - playerTransform.position; // this drew a line from the camera to the player
    //    var fromCameraToPlayerOpposite = playerTransform.position - mainCameraTransform.transform.position; // this drew a line from the player in the opposite direction of the camera :D which is what I want!
    //    Debug.DrawRay(playerTransform.position, fromCameraToPlayerOpposite, Color.blue);

    //    var playerDesiredRotation = Vector3.ProjectOnPlane(fromCameraToPlayerOpposite, Vector3.up); // line parallel to ground instead of at an angle
    //    var angleOfDesiredRotation = Vector3.SignedAngle(playerDesiredRotation, playerTransform.forward, Vector3.up) * -1;
    //    var debugPlayerAimRay = new Vector3(playerTransform.forward.x, playerTransform.forward.y, playerTransform.forward.z) * DebugPlayerAimRayLength;
    //    debugPlayerAimRay = Quaternion.AngleAxis(angleOfDesiredRotation, Vector3.up) * debugPlayerAimRay;
    //    Debug.DrawRay(playerTransform.position, debugPlayerAimRay, Color.green);

    //    rikishiController.SetDesiredAimTarget(debugPlayerAimRay + playerTransform.position);

    //    float h = CrossPlatformInputManager.GetAxis("Horizontal");
    //    float v = CrossPlatformInputManager.GetAxis("Vertical");
    //    var movementVector = v * playerTransform.forward + h * playerTransform.right;
    //    rikishiController.Move(movementVector);

    //    if (shouldDodgeRight)
    //    {
    //        shouldDodgeRight = false;
    //        rikishiController.DodgeRight();
    //    }

    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        rikishiController.AttemptShove();
    //    }
    //}

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.D))
        {
            if (PlayerDoubleTapped(KeyCode.D))
            {
                shouldDodgeRight = true;
            }
            LastTimeKeyTapped[KeyCode.D] = Time.time;
        }
    }

    private bool PlayerDoubleTapped(KeyCode key)
    {
        return (Time.time - LastTimeKeyTapped[key]) < doubleTapDelayTime;
    }

    IEnumerator DoTheFlab()
    {
        yield return new WaitForSeconds(secondsTilInflation);
        while (zeFlabben.Flabbiness < 100) {
            zeFlabben.Flabbiness += rateOfInflation * Time.deltaTime;
            yield return null;
        }
    }
}