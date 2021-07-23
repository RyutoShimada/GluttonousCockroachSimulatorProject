using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Cinemachine;

namespace Photon.Pun.Demo.PunBasics
{
    [RequireComponent(typeof(Rigidbody))]
    public class HumanMoveControllerNetWork : MonoBehaviourPunCallbacks, IPunObservable
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

        [SerializeField] GameObject m_camera = null;
        [SerializeField] GameObject m_vcamPrefab = null;

        /// <summary>攻撃値</summary>
        [SerializeField] int m_attackValue = 20; // 後で別のスクリプトに移すこと

        /// <summary>IKで右手を移動させるターゲット</summary>
        [SerializeField] Transform m_rightHandIKTarget = null;
        /// <summary>IKを滑らかに実行する速度</summary>
        [SerializeField] float m_rightIKPositionWeightSpeed = 1f;
        /// <summary>攻撃時のスプレーのパーティクル</summary>
        [SerializeField] GameObject m_sprayParticle = null;
        /// <summary>実際に変化するのIKのアニメーション速度</summary>
        float m_localIkWeight = 0f;

        /// <summary>スクリプト</summary>
        [SerializeField] HumanSprayAttackRange m_HSAR = null;
        /// <summary>垂直方向と水平方向の入力を受け付ける</summary>
        Vector2 m_input;
        /// <summary>方向</summary>
        Vector3 m_dir;
        /// <summary>速度ベクト</summary>
        Vector3 m_vel;
        /// <summary>攻撃しているかどうか</summary>
        bool isSprayAttacking = false;
        Rigidbody m_rb;
        Animator m_anim;
        RaycastHit m_hit;

#if DEBUG
        /// <summary>IKの動きを調整するときに使う</summary>
        [SerializeField] bool isIKTest = false;
#endif

        private void Awake()
        {
            if (photonView.IsMine)
            {
                m_Instance = gameObject;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            m_anim = GetComponent<Animator>();

            if (photonView.IsMine)
            {
                m_camera.SetActive(true);

                m_rb = GetComponent<Rigidbody>();

                GameObject go = null;

                if (m_vcamPrefab)
                {
                    go = Instantiate(m_vcamPrefab, m_camera.transform.position, m_camera.transform.rotation);
                }
                else
                {
                    Debug.LogError("m_vcamPrefab がアサインされていません");
                }

                if (go.TryGetComponent(out CinemachineVirtualCamera vcam))
                {
                    vcam.Follow = m_camera.transform;
                }
                else
                {
                    Debug.LogError("CinemachineVirtualCamera を GetComponent 出来ませんでした");
                }

                m_attackRangeObj = transform.Find("Camera").transform.Find("AttackRange").gameObject;

                if (m_attackRangeObj)
                {
                    m_attackRangeObj.SetActive(false);
                }
            }
            else
            {
                m_camera.SetActive(false);
            }
        }

        void FixedUpdate()
        {
            if (photonView.IsMine)
            {
                Move();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (photonView.IsMine)
            {
                m_input.x = Input.GetAxisRaw("Horizontal");
                m_input.y = Input.GetAxisRaw("Vertical");

                DoRotate();
                DoAnimation();

                if (Input.GetButton("Fire1") || isIKTest)
                {
                    photonView.RPC(nameof(AttackSpray), RpcTarget.All);
                }
                else if (Input.GetButtonUp("Fire1") || !isIKTest)
                {
                    photonView.RPC(nameof(CancelAttackSpray), RpcTarget.All);
                }
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

        bool RayOfAttack()
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out m_hit, m_maxRayDistance))
            {
                Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * m_maxRayDistance, Color.green);

                if (m_hit.collider.gameObject.tag == "Cockroach" && m_HSAR.m_sprayHit)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        [PunRPC]
        void AttackSpray()
        {
            isSprayAttacking = true;

            if (m_attackRangeObj)
            {
                m_attackRangeObj.SetActive(true);
            }

            m_sprayParticle.SetActive(true);

            if (RayOfAttack())
            {
                if (m_hit.collider.gameObject.TryGetComponent(out CockroachNetWork cockroachNetWork))
                {
                    cockroachNetWork.BeAttacked(m_attackValue);
                }
            }
        }

        [PunRPC]
        void CancelAttackSpray()
        {
            isSprayAttacking = false;

            if (m_attackRangeObj)
            {
                m_attackRangeObj.SetActive(false);
            }

            m_sprayParticle.SetActive(false);
        }

        void DoRotate()
        {
            if (isIKTest) return;
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

        // IK を計算するためのコールバック
        private void OnAnimatorIK(int layerIndex)
        {
            //if (!photonView.IsMine) return;
            if (m_rightHandIKTarget == null) return;

            // IKアニメーションを滑らかにする処理
            if (isSprayAttacking)
            {
                if (m_localIkWeight < 1.0f)
                {
                    m_localIkWeight += m_rightIKPositionWeightSpeed * Time.deltaTime;
                }
                else
                {
                    m_localIkWeight = 1.0f;
                }
            }
            else
            {
                if (m_localIkWeight > 0f)
                {
                    m_localIkWeight -= m_rightIKPositionWeightSpeed * Time.deltaTime;
                }
                else
                {
                    m_localIkWeight = 0f;
                }
            }

            if (m_anim)
            {
                m_anim.SetIKPositionWeight(AvatarIKGoal.RightHand, m_localIkWeight);
                m_anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f); // Rotation は滑らかにしない
                m_anim.SetIKPosition(AvatarIKGoal.RightHand, m_rightHandIKTarget.position);
                m_anim.SetIKRotation(AvatarIKGoal.RightHand, m_rightHandIKTarget.rotation);
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
                stream.SendNext(this.m_localIkWeight);
                stream.SendNext(this.m_rightHandIKTarget.position);
                stream.SendNext(this.m_rightHandIKTarget.rotation);
            }
            else
            {
                this.m_localIkWeight = (float)stream.ReceiveNext();
                m_rightHandIKTarget.position = (Vector3)stream.ReceiveNext();
                m_rightHandIKTarget.rotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}