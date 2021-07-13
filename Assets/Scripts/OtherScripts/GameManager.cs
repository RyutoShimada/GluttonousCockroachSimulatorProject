using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] Text m_timerText = null;
    [SerializeField] int m_minutes = 5;
    [SerializeField] float m_seconds = 59f;
    bool m_isGame = false;

    // Start is called before the first frame update
    void Start()
    {
        m_isGame = true;
    }

    // Update is called once per frame
    void Update()
    {
        TimeCountDown();
    }

    void TimeCountDown()
    {
        if (!m_isGame) return;

        if (m_seconds > 0)
        {
            m_seconds -= Time.deltaTime;
        }
        else
        {
            if (m_minutes > 0)
            {
                m_minutes--;
                m_seconds = 59f;
            }
            else
            {
                m_minutes = 0;
                m_seconds = 0;
                m_isGame = false;
                Debug.Log("TimeUp!");
            }
        }

        if (m_timerText != null)
        {
            m_timerText.text = $"{m_minutes.ToString("00")} : {m_seconds.ToString("00")}";
        }
    }
}
