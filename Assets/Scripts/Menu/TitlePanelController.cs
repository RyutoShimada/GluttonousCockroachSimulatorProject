using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Panel
{
    Title,
    Tutorial,
    Menu,
    Cockroach,
    Human
}

public class TitlePanelController : MonoBehaviour
{
    [SerializeField] GameObject[] m_panels = null;
    [SerializeField] Button m_titleStartSelect = null;
    [SerializeField] Button m_tutorialStartSelect = null;
    [SerializeField] Button m_menuStartSelect = null;
    [SerializeField] Button m_cockroachStartSelect = null;
    [SerializeField] Button m_humanStartSelect = null;

    /// <summary>
    /// 次に表示したいPanelを表示させて、それ以外は非表示にする
    /// </summary>
    /// <param name="go">次に表示させたいPanelオブジェクト</param>
    public void ChanegePanel(GameObject go)
    {
        Panel nextPanel = Panel.Title;

        for (int i = 0; i < m_panels.Length; i++)
        {
            if (m_panels[i].gameObject.name == go.name)
            {
                nextPanel = (Panel)i;
            }
        }

        switch (nextPanel)
        {
            case Panel.Title:
                ChangeActive(nextPanel);
                m_titleStartSelect.Select();
                break;
            case Panel.Tutorial:
                ChangeActive(nextPanel);
                m_tutorialStartSelect.Select();
                break;
            case Panel.Menu:
                ChangeActive(nextPanel);
                m_menuStartSelect.Select();
                break;
            case Panel.Cockroach:
                ChangeActive(nextPanel);
                m_cockroachStartSelect.Select();
                break;
            case Panel.Human:
                ChangeActive(nextPanel);
                m_humanStartSelect.Select();
                break;
            default:
                break;
        }
    }

    void ChangeActive(Panel panel)
    {
        Active(panel);

        for (int i = 0; i < m_panels.Length; i++)
        {
            if (i == (int)panel) continue;

            UnActive((Panel)i);
        }
    }

    void Active(Panel panel)
    {
        m_panels[(int)panel].SetActive(true);
    }

    void UnActive(Panel panel)
    {
        m_panels[(int)panel].SetActive(false);
    }
}
