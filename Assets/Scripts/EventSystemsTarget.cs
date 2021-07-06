using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSystemsTarget : MonoBehaviour, IEventCaller
{
    /// <Summary>
    /// EventSystemsを使用してこのメソッドを呼び出します。
    /// </Summary>
    public void EventCall()
    {
        // ログを表示します。
        Debug.Log("EventSystemsによるイベントが通知された！");
        Debug.Log("ここに実行するメソッドを書いておくと呼ばれる");
    }
}
