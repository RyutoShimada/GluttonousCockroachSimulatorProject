using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject[] m_menus = null;
    [SerializeField] Selectable[] m_startSelects = null;
    RectTransform m_rect;

    private void Awake()
    {
        m_rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        m_rect.SetAsLastSibling();//最前面に持ってくる
        ChangeMenu(m_menus[0]);
    }

    /// <summary>
    /// メニュー変更
    /// </summary>
    /// <param name="next">次に表示させたいメニューオブジェクト</param>
    public void ChangeMenu(GameObject next)
    {
        for (int i = 0; i < m_menus.Length; i++)
        {
            if (m_menus[i] == next)
            {
                OnActive(m_menus[i]);
                m_startSelects[i].Select();
            }
            else
            {
                UnActive(m_menus[i]);
            }
        }   
    }

    void OnActive(GameObject go)
    {
        if (go.activeSelf) return;
        go.SetActive(true);
    }

    public void UnActive(GameObject go)
    {
        if (!go.activeSelf) return;
        go.SetActive(false);
    }
}
