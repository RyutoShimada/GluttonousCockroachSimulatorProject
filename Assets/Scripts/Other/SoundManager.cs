using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioClip m_cursolSE = null;
    [SerializeField] AudioClip m_clickSE = null;
    AudioSource m_audio = null;

    private void Start()
    {
        m_audio = GetComponent<AudioSource>();
    }

    public void Cursol()
    {
        m_audio.PlayOneShot(m_cursolSE);
    }

    public void Click()
    {
        m_audio.PlayOneShot(m_clickSE);
    }
}
