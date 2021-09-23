using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class SensitivityController : MonoBehaviour
{
    [SerializeField] Slider m_xSlider = null;
    [SerializeField] Slider m_ySlider = null;

    [Tooltip("ニンゲン側の感度")]
    [HideInInspector] public CinemachineVirtualCameraBase m_vcam = null;
    [Tooltip("ゴキブリ側の感度")]
    [HideInInspector] public PlayerInput playerInput = null;

    public void SetSliderValueAsSensitivity()
    {
        
    }
}
