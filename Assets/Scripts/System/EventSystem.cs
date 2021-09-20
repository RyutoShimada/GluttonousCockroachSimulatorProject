using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystem
{
    // 参考 : https://indie-du.com/entry/2017/05/26/130000

    public static EventSystem Instance = new EventSystem();


    #region Definition Of Delegate

    // 通知受け取りデリゲート定義
    // public delegate void Life(bool isDed);
    //event Life Ded;

    public delegate void Reset(Vector3 position, Quaternion rotation);
    event Reset Transform;

    #endregion


    #region Subscribe To Event

    // 通知受け取り登録
    //public void Subscribe(Life cockroachIsDed)
    //{
    //    Ded += cockroachIsDed;
    //}

    public void Subscribe(Reset resetTransform)
    {
        Transform += resetTransform;
    }

    #endregion


    #region Unsubscribe To Event

    // 通知受け取り登録解除
    //public void Unsubscribe(Life cockroachIsDed)
    //{
    //    Ded -= cockroachIsDed;
    //}

    public void Unsubscribe(Reset resetTransform)
    {
        Transform -= resetTransform;
    }

    #endregion


    #region DoEvent

    // 通知実行
    //public void IsDed(bool isDed)
    //{
    //    if (Ded != null)
    //    {
    //        Ded(isDed);
    //    }
    //}

    public void ResetTransform(Vector3 position, Quaternion rotation)
    {
        if (Transform != null)
        {
            Transform(position, rotation);
        }
    }

    #endregion
}
