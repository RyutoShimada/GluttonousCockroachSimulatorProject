using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using Photon.Pun;

public class Human : MonoBehaviourPunCallbacks, IIsCanMove
{
    [SerializeField] Transform m_cameraPos = null;
    [SerializeField] GameObject m_vcamPrefab = null;
    [SerializeField] GameObject m_humanUi = null;

    [HideInInspector] public Image m_crossHair = null;

    HumanMoveController m_moveController = null;
    GameObject m_vcam = null;
    CinemachineVirtualCamera m_vcamBase = null;
    GameObject m_ui = null;

    private void Start()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;

        EventSystem.Instance.Subscribe((EventSystem.ResetTransform)ResetPosition);
        Transform t = GameObject.Find("Canvas").transform;
        m_ui = Instantiate(m_humanUi, t);
        m_crossHair = m_ui.GetComponent<Image>();
        m_moveController = GetComponent<HumanMoveController>();
        VcamSetUp();
    }

    void Update()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        m_cameraPos.rotation = Camera.main.transform.rotation;
    }

    private void OnDestroy() => EventSystem.Instance.Unsubscribe((EventSystem.ResetTransform)ResetPosition);

    void VcamSetUp()
    {
        if (m_vcamPrefab)
        {
            m_vcam = Instantiate(m_vcamPrefab, m_cameraPos.position, m_cameraPos.rotation);
        }
        else
        {
            Debug.LogError("m_vcamPrefab がアサインされていません");
        }

        if (m_vcam.TryGetComponent(out CinemachineVirtualCamera vcam))
        {
            m_vcamBase = vcam.GetComponent<CinemachineVirtualCamera>();
            vcam.Follow = m_cameraPos;
            m_vcamBase.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = Vector3.zero;

            if (!m_moveController.m_canMove)
            {
                m_vcamBase.enabled = false;
            }
        }
        else
        {
            Debug.LogError("CinemachineVirtualCamera を GetComponent 出来ませんでした");
        }
    }

    void ResetPosition(Vector3 v, Quaternion q)
    {
        if (photonView.IsMine)
        {
            this.transform.position = v;
            this.transform.rotation = q;
            m_vcamBase.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = Vector3.zero;
            m_vcamBase.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value = 0;
            m_vcamBase.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value = 0;
        }
    }

    /// <summary>
    /// Human の UI を非表示にします
    /// </summary>
    public void UiSetActiveFalse() => m_ui.SetActive(false);

    public void IsMove(bool isMove)
    {
        if (m_vcamBase)
        {
            m_vcamBase.enabled = isMove;
        }
    }

    public void HitCockroach()
    {
        Debug.Log("Hit");
    }
}
