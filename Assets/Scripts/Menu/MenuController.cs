using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject m_menu = null;
    [SerializeField] GameObject[] m_menus = null;
    [SerializeField] Selectable[] m_startSelects = null;
    RectTransform m_rect;
    public static Action<bool> IsMove;

    void Awake()
    {
        m_rect = transform.parent.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!NetWorkGameManager.m_Instance) return;
        if (Input.GetButtonDown("Start"))
        {
            if (!m_menu.activeSelf)
            {
                Open();
                m_rect.SetAsLastSibling();//最前面に持ってくる
                ChangeMenu(m_menus[0]);
            }
            else
            {
                Close();
            }
        }
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

    void UnActive(GameObject go)
    {
        if (!go.activeSelf) return;
        go.SetActive(false);
    }

    void Open()
    {
        m_menu.gameObject.SetActive(true);
        IsMove?.Invoke(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Close()
    {
        m_menu.gameObject.SetActive(false);
        IsMove?.Invoke(true);
        if (!NetWorkGameManager.m_Instance) return;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
