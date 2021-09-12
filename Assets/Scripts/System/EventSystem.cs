using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystem
{
    // 参考 : https://indie-du.com/entry/2017/05/26/130000

    public static EventSystem Instance = new EventSystem();

    //public static EventSystem Instance
    //{
    //    get
    //    {
    //        if (m_instance == null)
    //        {
    //            m_instance = new EventSystem();
    //        }

    //        return m_instance;
    //    }
    //}

    #region Definition Of Delegate

    // 通知受け取りデリゲート定義
    public delegate void CanMove(bool canMove);
    event CanMove m_canMove;

    public delegate void CockroachIsDed(bool isDed);
    event CockroachIsDed m_cockroachIsDed;

    public delegate void FoodGenerate();
    event FoodGenerate m_foodGenerate;

    public delegate void ResetTransform(Vector3 position, Quaternion rotation);
    event ResetTransform m_resetTransform;

    #endregion


    #region Subscribe To Event

    // 通知受け取り登録
    public void Subscribe(CanMove canMove)
    {
        m_canMove += canMove;
    }

    public void Subscribe(CockroachIsDed cockroachIsDed)
    {
        m_cockroachIsDed += cockroachIsDed;
    }

    public void Subscribe(FoodGenerate foodGenerate)
    {
        m_foodGenerate += foodGenerate;
    }

    public void Subscribe(ResetTransform resetTransform)
    {
        m_resetTransform += resetTransform;
    }

    #endregion


    #region Unsubscribe To Event

    // 通知受け取り登録解除
    public void Unsubscribe(CanMove canMove)
    {
        m_canMove -= canMove;
    }

    public void Unsubscribe(CockroachIsDed cockroachIsDed)
    {
        m_cockroachIsDed -= cockroachIsDed;
    }

    public void Unsubscribe(FoodGenerate foodGenerate)
    {
        m_foodGenerate -= foodGenerate;
    }

    public void Unsubscribe(ResetTransform resetTransform)
    {
        m_resetTransform -= resetTransform;
    }

    #endregion


    #region DoEvent

    // 通知実行
    public void IsMove(bool canMove)
    {
        if (m_canMove != null)
        {
            m_canMove(canMove);
        }
    }

    public void IsDed(bool isDed)
    {
        if (m_cockroachIsDed != null)
        {
            m_cockroachIsDed(isDed);
        }
    }

    public void Generate()
    {
        if (m_foodGenerate != null)
        {
            m_foodGenerate();
        }
    }

    public void Reset(Vector3 position, Quaternion rotation)
    {
        if (m_resetTransform != null)
        {
            m_resetTransform(position, rotation);
        }
    }

    #endregion
}
