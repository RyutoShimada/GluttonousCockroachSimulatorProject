using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPageController : MonoBehaviour
{
    [SerializeField] GameObject[] m_panels = null;
    [SerializeField] GameObject m_nextButton = null;
    [SerializeField] GameObject m_buckButton = null;
    int m_currentPage = 0;

    private void OnEnable()
    {
        m_currentPage = 0;
        RestetPages();
        ButtonCheck();
    }

    void Active(int index)
    {
        m_panels[index].SetActive(true);
    }

    void UnActive(int index)
    {
        m_panels[index].SetActive(false);
    }

    void ButtonCheck()
    {
        if (m_currentPage == 0)
        {
            m_buckButton.SetActive(false);
            m_nextButton.SetActive(true);
            m_nextButton.GetComponent<Button>()?.Select();
        }
        else if (m_currentPage == m_panels.Length - 1)
        {
            m_buckButton.SetActive(true);
            m_nextButton.SetActive(false);
            m_buckButton.GetComponent<Button>()?.Select();
        }
        else
        {
            m_buckButton.SetActive(true);
            m_nextButton.SetActive(true);
        }
    }

    public void NextPage()
    {
        if (m_currentPage + 1 < m_panels.Length)
        {
            UnActive(m_currentPage);
            m_currentPage++;
            Active(m_currentPage);
        }
        ButtonCheck();
    }

    public void BeforePage()
    {
        if (m_currentPage - 1 >= 0)
        {
            UnActive(m_currentPage);
            m_currentPage--;
            Active(m_currentPage);
        }
        ButtonCheck();
    }

    void RestetPages()
    {
        for (int i = 0; i < m_panels.Length; i++)
        {
            UnActive(i);
        }

        Active(0);
    }
}
