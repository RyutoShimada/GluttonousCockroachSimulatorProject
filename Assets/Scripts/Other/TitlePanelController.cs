using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Panel
{
    Title,
    Solo,
    Multi,
    Tutorial,
    Cockroach,
    Human
}

public class TitlePanelController : MonoBehaviour
{
    [SerializeField] GameObject[] m_panels = null;

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
                break;
            case Panel.Solo:
                ChangeActive(nextPanel);
                break;
            case Panel.Multi:
                ChangeActive(nextPanel);
                break;
            case Panel.Tutorial:
                ChangeActive(nextPanel);
                break;
            case Panel.Cockroach:
                ChangeActive(nextPanel);
                break;
            case Panel.Human:
                ChangeActive(nextPanel);
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
