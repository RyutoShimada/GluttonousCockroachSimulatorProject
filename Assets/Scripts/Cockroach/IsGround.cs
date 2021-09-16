using UnityEngine;
using Photon.Pun;

public class IsGround : MonoBehaviourPunCallbacks
{
    [Tooltip("CockroachMoveController がアタッチされているオブジェクトをアサインする")]
    [SerializeField] CockroachMoveController m_parent = null;

    private void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        m_parent.IsGround(true);
    }

    private void OnTriggerStay(Collider other)
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        m_parent.IsGround(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        m_parent.IsGround(false);
    }
}