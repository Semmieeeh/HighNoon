using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    public BNGPlayerController playerController;
    public CharacterController characterController;
    public LocomotionManager locomotionManager;
    public SmoothLocomotion smoothLocomotion;
    public PlayerRotation playerRot;
    public PlayerGravity playerGravity;
    public Animator anim;

    private void Start()
    {
        
    }
    public void HandlePlayer()
    {
        playerController.enabled = false;
        characterController.enabled = false;
        locomotionManager.enabled = false;
        smoothLocomotion.enabled = false;
        playerRot.enabled = false;
        playerGravity.enabled = false;
        anim.SetTrigger("Dead");
    }
}
