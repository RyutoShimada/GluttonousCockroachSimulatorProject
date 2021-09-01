using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class IsGround : MonoBehaviourPunCallbacks
{
    [Tooltip("CockroachMoveController がアタッチされているオブジェクトをアサインする")]
    [SerializeField] CockroachMoveControllerNetWork m_parent = null;

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine || !photonView) return;

        if (other.tag != "Cockroach")
        {
            m_parent.IsGround(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!photonView.IsMine || !photonView) return;

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