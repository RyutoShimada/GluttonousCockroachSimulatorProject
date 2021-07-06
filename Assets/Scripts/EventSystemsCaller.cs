using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemsCaller : MonoBehaviour
{
    public GameObject targetObj;

    void Start()
    {
        DoMyEvent();
    }

    /// <Summary>
    /// EventSystemsを使用してイベントを実行します。
    /// </Summary>
    void DoMyEvent()
    {
        NotifyEvent(targetObj);
    }

    /// <Summary>
    /// 対象のオブジェクトにイベントを通知します。
    /// </Summary>
    /// <param name="targetObj">対象のオブジェクト</param>
    void NotifyEvent(GameObject targetObj)
    {
        ExecuteEvents.Execute<IEventCaller>(
                        target: targetObj,
                        eventData: null,
                        functor: CallMyEvent
                        );
    }

    /// <Summary>
    /// このイベントで指定するインタフェースのメソッドです。
    /// </Summary>
    void CallMyEvent(IEventCaller inf, BaseEventData eventData)
    {
        inf.EventCall();
    }
}
