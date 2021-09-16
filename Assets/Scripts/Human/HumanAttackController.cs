using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class HumanAttackController : MonoBehaviourPunCallbacks, IPunObservable, IIsCanMove
{
    public static Action<int> HitDamege;

    [HideInInspector] public bool m_canMove = true;

    /// <summary>パンチする速度</summary>
    [SerializeField] float m_punchSpeed = 0.1f;
    /// <summary>エネルギーの増加量</summary>
    [SerializeField] int m_addEnergyValue = 3;
    [Tooltip("パンチの当たり判定をするオブジェクト")]
    [SerializeField] Collider m_punchRange = null;
    [Tooltip("足の当たり判定をするオブジェクト")]
    [SerializeField] GameObject m_foolAttack = null;
    /// <summary>IKで右手を移動させるターゲット</summary>
    [SerializeField] Transform m_punchIKTarget = null;
    /// <summary>IKで右手を移動させるターゲット</summary>
    [SerializeField] Transform m_rightHandIKTarget = null;
    /// <summary>IKで右腕を移動させるターゲット</summary>
    [SerializeField] Transform m_rightArmIKTarget = null;
    /// <summary>IKで左腕を移動させるターゲット</summary>
    [SerializeField] Transform m_leftArmIKTarget = null;
    /// <summary>IKを滑らかに実行する速度</summary>
    [SerializeField] float m_rightIKPositionWeightSpeed = 1f;
    /// <summary>攻撃時のビームのパーティクル</summary>
    [SerializeField] GameObject m_beamPrefab = null;
    /// <summary>攻撃時のビームのパーティクル</summary>
    [SerializeField] GameObject m_chargePrefab = null;
    [Tooltip("エネルギーが溜まった音")]
    [SerializeField] AudioClip m_energyChargedSE = null;
    [Tooltip("チャージ音")]
    [SerializeField] AudioClip m_chargedSE = null;
    [Tooltip("ビーム音")]
    [SerializeField] AudioClip m_beamSE = null;

    AudioSource m_audio = null;
    Animator m_anim = null;
    /// <summary>ビーム攻撃の準備ができているかどうか</summary>
    bool m_isStandByBeam = false;
    /// <summary>ビームで攻撃しているかどうか</summary>
    bool m_isBeamAttacking = false;
    /// <summary>ビームで攻撃しているかどうか</summary>
    bool m_isLeftAttacking = false;
    /// <summary>ビームで攻撃しているかどうか</summary>
    bool m_isRightAttacking = false;
    /// <summary>実際に変化するのIKのアニメーション速度</summary>
    float m_localRightHandIkWeight = 0f;

    Transform m_leftArmOriginPos = null;
    Transform m_rightArmOriginPos = null;

    readonly int m_needEnergy = 100;
    int m_currentEnergy = 0;
    bool m_chargeCompleted = false;

    Human m_human = null;

    const float beamTime = 3f;

    void Start()
    {
        m_audio = GetComponent<AudioSource>();
        m_anim = GetComponent<Animator>();
        m_leftArmOriginPos = m_leftArmIKTarget;
        m_rightArmOriginPos = m_rightArmIKTarget;

        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        MenuController.IsMove += IsMove;
        EventSystem.Instance.Subscribe(AddEnergy);
        m_human = GetComponent<Human>();
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        if (!m_canMove) return;
        Attack();
    }

    private void OnDestroy() => EventSystem.Instance.Unsubscribe(AddEnergy);

    public void IsMove(bool isMove) => m_canMove = isMove;

    void Attack()
    {
        if (m_chargeCompleted)
        {
            if (Input.GetButton("Jump"))
            {
                StandByBeam();
                if (!PhotonNetwork.IsConnected) return;
                photonView.RPC(nameof(StandByBeam), RpcTarget.All);
            }
            else if (Input.GetButtonUp("Jump"))
            {
                FireBeam();
                if (!PhotonNetwork.IsConnected) return;
                photonView.RPC(nameof(FireBeam), RpcTarget.All);
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                m_isLeftAttacking = true;
                StartPunching();
            }
            else if (Input.GetButtonDown("Fire2"))
            {
                m_isRightAttacking = true;
                StartPunching();
            }
        }
    }

    public void FootAttack() => m_foolAttack.SetActive(true);
    public void FootAttackCancel() => m_foolAttack.SetActive(false);

    public void HitCockroach(int damge)
    {
        Debug.Log("Hit");
        HitDamege.Invoke(damge);
    }

    [PunRPC]
    void StartPunching()
    {
        if (m_isBeamAttacking) return;
        StartCoroutine(Punching());
    }

    IEnumerator Punching()
    {
        m_punchRange.enabled = true;

        if (m_isLeftAttacking)
        {
            m_leftArmIKTarget = m_punchIKTarget;
            yield return new WaitForSeconds(m_punchSpeed);
            m_leftArmIKTarget = m_leftArmOriginPos;
            m_isLeftAttacking = false;
        }
        else if (m_isRightAttacking)
        {
            m_rightArmIKTarget = m_punchIKTarget;
            yield return new WaitForSeconds(m_punchSpeed);
            m_rightArmIKTarget = m_rightArmOriginPos;
            m_isRightAttacking = false;
        }

        m_punchRange.enabled = false;
        m_human.JudgeAttack(false);
    }

    void AddEnergy()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        if (m_isBeamAttacking || m_chargeCompleted) return;

        if (m_currentEnergy + m_addEnergyValue <= m_needEnergy)
        {
            m_currentEnergy += m_addEnergyValue;
        }
        else
        {
            m_currentEnergy = m_needEnergy;
            m_chargeCompleted = true;
            m_audio.PlayOneShot(m_energyChargedSE);
        }

        m_human.ChangeGauge(m_currentEnergy, 0.2f); // UIのGaugeに反映
    }

    [PunRPC]
    void StandByBeam()
    {
        if (m_isBeamAttacking) return;

        if (!m_isStandByBeam) m_isStandByBeam = true;
        // チャージエフェクトを表示
        if (!m_chargePrefab.activeSelf) m_chargePrefab.SetActive(true);
        // SEを再生
        if (!m_audio.isPlaying) m_audio.PlayOneShot(m_chargedSE);
    }

    [PunRPC]
    void FireBeam()
    {
        StartCoroutine(Beam());
    }

    IEnumerator Beam()
    {
        m_isBeamAttacking = true;
        m_chargePrefab.SetActive(false);
        m_beamPrefab.SetActive(true);
        m_currentEnergy = 0;
        m_chargeCompleted = false;
        m_audio.PlayOneShot(m_beamSE);
        m_human?.ChangeGauge(m_currentEnergy, beamTime); // UIのGaugeに反映
        yield return new WaitForSeconds(beamTime);
        m_beamPrefab.SetActive(false);
        m_isBeamAttacking = false;
        m_isStandByBeam = false;
        m_audio.Stop();
    }

    void BeamIkWeight()
    {
        // IKアニメーションを滑らかにする処理
        if (m_isStandByBeam)
        {
            if (m_localRightHandIkWeight < 1.0f)
            {
                m_localRightHandIkWeight += m_rightIKPositionWeightSpeed * Time.deltaTime;
            }
            else
            {
                m_localRightHandIkWeight = 1.0f;
            }
        }
        else
        {
            if (m_localRightHandIkWeight > 0f)
            {
                m_localRightHandIkWeight -= m_rightIKPositionWeightSpeed * Time.deltaTime;
            }
            else
            {
                m_localRightHandIkWeight = 0f;
            }
        }
    }

    // IK を計算するためのコールバック
    private void OnAnimatorIK(int layerIndex)
    {
        if (m_anim == null) return;

        BeamIkWeight();

        if (m_rightArmIKTarget != null) // 右腕
        {
            m_anim.SetIKPosition(AvatarIKGoal.RightHand, m_rightArmIKTarget.position);
            m_anim.SetIKRotation(AvatarIKGoal.RightHand, m_rightArmIKTarget.rotation);
            m_anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            m_anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
        }

        if (m_leftArmIKTarget != null) // 左腕
        {
            m_anim.SetIKPosition(AvatarIKGoal.LeftHand, m_leftArmIKTarget.position);
            m_anim.SetIKRotation(AvatarIKGoal.LeftHand, m_leftArmIKTarget.rotation);
            m_anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            m_anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
        }

        if (m_rightHandIKTarget != null && m_isStandByBeam) // ビームの時の手
        {
            m_anim.SetIKPosition(AvatarIKGoal.RightHand, m_rightHandIKTarget.position);
            m_anim.SetIKRotation(AvatarIKGoal.RightHand, m_rightHandIKTarget.rotation);
            m_anim.SetIKPositionWeight(AvatarIKGoal.RightHand, m_localRightHandIkWeight);
            m_anim.SetIKRotationWeight(AvatarIKGoal.RightHand, m_localRightHandIkWeight);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && photonView.IsMine)
        {
            stream.SendNext(m_isBeamAttacking);
            stream.SendNext(m_isStandByBeam);
            stream.SendNext(m_currentEnergy);

            if (m_isStandByBeam)
            {
                stream.SendNext(m_localRightHandIkWeight);
                stream.SendNext(m_rightHandIKTarget.position);
                stream.SendNext(m_rightHandIKTarget.rotation);
            }
            else
            {
                stream.SendNext(m_rightArmIKTarget.position);
                stream.SendNext(m_rightArmIKTarget.rotation);
                stream.SendNext(m_leftArmIKTarget.position);
                stream.SendNext(m_leftArmIKTarget.rotation);
            }
        }
        else if (stream.IsReading && !photonView.IsMine)
        {
            m_isBeamAttacking = (bool)stream.ReceiveNext();
            m_isStandByBeam = (bool)stream.ReceiveNext();
            m_currentEnergy = (int)stream.ReceiveNext();

            if (m_isStandByBeam)
            {
                m_localRightHandIkWeight = (float)stream.ReceiveNext();
                m_rightHandIKTarget.position = (Vector3)stream.ReceiveNext();
                m_rightHandIKTarget.rotation = (Quaternion)stream.ReceiveNext();
            }
            else
            {
                m_rightArmIKTarget.position = (Vector3)stream.ReceiveNext();
                m_rightArmIKTarget.rotation = (Quaternion)stream.ReceiveNext();
                m_leftArmIKTarget.position = (Vector3)stream.ReceiveNext();
                m_leftArmIKTarget.rotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}
