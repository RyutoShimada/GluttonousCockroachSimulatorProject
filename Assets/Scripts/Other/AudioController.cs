using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    [SerializeField] AudioMixerGroup m_Master = null;
    [SerializeField] AudioMixerGroup m_BGM = null;
    [SerializeField] AudioMixerGroup m_SE = null;

    [SerializeField] Slider m_MasterSlider = null;
    [SerializeField] Slider m_BGMSlider = null;
    [SerializeField] Slider m_SESlider = null;

    public static float m_masterValue = 0;
    public static float m_bgmValue = 0;
    public static float m_seValue = 0;

    void Start()
    {
        UpdateMasterVolume();
        UpdateBGMVolume();
        UpdateSEVolume();

        m_MasterSlider.value = m_masterValue;
        m_BGMSlider.value = m_bgmValue;
        m_SESlider.value = m_seValue;

        SceneManager.sceneLoaded += SetValue;
        SceneManager.sceneUnloaded += GetValue;
    }

    public void UpdateMasterVolume()
    {
        m_masterValue = m_MasterSlider.value;
        m_Master.audioMixer.SetFloat(m_Master.name, ChangeDB(m_MasterSlider.value));
    }

    public void UpdateBGMVolume()
    {
        m_bgmValue = m_BGMSlider.value;
        m_BGM.audioMixer.SetFloat(m_BGM.name, ChangeDB(m_BGMSlider.value));
    }

    public void UpdateSEVolume()
    {
        m_seValue = m_SESlider.value;
        m_SE.audioMixer.SetFloat(m_SE.name, ChangeDB(m_SESlider.value));
    }

    float ChangeDB(float value)
    {
        // valueは   0 ～ 100
        // dBは    -80 ～ 20
        float dB = value - 80;
        return dB;
    }

    void SetValue(Scene next, LoadSceneMode mode)
    {
        if (m_MasterSlider.value == m_masterValue) return;
        if (m_BGMSlider.value == m_bgmValue) return;
        if (m_SESlider.value == m_seValue) return;

        m_MasterSlider.value = m_masterValue;
        m_BGMSlider.value = m_bgmValue;
        m_SESlider.value = m_seValue;
    }

    void GetValue(Scene next)
    {
        if (m_MasterSlider.value == m_masterValue) return;
        if (m_BGMSlider.value == m_bgmValue) return;
        if (m_SESlider.value == m_seValue) return;

        UpdateMasterVolume();
        UpdateBGMVolume();
        UpdateSEVolume();
    }
}
