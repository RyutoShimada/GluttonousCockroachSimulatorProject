using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystem
{
    // 参考 : https://indie-du.com/entry/2017/05/26/130000

    static EventSystem m_instance;

    public static EventSystem Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new EventSystem();
            }

            return m_instance;
        }
    }

    // 通知受け取りデリゲート定義
    public delegate void CockroachIsDed(bool isDed);
    event CockroachIsDed m_cockroachIsDed;

    // 通知受け取り登録
    public void Subscribe(CockroachIsDed cockroachIsDed)
    {
        m_cockroachIsDed += cockroachIsDed;
    }

    // 通知受け取り登録解除
    public void Unsubscribe(CockroachIsDed cockroachIsDed)
    {
        m_cockroachIsDed -= cockroachIsDed;
    }

    // 通知実行
    public void IsDed(bool isDed)
    {
        if (m_cockroachIsDed != null)
        {
            m_cockroachIsDed(isDed);
        }
    }
}
