using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AttackRangeJudge : MonoBehaviour
{
    [Tooltip("トリガーに Cockroach または CockroachChild が接触した時に呼ぶ関数を登録する。")]
    [SerializeField] UnityEvent m_hitActions = default;
    [Tooltip("トリガーに Cockroach または CockroachChild 以外が接触した時に呼ぶ関数を登録する。")]
    [SerializeField] UnityEvent m_notHitactions = default;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Cockroach" || other.tag == "CockroachChild")
        {
            m_hitActions.Invoke();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Cockroach" || other.tag == "CockroachChild")
        {
            m_hitActions.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Cockroach" || other.tag == "CockroachChild")
        {
            m_notHitactions.Invoke();
        }
    }
}
