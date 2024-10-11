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
                text.text = "Are You Sure?";
                alreadyPressed = true;
            }
        }
        else
        {
            Application.Quit();
        }
        
        Invoke("CycleQuit", 3f);
        
        
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
    public void StartLoop()
    {
        if(started == false)
        {
            spawn.Initiate();
        }
        started = true;
    }

    void CycleQuit(UiObject obj)
    {
        foreach (TextMeshProUGUI text in obj.texts)
        {
            text.text = "Are You Sure";
        }
        alreadyPressed = false;
    }
}
