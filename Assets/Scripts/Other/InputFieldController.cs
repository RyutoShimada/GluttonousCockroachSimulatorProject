using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class InputFieldController : MonoBehaviour
{
    InputField m_inputField = null;
    
    void Start()
    {
        m_inputField = GetComponent<InputField>();    
    }

    public void UpdateValue(Slider slider)
    {
        m_inputField.text = slider.value.ToString();
    }
}
