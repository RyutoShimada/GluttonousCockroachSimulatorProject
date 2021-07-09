using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 食べ物にアタッチし、オブジェクトプールで生成する。
/// </summary>
public class Food : MonoBehaviour
{
    /// <summary>回復する値</summary>
    public int m_heelValue = 10;

    /// <summary>
    /// このオブジェクトを非アクティブにする。(Cockroachから呼ばれる)
    /// </summary>
    public void UnActive()
    {
        this.gameObject.SetActive(false);
    }
}
