using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiObject : MonoBehaviour
{
    public MainUi ui;
    public GameObject volumeSettings;
    public GameObject settings;
    public GameObject brightnessSettings;
    public GameObject difficultySettings;
    public GameObject mainUi;
    public TextMeshProUGUI[] texts;
    public bool dofunc;
    public enum UiFunction
    {
        OPENVOLUME,
        OPENBRIGHTNESS,
        OPENDIFFICULTY,
        START,
        BACK,
        EDITDIFFICULTY,
        EDITBRIGHTNESS,
        EDITVOLUME,
        QUIT,

    }
    public UiFunction function;
    private void Update()
    {
        if(dofunc == true)
        {
            DoFunction();
            dofunc = false;
        }
    }
    
    public void DoFunction()
    {
        switch (function)
        {
            case UiFunction.START:

                ui.StartLoop();
                break;
            case UiFunction.EDITDIFFICULTY:

                if (slider != null)
                {
                    SendChallengeValue();
                }
                
                break;

            case UiFunction.QUIT:
                ui.QuitGame(this);

                break;

            case UiFunction.EDITBRIGHTNESS:
                if (slider != null)
                {
                    SendBrightness();
                }

                break;

            case UiFunction.BACK:
                ui.ToggleSettings();
                break;
            case UiFunction.EDITVOLUME:
                if (slider != null)
                {
                    SendSliderValue();
                }
                break;
            case UiFunction.OPENVOLUME:
                volumeSettings.SetActive(true);
                settings.SetActive(false);
                difficultySettings.SetActive(false);
                brightnessSettings.SetActive(false);
                mainUi.SetActive(false);
                break;
            case UiFunction.OPENDIFFICULTY:
                volumeSettings.SetActive(false);
                settings.SetActive(false);
                difficultySettings.SetActive(true);
                brightnessSettings.SetActive(false);
                mainUi.SetActive(false);
                break;
            case UiFunction.OPENBRIGHTNESS:
                volumeSettings.SetActive(false);
                settings.SetActive(false);
                difficultySettings.SetActive(false);
                brightnessSettings.SetActive(true);
                mainUi.SetActive(false);
                break;
        }
    }
    public float valueToSend;
    public Slider slider;
    void SendBrightness()
    {
        slider.value = valueToSend;
        ui.SetBrightness(valueToSend);
    }
    void SendChallengeValue()
    {
        slider.value = valueToSend;
        ui.SetDifficulty(slider.value);
    }
    public void SendSliderValue()
    {
        slider.value = valueToSend;
        ui.SetVolume(slider.value);
    }
}
