using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Photon 用の名前空間を参照する
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class IsGround : MonoBehaviour
{
    CockroachMoveController m_parent = null;
    PhotonView m_view;

    // Start is called before the first frame update
    void Start()
    {
        m_parent = transform.parent.gameObject.GetComponent<CockroachMoveController>();
        m_view = GetComponent<PhotonView>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_view || !m_view.IsMine) return;
        if (other.tag != "Cockroach")
        {
            m_parent.IsGround(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!m_view || !m_view.IsMine) return;
        if (other.tag != "Cockroach")
        {
            m_parent.IsGround(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!m_view || !m_view.IsMine) return;
        if (other.tag != "Cockroach")
        {
            m_parent.IsGround(false);
        }
    }
}
