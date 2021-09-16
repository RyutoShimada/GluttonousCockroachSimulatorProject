using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Attack : MonoBehaviourPunCallbacks
{
    [SerializeField] int m_damge = 10;
    HumanAttackController m_human = null;

    void Start()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            GetComponent<Collider>().enabled = false;
            return;
        }
        m_human = transform.root.GetComponent<HumanAttackController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Cockroach")
        {
            m_human?.HitCockroach(m_damge);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Cockroach")
        {
            m_human?.HitCockroach(m_damge);
        }
    }
}
