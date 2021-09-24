using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.SceneManagement;
using System;

public class SensitivityController : MonoBehaviour
{
    public static Action<int> SetXSensitivity;
    public static Action<int> SetYSensitivity;

    [SerializeField] Slider m_xSlider = null;
    [SerializeField] Slider m_ySlider = null;

    private void Start()
    {
        SceneManager.sceneLoaded += SetValue;

        SetXSliderValueAsSensitivity();
        SetYSliderValueAsSensitivity();
    }

    public void SetXSliderValueAsSensitivity() => SetXSensitivity?.Invoke((int)m_xSlider.value * 10);

    public void SetYSliderValueAsSensitivity() => SetYSensitivity?.Invoke((int)m_ySlider.value * 10);

    void SetValue(Scene next, LoadSceneMode mode)
    {
        var sc = FindObjectOfType<SensitivityController>();
        if (sc != null)
        {
            sc.m_xSlider.value = m_xSlider.value;
            sc.m_ySlider.value = m_ySlider.value;
        }
    }
}
