using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInput : MonoBehaviourPunCallbacks, IIsCanMove
{
    [SerializeField] CockroachMoveController m_moveController = null;
    [HideInInspector] public bool m_canMove = true;
    Cockroach m_cockroach = null;
    [SerializeField, Range(0, 100)] int m_sensitivity = 1;

    public int Sensitivity
    {
        get => m_sensitivity;
        set
        {
            if (value < 0)
            {
                m_sensitivity = 0;
            }
            else if (value > 100)
            {
                m_sensitivity = 100;
            }
            else
            {
                m_sensitivity = value;
            }
        }
    }

    private void Awake()
    {
        m_cockroach = GetComponent<Cockroach>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        MenuController.IsMove += IsMove;
        SensitivityController.SetXSensitivity += SetXSensitivity;
        m_moveController.StartSetPlayer();
        StartCoroutine(UpdateCoroutine());
    }

    private void OnDestroy()
    {
        MenuController.IsMove -= IsMove;
        SensitivityController.SetXSensitivity -= SetXSensitivity;
    }

    void SetXSensitivity(int value) => Sensitivity = value;

    IEnumerator UpdateCoroutine()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) yield return null;
        while (!m_cockroach.m_isDed && m_canMove)
        {
            m_moveController.Jump(Input.GetButtonDown("Jump"));
            m_moveController.Move(Input.GetAxisRaw("Vertical"));
            m_moveController.MouseMove(Input.GetAxisRaw("Horizontal") * m_sensitivity, Input.GetAxis("Look X") * m_sensitivity);
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
