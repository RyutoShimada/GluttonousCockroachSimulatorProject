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
    [SerializeField] GameObject m_canvas = null;
    [SerializeField] FoodGenerater m_foodGenerater = null;
    [SerializeField] int m_generateFoodCount = 1;
    [SerializeField] float m_generateInterval = 3f;
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

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Start is called before the first frame update
    void Start()
    {
        ChangeOperationSutate();

        m_isGame = true;

        if (m_cockoroach && m_human)
        {
            m_cockoroachCamera = m_cockoroach.transform.Find("Main Camera").gameObject;
            m_humanCamera = m_human.transform.Find("Main Camera").gameObject;
        }

        if (m_foodGenerater)
        {
            FoodGenerate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_isGame) return;
        TimeCountDown();
        ChangeOperateOnPlaying();
    }

    private void OnValidate()
    {
        ChangeOperationSutate();
    }

    private void ChangeOperationSutate()
    {
        if (!m_cockoroach || !m_cockoroachCamera || !m_cockroachCanvas || !m_human || !m_humanCamera || !m_isGame) return;

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
                m_canvas.transform.Find("GameOverText").gameObject.SetActive(true);
                m_human.GetComponent<HumanMoveController>().IsCanMove = false;
                m_cockoroach.GetComponent<CockroachMoveController>().IsCanMove = false;
            }
        }

        if (m_timerText != null)
        {
            m_timerText.text = $"{m_minutes.ToString("00")} : {m_seconds.ToString("00")}";
        }
    }

    public void FoodGenerate()
    {
        StartCoroutine(m_foodGenerater.Generate(m_generateFoodCount, m_generateInterval));
    }

    /// <summary>
    /// ビルドした後にプレイ中に操作キャラを変更できる
    /// </summary>
    void ChangeOperateOnPlaying()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.RightShift))
        {
            if (OperationSutate == OperationSutate.CockRoach)
            {
                OperationSutate = OperationSutate.Human;
            }
            else
            {
                OperationSutate = OperationSutate.CockRoach;
            }
        }
    }
}
