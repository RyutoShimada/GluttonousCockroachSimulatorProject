using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInput : MonoBehaviourPunCallbacks
{
    [SerializeField] CockroachMoveController m_moveController = null;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        m_moveController.StartSet();
        StartCoroutine(UpdateCoroutine());
    }

    IEnumerator UpdateCoroutine()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) yield return null;

        while (!m_moveController.IsDed && m_moveController.m_canMove)
        {
            m_moveController.Jump(Input.GetButtonDown("Jump"));
            m_moveController.Move(Input.GetAxisRaw("Vertical"));
            m_moveController.MouseMove(Input.GetAxisRaw("Horizontal"), Input.GetAxis("Look X"));
            yield return null;
        }
    }
}
