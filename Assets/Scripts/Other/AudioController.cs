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

    public Slider m_MasterSlider = null;
    public Slider m_BGMSlider = null;
    public Slider m_SESlider = null;

    void Start()
    {
        UpdateMasterVolume();
        UpdateBGMVolume();
        UpdateSEVolume();

        SceneManager.sceneLoaded += SetValue;
    }

    public void UpdateMasterVolume()
    {
        m_Master.audioMixer.SetFloat(m_Master.name, ChangeDB(m_MasterSlider.value));
    }

    public void UpdateBGMVolume()
    {
        m_BGM.audioMixer.SetFloat(m_BGM.name, ChangeDB(m_BGMSlider.value));
    }

    public void UpdateSEVolume()
    {
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
        var audioController = FindObjectOfType<AudioController>();
        if (audioController != null)
        {
            audioController.m_MasterSlider.value = m_MasterSlider.value;
            audioController.m_BGMSlider.value = m_BGMSlider.value;
            audioController.m_SESlider.value = m_SESlider.value;
        }
    }
}
