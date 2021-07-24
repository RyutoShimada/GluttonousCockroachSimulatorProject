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

    #region Definition Of Delegate

    // 通知受け取りデリゲート定義
    public delegate void CockroachIsDed(bool isDed);
    event CockroachIsDed m_cockroachIsDed;

    public delegate void FoodGenerate(int count);
    event FoodGenerate m_foodGenerate;

    #endregion


    #region Subscribe To Event

    // 通知受け取り登録
    public void Subscribe(CockroachIsDed cockroachIsDed)
    {
        m_cockroachIsDed += cockroachIsDed;
    }

    public void Subscribe(FoodGenerate foodGenerate)
    {
        m_foodGenerate += foodGenerate;
    }

    #endregion


    #region Unsubscribe To Event

    // 通知受け取り登録解除
    public void Unsubscribe(CockroachIsDed cockroachIsDed)
    {
        m_cockroachIsDed -= cockroachIsDed;
    }

    public void Unsubscribe(FoodGenerate foodGenerate)
    {
        m_foodGenerate -= foodGenerate;
    }

    #endregion


    #region DoEvent

    // 通知実行
    public void IsDed(bool isDed)
    {
        if (m_cockroachIsDed != null)
        {
            m_cockroachIsDed(isDed);
        }
    }

    public void Generate(int count)
    {
        if (m_foodGenerate != null)
        {
            m_foodGenerate(count);
        }
    }

    #endregion
}
