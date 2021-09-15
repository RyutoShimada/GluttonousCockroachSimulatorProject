using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Attack : MonoBehaviourPunCallbacks
{
    Human m_human = null;

    void Start()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        m_human = transform.root.GetComponent<Human>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Cockroach")
        {
            m_human?.HitCockroach();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Cockroach")
        {
            m_human?.HitCockroach();
        }
    }
}
