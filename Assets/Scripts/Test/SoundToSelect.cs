using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 選択したときに音を出す
/// </summary>
public class SoundToSelect : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, ISelectHandler, ISubmitHandler
{
    [SerializeField] AudioSource m_seAudio = null;
    [SerializeField] AudioClip m_select = null;
    [SerializeField] AudioClip m_submit = null;

    public void OnPointerClick(PointerEventData eventData)
    {
        m_seAudio.PlayOneShot(m_submit);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_seAudio.PlayOneShot(m_select);
    }

    public void OnSelect(BaseEventData eventData)
    {
        m_seAudio.PlayOneShot(m_select);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        m_seAudio.PlayOneShot(m_submit);
    }
}
