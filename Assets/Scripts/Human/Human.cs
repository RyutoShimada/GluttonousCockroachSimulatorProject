using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using Photon.Pun;
using DG.Tweening;
using System;

public class Human : MonoBehaviourPunCallbacks, IIsCanMove
{
    public static Action<bool> CockChildAttack;
    [SerializeField] Transform m_cameraPos = null;
    [SerializeField] GameObject m_vcamPrefab = null;
    [SerializeField] GameObject m_humanUiPrefab = null;

    Image m_crossHair = null;
    Image m_gauge = null;
    Text m_energyText = null;
    HumanMoveController m_moveController = null;
    GameObject m_vcam = null;
    CinemachineVirtualCamera m_vcamBase = null;
    GameObject m_ui = null;

    int m_energyValue = 0;

    private void Awake()
    {
        m_moveController = GetComponent<HumanMoveController>();
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;

        EventSystem.Instance.Subscribe(ResetPosition);
        MenuController.IsMove += IsMove;
        //NPCController.Ded += CockChilAttackRPC;
        NPCController.Ded += JudgeAttackCockChilCallback;
        Transform t = GameObject.Find("Canvas").transform;
        m_ui = Instantiate(m_humanUiPrefab, t);
        m_crossHair = m_ui.transform.Find("CrossHair").GetComponent<Image>();
        m_gauge = m_ui.transform.Find("Gauge").transform.Find("GaugeImage").GetComponent<Image>();
        m_energyText = m_ui.GetComponentInChildren<Text>();
        VcamSetUp();
    }

    void Update()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        m_cameraPos.rotation = Camera.main.transform.rotation;
    }

    private void OnDestroy()
    {
        MenuController.IsMove -= IsMove;
        NPCController.Ded -= CockChilAttackRPC;
        NPCController.Ded -= JudgeAttackCockChilCallback;
        EventSystem.Instance.Unsubscribe(ResetPosition);
    }

    /// <summary>
    /// 外部から呼ばれる。ゴキブリが攻撃可能範囲内にいればクロスヘアが赤くなる
    /// </summary>
    /// <param name="hit"></param>
    public void JudgeAttack(bool hit)
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;

        if (hit)
        {
            m_crossHair.color = Color.red;
        }
        else
        {
            m_crossHair.color = Color.white;
        }
    }

    void JudgeAttackCockChilCallback()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        m_crossHair.color = Color.white;
    }

    public void ChangeGauge(int value, float time)
    {
        m_gauge.DOFillAmount(value * 0.01f, time);
        // 数値を滑らかに変動させている
        DOTween.To(() => m_energyValue, n => m_energyValue = n, value, time)
            .OnUpdate(() => m_energyText.text = m_energyValue.ToString());
    }

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
        m_moveController.m_canMove = isMove;

        if (m_vcamBase)
        {
            m_vcamBase.enabled = isMove;
        }
    }

    void CockChilAttackRPC()
    {
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(CockChildAttacking), RpcTarget.OthersBuffered);
        }
    }

    [PunRPC]
    public void CockChildAttacking()
    {
        CockChildAttack.Invoke(false);
    }
}
