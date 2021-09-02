using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OutlineOfTheSelectedChracter : MonoBehaviour
{
    [SerializeField] CockroachOutline m_cockroachOutline = null;
    [SerializeField] HumanOutline m_humanOutline = null;

    [SerializeField] AudioClip m_cursolSE = null;
    [SerializeField] AudioClip m_clickSE = null;

    Vector3 m_cursorPosition;
    Vector3 m_cursorPosition3d;
    RaycastHit m_hit;

    AudioSource m_audio;

    private void Start()
    {
        m_audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        OnRay();
    }

    void OnRay()
    {
        m_cursorPosition = Mouse.current.position.ReadValue();
        m_cursorPosition.z = 10.0f; // z座標に適当な値を入れる
        m_cursorPosition3d = Camera.main.ScreenToWorldPoint(m_cursorPosition); // 3Dの座標になおす

        // カメラから cursorPosition3d の方向へレイを飛ばす
        if (Physics.Raycast(Camera.main.transform.position, (m_cursorPosition3d - Camera.main.transform.position), out m_hit, Mathf.Infinity))
        {
            Debug.DrawRay(Camera.main.transform.position, (m_cursorPosition3d - Camera.main.transform.position) * m_hit.distance, Color.red);

            if (m_hit.collider.gameObject.tag == "Cockroach")
            {
                if (!m_cockroachOutline.enabled)
                {
                    m_cockroachOutline.enabled = true;
                    m_audio.PlayOneShot(m_cursolSE);
                }
            }
            else if (m_hit.collider.gameObject.tag == "Human")
            {
                if (!m_humanOutline.enabled)
                {
                    m_humanOutline.enabled = true;
                    m_audio.PlayOneShot(m_cursolSE);
                }
            }
            else
            {
                UnEnabled();
            }
        }
        else
        {
            UnEnabled();
        }
    }

    void UnEnabled()
    {
        if (m_cockroachOutline.enabled)
        {
            m_cockroachOutline.enabled = false;
        }

        if (m_humanOutline.enabled)
        {
            m_humanOutline.enabled = false;
        }
    }

    public void Click()
    {
        m_audio.PlayOneShot(m_clickSE);
    }
}
