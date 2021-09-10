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

    void Start()
    {
        UpdateMasterVolume();
        UpdateBGMVolume();
        UpdateSEVolume();

        SceneManager.sceneLoaded += SetValue;
        SceneManager.sceneUnloaded += GetValue;
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
        if (PlayerPrefs.GetFloat("Master") == m_MasterSlider.value) return;
        if (PlayerPrefs.GetFloat("BGM") == m_BGMSlider.value) return;
        if (PlayerPrefs.GetFloat("SE") == m_SESlider.value) return;

        PlayerPrefs.SetFloat("Master", m_MasterSlider.value);
        PlayerPrefs.SetFloat("BGM", m_BGMSlider.value);
        PlayerPrefs.SetFloat("SE", m_SESlider.value);
        PlayerPrefs.Save();
    }

    void GetValue(Scene next)
    {
        if (PlayerPrefs.GetFloat("Master") == m_MasterSlider.value) return;
        if (PlayerPrefs.GetFloat("BGM") == m_BGMSlider.value) return;
        if (PlayerPrefs.GetFloat("SE") == m_SESlider.value) return;

        m_MasterSlider.value = PlayerPrefs.GetFloat("Master");
        m_BGMSlider.value = PlayerPrefs.GetFloat("BGM");
        m_SESlider.value = PlayerPrefs.GetFloat("SE");

        UpdateMasterVolume();
        UpdateBGMVolume();
        UpdateSEVolume();
    }
}
