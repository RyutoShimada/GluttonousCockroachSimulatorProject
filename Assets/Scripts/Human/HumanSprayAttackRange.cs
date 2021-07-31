using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanSprayAttackRange : MonoBehaviour
{
    /// <summary>ゴキブリに当たっているかどうか</summary>
    public bool m_sprayHit { get; private set; }

    private void Start()
    {
        m_sprayHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Cockroach") return;
        m_sprayHit = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag != "Cockroach") return;
         m_sprayHit = false;
    }
}
