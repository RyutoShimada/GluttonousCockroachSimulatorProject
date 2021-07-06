using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IEventCaller : IEventSystemHandler
{
    // イベントを呼び出すメソッドです。
    void EventCall();
}
