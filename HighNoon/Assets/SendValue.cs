using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SendValue : MonoBehaviour
{
    public float valueToSend;
    public Slider slider;
    public MainUi uiObject;



    public void SendSliderValue()
    {
        slider.value = valueToSend;
        uiObject.SetVolume(slider.value);
    }
}
