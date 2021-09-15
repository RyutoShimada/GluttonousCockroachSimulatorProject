using System.Collections;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class HumanMoveController : MonoBehaviourPunCallbacks, IPunObservable, IIsCanMove
{
    static public GameObject m_Instance;

    /// <summary>移動速度</summary>
    [SerializeField] float m_moveSpeed = 1f;
    /// <summary>体を回転する速度</summary>
    [SerializeField] float m_turnSpeed = 1f;
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
    /// <summary>実際に変化するのIKのアニメーション速度</summary>
    float m_localRightHandIkWeight = 0f;

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

    
    [HideInInspector]
    public bool m_canMove = true;

    Transform m_leftArmOriginPos = null;
    Transform m_rightArmOriginPos = null;

    void Start()
    {
        m_anim = GetComponent<Animator>();
        m_leftArmOriginPos = m_leftArmIKTarget;
        m_rightArmOriginPos = m_rightArmIKTarget;

        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;

        m_rb = GetComponent<Rigidbody>();
        MenuController.IsMove += IsMove;
    }

    void FixedUpdate()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        if (!m_canMove) return;
        Move();
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsConnected && !photonView.IsMine) return;
        if (!m_canMove) return;

        m_input.x = Input.GetAxisRaw("Horizontal");
        m_input.y = Input.GetAxisRaw("Vertical");

        DoRotate();
        
        if (Input.GetButton("Fire1"))
        {
            Attack();
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            StopAttack();
        }
    }

    void LateUpdate()
    {
        DoAnimation();
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

    void Attack()
    {
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(StartCharge), RpcTarget.All);
        }
        else
        {
            StartCharge();
            //m_leftArmIKTarget = m_punchIKTarget;
            //m_rightArmIKTarget = m_punchIKTarget;
        }
    }

    void StopAttack()
    {
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(StopCharge), RpcTarget.All);
        }
        else
        {
            if (m_isBeamAttacking)
            {
                StopCharge();
            }
            
            //m_leftArmIKTarget = m_leftArmOriginPos;
            //m_rightArmIKTarget = m_rightArmOriginPos;
        }
    }

    [PunRPC]
    void StartCharge()
    {
        m_isBeamAttacking = true;
        m_chargePrefab.SetActive(true);
    }

    [PunRPC]
    void StopCharge()
    {
        StartCoroutine(Beam());
    }

    IEnumerator Beam()
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
            // 右腕
            m_anim.SetIKPosition(AvatarIKGoal.RightHand, m_rightArmIKTarget.position);
            m_anim.SetIKRotation(AvatarIKGoal.RightHand, m_rightArmIKTarget.rotation);
            m_anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            m_anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);

            // 左腕
            m_anim.SetIKPosition(AvatarIKGoal.LeftHand, m_leftArmIKTarget.position);
            m_anim.SetIKRotation(AvatarIKGoal.LeftHand, m_leftArmIKTarget.rotation);
            m_anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            m_anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);

            if (!m_isBeamAttacking) return;
            // ビームの時の手
            m_anim.SetIKPosition(AvatarIKGoal.RightHand, m_rightHandIKTarget.position);
            m_anim.SetIKRotation(AvatarIKGoal.RightHand, m_rightHandIKTarget.rotation);
            m_anim.SetIKPositionWeight(AvatarIKGoal.RightHand, m_localRightHandIkWeight);
            m_anim.SetIKRotationWeight(AvatarIKGoal.RightHand, m_localRightHandIkWeight);
        }
        else
        {
            Debug.LogError("m_anim が Null です");
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && photonView.IsMine)
        {
            stream.SendNext(m_isBeamAttacking);
            
            if (m_isBeamAttacking)
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
        else if(stream.IsReading && !photonView.IsMine)
        {
            m_isBeamAttacking = (bool)stream.ReceiveNext();
            
            if (m_isBeamAttacking)
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

    public void IsMove(bool isMove) => m_canMove = isMove;
}