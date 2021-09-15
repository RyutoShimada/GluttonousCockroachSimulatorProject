using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Cinemachine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class HumanMoveControllerNetWork : MonoBehaviourPunCallbacks, IPunObservable, IIsCanMove
{
    static public GameObject m_Instance;

    /// <summary>移動速度</summary>
    [SerializeField] float m_moveSpeed = 1f;
    /// <summary>体を回転する速度</summary>
    [SerializeField] float m_turnSpeed = 1f;
    /// <summary>攻撃する範囲のオブジェクト</summary>
    GameObject m_attackRangeObj = null;
    /// <summary>Rayを飛ばす最大距離</summary>
    [SerializeField] float m_maxRayDistance = 1f;

    [SerializeField] Transform m_cameraPos = null;
    [SerializeField] GameObject m_vcamPrefab = null;

    /// <summary>攻撃値</summary>
    [SerializeField] int m_attackValue = 20; // 後で別のスクリプトに移すこと

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
    /// <summary>実際に変化するのIKのアニメーション速度</summary>
    float m_localRightHandIkWeight = 0f;

    /// <summary>スクリプト</summary>
    [SerializeField] HumanSprayAttackControllerNetWork m_HSACN = null;
    /// <summary>垂直方向と水平方向の入力を受け付ける</summary>
    Vector2 m_input;
    /// <summary>方向</summary>
    Vector3 m_dir;
    /// <summary>速度ベクト</summary>
    Vector3 m_vel;
    /// <summary>ビームで攻撃しているかどうか</summary>
    bool m_isBeamAttacking = false;
    /// <summary>ビームで攻撃しているかどうか</summary>
    bool m_isLeftAttacking = false;
    /// <summary>ビームで攻撃しているかどうか</summary>
    bool m_isRightAttacking = false;
    Rigidbody m_rb;
    Animator m_anim;
    RaycastHit m_hit;

    GameObject m_vcam = null;
    CinemachineVirtualCamera m_vcamBase = null;
    [HideInInspector]
    public bool m_canMove = true;

    void Start()
    {
        m_anim = GetComponent<Animator>();

        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;

        m_rb = GetComponent<Rigidbody>();
        EventSystem.Instance.Subscribe((EventSystem.ResetTransform)ResetPosition);

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
            if (!m_canMove)
            {
                m_vcamBase.enabled = false;
            }
        }
        else
        {
            Debug.LogError("CinemachineVirtualCamera を GetComponent 出来ませんでした");
        }

        //m_attackRangeObj = transform.Find("CameraPos").transform.Find("AttackRange").gameObject;

        if (m_attackRangeObj)
        {
            m_attackRangeObj.SetActive(false);
        }

        MenuController.IsMove += IsMove;
    }

    void FixedUpdate()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        if (!m_canMove) return;
        Move();
        m_cameraPos.rotation = Camera.main.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        if (!m_canMove) return;

        m_input.x = Input.GetAxisRaw("Horizontal");
        m_input.y = Input.GetAxisRaw("Vertical");

        DoRotate();
        DoAnimation();

        if (Input.GetButton("Fire1"))
        {
            AttackSpray();
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            CancelAttackSpray();
            //m_HSACN.m_hit = false;
            //m_HSACN.m_crossHair.color = Color.white;
        }
    }

    private void OnDestroy()
    {
        EventSystem.Instance.Unsubscribe((EventSystem.ResetTransform)ResetPosition);
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

    void Move()
    {
        m_dir = Vector3.forward * m_input.y + Vector3.right * m_input.x;

        if (m_input != Vector2.zero)
        {
            //カメラが向いている方向を基準にキャラクターが動くように、入力のベクトルを変換する
            m_dir = Camera.main.transform.TransformDirection(m_dir);
            m_dir.y = 0;

            m_vel = m_dir.normalized * m_moveSpeed;
            m_vel.y = m_rb.velocity.y;
            m_rb.velocity = m_vel;
        }
        else
        {
            m_rb.velocity = Vector3.zero;
        }
    }

    //bool RayOfAttack()
    //{
    //    Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * m_maxRayDistance, Color.green);

    //    if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out m_hit, m_maxRayDistance))
    //    {
    //        if (m_hit.collider.tag == "Cockroach") return true;

    //        m_attackRangeObj.transform.position = new Vector3(m_hit.point.x, m_hit.point.y, m_hit.point.z);

    //        if (m_HSACN.m_hit)
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }
    //    else
    //    {
    //        m_attackRangeObj.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * m_maxRayDistance);
    //        return false;
    //    }
    //}

    void AttackSpray()
    {
        m_isBeamAttacking = true;

        if (m_attackRangeObj)
        {
            m_attackRangeObj.SetActive(true);
        }

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(ActiveSpray), RpcTarget.All);
        }
        else
        {
            ActiveSpray();
        }

        //if (RayOfAttack())
        //{
        //    if (m_hit.collider.gameObject.TryGetComponent(out Cockroach cockroachNetWork))
        //    {
        //        cockroachNetWork.BeAttacked(m_attackValue);
        //    }
        //}
    }

    void CancelAttackSpray()
    {
        if (m_attackRangeObj)
        {
            m_attackRangeObj.SetActive(false);
        }

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(UnActiveSpray), RpcTarget.All);
        }
        else
        {
            UnActiveSpray();
        }
    }

    [PunRPC]
    void ActiveSpray()
    {
        m_chargePrefab.SetActive(true);
    }

    [PunRPC]
    void UnActiveSpray()
    {
        StartCoroutine(StopBeam());
    }

    IEnumerator StopBeam()
    {
        m_chargePrefab.SetActive(false);
        m_beamPrefab.SetActive(true);
        yield return new WaitForSeconds(3f);
        m_beamPrefab.SetActive(false);
        m_isBeamAttacking = false;
    }

    void DoRotate()
    {
        //Playerの向きをカメラの向いている方向にする
        this.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, this.transform.rotation, m_turnSpeed * Time.deltaTime);
        //Playerが倒れないようにする
        this.transform.rotation = new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w);
    }

    void DoAnimation()
    {
        // とりあえずのアニメーション
        if (m_input.x != 0)
        {
            m_anim.SetFloat("Speed", Mathf.Abs(m_input.x));
        }
        else
        {
            m_anim.SetFloat("Speed", Mathf.Abs(m_input.y));
        }

    }

    void BeamIkWeight()
    {
        // IKアニメーションを滑らかにする処理
        if (m_isBeamAttacking)
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
        if (m_rightHandIKTarget == null || m_rightArmIKTarget == null || m_leftArmIKTarget == null) return;

        BeamIkWeight();

        if (m_anim)
        {
            if (m_isBeamAttacking)
            {
                // 右手
                m_anim.SetIKPosition(AvatarIKGoal.RightHand, m_rightHandIKTarget.position);
                m_anim.SetIKRotation(AvatarIKGoal.RightHand, m_rightHandIKTarget.rotation);
                m_anim.SetIKPositionWeight(AvatarIKGoal.RightHand, m_localRightHandIkWeight);
                m_anim.SetIKRotationWeight(AvatarIKGoal.RightHand, m_localRightHandIkWeight);
            }
            else
            {
                // 右腕
                m_anim.SetIKPosition(AvatarIKGoal.RightHand, m_rightArmIKTarget.position);
                m_anim.SetIKRotation(AvatarIKGoal.RightHand, m_rightArmIKTarget.rotation);
                m_anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
                m_anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
            }

            // 左腕
            m_anim.SetIKPosition(AvatarIKGoal.LeftHand, m_leftArmIKTarget.position);
            m_anim.SetIKRotation(AvatarIKGoal.LeftHand, m_leftArmIKTarget.rotation);
            m_anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            m_anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
        }
        else
        {
            Debug.LogError("m_anim が Null です");
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(m_isBeamAttacking);
            stream.SendNext(this.m_localRightHandIkWeight);
            stream.SendNext(this.m_rightHandIKTarget.position);
            stream.SendNext(this.m_rightHandIKTarget.rotation);
        }
        else
        {
            m_isBeamAttacking = (bool)stream.ReceiveNext();
            m_localRightHandIkWeight = (float)stream.ReceiveNext();
            m_rightHandIKTarget.position = (Vector3)stream.ReceiveNext();
            m_rightHandIKTarget.rotation = (Quaternion)stream.ReceiveNext();
        }
    }

    public void IsMove(bool isMove)
    {
        m_canMove = isMove;

        if (m_vcamBase)
        {
            m_vcamBase.enabled = isMove;
        }
    }
}