using BNG;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class MainUi : MonoBehaviour
{
    bool alreadyPressed;
    public SpawnEnemy spawn;
    public GameObject mainUi;
    public GameObject settingsUi;
    public GameObject difficultyUi;
    public GameObject volumeSettings;
    public GameObject brightness;
    public void QuitGame(UiObject obj)
    {
        if(alreadyPressed == false)
        {
            foreach (TextMeshProUGUI text in obj.texts)
            {
                text.text = "YOU SURE?";
                alreadyPressed = true;
            }
        }
        else
        {
            Application.Quit();
        }

        StartCoroutine(nameof(CycleQuit),obj);
        
        
    }

    public void SetBrightness(float brightness)
    {
        print("Brightness = " + brightness);
    }
    public void SetDifficulty(float difficulty)
    {
        spawn.difficulty = difficulty;
    }
    public AudioMixer mixer;
    public void SetVolume(float value)
    {

        mixer.SetFloat("Master", Mathf.Log10(value) * 20);

    }
    public void StartGame()
    {
        print("Starting!");
    }


    bool settings;
    public void ToggleSettings()
    {
        settings = !settings;
        if (settings == false)
        {
            mainUi.SetActive(true);
            settingsUi.SetActive(false);
            volumeSettings.SetActive(false);
            difficultyUi.SetActive(false);
            brightness.SetActive(false);

        }
        else
        {
            mainUi.SetActive(false);
            settingsUi.SetActive(true);
            volumeSettings.SetActive(false);
            difficultyUi.SetActive(false);
            brightness.SetActive(false);
        }
    }
    bool started;
    public void StartLoop(UiObject obj)
    {
        if(started == false)
        {
            spawn.Initiate();
            foreach (TextMeshProUGUI text in obj.texts)
            {
                text.text = "CLOSE BAR";
                print("Opened Bar");
            }
        }
        else
        {
            foreach (TextMeshProUGUI text in obj.texts)
            {
                text.text = "OPEN BAR";
                print("Closed Bar");
            }
            spawn.UnInitiate();
            
        }
        started = !started;
    }

    IEnumerator CycleQuit(UiObject obj)
    {
        yield return new WaitForSeconds(3);
        foreach (TextMeshProUGUI text in obj.texts)
        {
            text.text = "QUIT GAME";
        }
        alreadyPressed = false;
    }
}
