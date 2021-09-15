using UnityEngine;
using Photon.Pun;

public class IsGround : MonoBehaviourPunCallbacks
{
    [Tooltip("CockroachMoveController がアタッチされているオブジェクトをアサインする")]
    [SerializeField] CockroachMoveController m_parent = null;

    private void OnTriggerEnter(Collider other)
    {
        DoTask();
    }

    private void OnTriggerStay(Collider other)
    {
        DoTask();
    }

    private void OnTriggerExit(Collider other)
    {
        DoTask();
    }

    void DoTask()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        m_parent.IsGround(false);
    }
}