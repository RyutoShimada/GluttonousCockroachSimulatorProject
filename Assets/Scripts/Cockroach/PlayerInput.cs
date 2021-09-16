using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInput : MonoBehaviourPunCallbacks, IIsCanMove
{
    [SerializeField] CockroachMoveController m_moveController = null;
    [HideInInspector]
    public bool m_canMove = true;
    Cockroach m_cockroach = null;

    private void Awake()
    {
        m_cockroach = GetComponent<Cockroach>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        MenuController.IsMove += IsMove;
        m_moveController.StartSetPlayer();
        StartCoroutine(UpdateCoroutine());
    }

    IEnumerator UpdateCoroutine()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        while (!m_cockroach.m_isDed && m_canMove)
        {
            m_moveController.Jump(Input.GetButtonDown("Jump"));
            m_moveController.Move(Input.GetAxisRaw("Vertical"));
            m_moveController.MouseMove(Input.GetAxisRaw("Horizontal"), Input.GetAxis("Look X"));
            yield return null;
        }
    }

    public void IsMove(bool isMove)
    {
        m_canMove = isMove;

        if (m_canMove)
        {
            StartCoroutine(UpdateCoroutine());
        }
    }
}
