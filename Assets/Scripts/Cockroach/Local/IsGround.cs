using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsGround : MonoBehaviour
{
    [Tooltip("CockroachMoveController がアタッチされているオブジェクトをアサインする")]
    [SerializeField] CockroachMoveController m_parent = null;

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag != "Cockroach")
        {
            m_parent.IsGround(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {

        if (other.tag != "Cockroach")
        {
            m_parent.IsGround(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Cockroach")
        {
            m_parent.IsGround(false);
        }
    }
}
