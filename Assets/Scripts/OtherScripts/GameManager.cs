using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum OperationSutate
{
    CockRoach,
    Human
}

public class GameManager : MonoBehaviour
{
    [SerializeField] Text m_timerText = null;
    [SerializeField] int m_minutes = 5;
    [SerializeField] float m_seconds = 59f;

    [SerializeField] GameObject m_cockoroach = null;
    [SerializeField] GameObject m_human = null;
    [SerializeField] GameObject m_cockroachCanvas = null;
    GameObject m_cockoroachCamera = null;
    GameObject m_humanCamera = null;
    [SerializeField] OperationSutate m_os = OperationSutate.CockRoach;

    bool m_isGame = false;

    public OperationSutate OperationSutate
    {
        get => m_os;
        set
        {
            m_os = value;
            ChangeOperationSutate();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ChangeOperationSutate();

        m_isGame = true;

        Cursor.lockState = CursorLockMode.Locked;

        if (m_cockoroach && m_human)
        {
            m_cockoroachCamera = m_cockoroach.transform.Find("Main Camera").gameObject;
            m_humanCamera = m_human.transform.Find("Main Camera").gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        TimeCountDown();
    }

    private void OnValidate()
    {
        ChangeOperationSutate();
    }

    private void ChangeOperationSutate()
    {
        if (!m_cockoroach || !m_cockoroachCamera || !m_cockroachCanvas || !m_human || !m_humanCamera) return;

        switch (m_os)
        {
            case OperationSutate.CockRoach:
                m_cockoroach.GetComponent<CockroachMoveController>().IsCanMove = true;
                m_human.GetComponent<HumanMoveController>().IsCanMove = false;
                m_humanCamera.SetActive(false);
                m_cockoroachCamera.SetActive(true);
                m_cockroachCanvas.SetActive(true);
                break;
            case OperationSutate.Human:
                m_human.GetComponent<HumanMoveController>().IsCanMove = true;
                m_cockoroach.GetComponent<CockroachMoveController>().IsCanMove = false;
                m_cockoroachCamera.SetActive(false);
                m_humanCamera.SetActive(true);
                m_cockroachCanvas.SetActive(false);
                break;
            default:
                break;
        }
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
