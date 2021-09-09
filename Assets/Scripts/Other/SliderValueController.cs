using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderValueController : MonoBehaviour
{
    Slider m_slider = null;

    void Start()
    {
        m_slider = GetComponent<Slider>();
    }

    public void InputField(InputField input)
    {
        if (float.TryParse(input.text, out float i))
        {
            m_slider.value = i;
        }   
    }
}
