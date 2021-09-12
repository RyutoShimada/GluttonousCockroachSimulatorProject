using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class InputFieldController : MonoBehaviour
{
    Slider m_slider = null;
    InputField m_inputField = null;

    void Start()
    {
        if (TryGetComponent(out m_inputField) && transform.parent.TryGetComponent(out m_slider))
        {
            UpdateValue(m_slider);
        }
        else
        {
            Debug.LogError("コンポーネントの取得ができませんでした", this);
        }
    }

    public void UpdateValue(Slider slider) => m_inputField.text = slider.value.ToString();
}
