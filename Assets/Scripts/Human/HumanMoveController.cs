using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class HumanMoveController : MonoBehaviour
{
    /// <summary>移動速度</summary>
    [SerializeField] float m_moveSpeed = 1f;
    /// <summary>体を回転する速度</summary>
    [SerializeField] float m_turnSpeed = 1f;
    /// <summary>攻撃する範囲のオブジェクト</summary>
    [SerializeField] GameObject m_attackRangeObj = null;
    /// <summary>Rayを飛ばす最大距離</summary>
    [SerializeField] float m_maxRayDistance = 1f;

    /// <summary>攻撃値</summary>
    [SerializeField] int m_attackValue = 20; // 後で別のスクリプトに移すこと

    /// <summary>IKで右手を移動させるターゲット</summary>
    [SerializeField] Transform m_rightHandIKTarget = null;
    /// <summary>IKを滑らかに実行する速度</summary>
    [SerializeField] float m_rightIKPositionWeightSpeed = 1f;
    /// <summary>攻撃時のスプレーのパーティクル</summary>
    [SerializeField] GameObject m_sprayParticle = null;
    /// <summary>実際に変化するのIKのアニメーション速度</summary>
    float m_IKWeight = 0f;

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


    /// <summary>IKの動きを調整するときに使う</summary>
    [SerializeField] bool isIKTest = false;
    /// <summary>動けるかどうか</summary>
    bool m_isCanMove = true;
    /// <summary>動けるかどうか</summary>
    public bool IsCanMove
    {
        get => m_isCanMove;

        set
        {
            m_isCanMove = value;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_anim = GetComponent<Animator>();
        m_attackRangeObj.SetActive(false);
    }

    void FixedUpdate()
    {
        if (!m_isCanMove) return;
        Move();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_isCanMove) return;
        m_input.x = Input.GetAxisRaw("Horizontal");
        m_input.y = Input.GetAxisRaw("Vertical");
        AttackSpray();
        DoRotate();
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

    bool RayOfAttack()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * m_maxRayDistance, Color.green);

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out m_hit, m_maxRayDistance))
        {
            m_attackRangeObj.transform.position = new Vector3(m_hit.point.x, m_hit.point.y, m_hit.point.z);

            if (m_HSAR.m_sprayHit)
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
            m_attackRangeObj.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * m_maxRayDistance);
            return false;
        }
    }

    void AttackSpray()
    {
        m_attackRangeObj.transform.rotation = Camera.main.transform.rotation;

        if (Input.GetButton("Fire1") || isIKTest)
        {
            isSprayAttacking = true;
            m_sprayParticle.SetActive(true);
            m_attackRangeObj.SetActive(true);

            if (RayOfAttack())
            {
                m_hit.collider.gameObject.GetComponent<Cockroach>()?.BeAttacked(m_attackValue);
            }
        }
        else if (Input.GetButtonUp("Fire1") || !isIKTest)
        {
            m_HSAR.m_crossHair.color = Color.white;
            isSprayAttacking = false;
            m_attackRangeObj.SetActive(false);
            m_sprayParticle.SetActive(false);
        }
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
        if (m_rightHandIKTarget == null) return;

        if (isSprayAttacking)
        {
            if (m_IKWeight < 1.0f)
            {
                m_IKWeight += m_rightIKPositionWeightSpeed * Time.deltaTime;
            }
            else
            {
                m_IKWeight = 1.0f;
            }
        }
        else
        {
            if (m_IKWeight > 0f)
            {
                m_IKWeight -= m_rightIKPositionWeightSpeed * Time.deltaTime;
            }
            else
            {
                m_IKWeight = 0f;
            }
        }

        m_anim.SetIKPositionWeight(AvatarIKGoal.RightHand, m_IKWeight);
        m_anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
        m_anim.SetIKPosition(AvatarIKGoal.RightHand, m_rightHandIKTarget.position);
        m_anim.SetIKRotation(AvatarIKGoal.RightHand, m_rightHandIKTarget.rotation);

    }
}
