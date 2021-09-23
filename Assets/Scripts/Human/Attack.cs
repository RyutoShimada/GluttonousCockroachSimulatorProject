using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Attack : MonoBehaviourPunCallbacks
{
    [SerializeField] int m_damge = 10;
    [SerializeField] AudioClip m_clip = null;
    HumanAttackController m_human = null;
    AudioSource m_audio = null;

    void Start()
    {
        m_audio = transform.root.GetComponent<AudioSource>();
        if (PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            GetComponent<Collider>().enabled = false;
            return;
        }
        m_human = transform.root.GetComponent<HumanAttackController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Cockroach" && other.tag != "CockroachChild") return;

        if (m_clip != null)
        {
            m_audio?.PlayOneShot(m_clip);
        }

        if (other.tag == "Cockroach")
        {
            m_human?.HitCockroach(m_damge);
        }
    }
}
