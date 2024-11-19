using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class KillPlayer : MonoBehaviour
{
    public BNGPlayerController playerController;
    public CharacterController characterController;
    public LocomotionManager locomotionManager;
    public SmoothLocomotion smoothLocomotion;
    public PlayerRotation playerRot;
    public PlayerGravity playerGravity;
    public Animator anim;
    public Image fadeImage;
    public Image fadeImage2;
    bool canFade;
    Color a;
    public AudioSource source;

    private void Start()
    {

    }
    public void HandlePlayer()
    {
        canFade = true;
        playerController.enabled = false;
        characterController.enabled = false;
        locomotionManager.enabled = false;
        smoothLocomotion.enabled = false;
        playerRot.enabled = false;
        playerGravity.enabled = false;
        fadeImage.gameObject.SetActive(true);
        source.Play();
        Invoke("Die", 3f);

    }
    private void Update()
    {
        if (canFade)
        {
            a.a += Time.deltaTime / 2;
            fadeImage2.color = a;
        }
    }

    public void Die()
    {
        SceneManager.LoadScene(0);
    }
}
