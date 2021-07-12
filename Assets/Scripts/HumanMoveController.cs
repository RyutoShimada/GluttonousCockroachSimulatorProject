using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class HumanMoveController : MonoBehaviour
{
    [SerializeField] float m_moveSpeed = 1f;
    [SerializeField] float m_turnSpeed = 1f;
    [SerializeField] Transform m_rayOrigin = null;
    [SerializeField] GameObject m_attackRangeObj = null;
    [SerializeField] float m_maxRayDistance = 1f;

    [SerializeField] Transform m_rightHandIKTarget = null;
    [SerializeField] float m_rightIKPositionWeightSpeed = 1f;
    float m_IKWeight = 0f;

    [SerializeField] bool isIKTest = false;
    Rigidbody m_rb;
    Vector2 m_input;
    Vector3 m_dir;
    Vector3 m_vel;
    Animator m_anim;
    RaycastHit m_hit;

    bool isSprayAttacking = false;

    // Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        m_anim = GetComponent<Animator>();
        m_attackRangeObj.SetActive(false);
    }

    void FixedUpdate()
    {
        Move();
    }

    // Update is called once per frame
    void Update()
    {
        m_input.x = Input.GetAxisRaw("Horizontal");
        m_input.y = Input.GetAxisRaw("Vertical");
        Attack();
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

            m_vel = m_dir.normalized * m_moveSpeed;
            m_vel.y = m_rb.velocity.y;
            m_rb.velocity = m_vel;
        }
        else
        {
            m_rb.velocity = Vector3.zero;
        }
    }

    void RayOfAttack()
    {
        if (Physics.Raycast(m_rayOrigin.position, Camera.main.transform.forward, out m_hit, m_maxRayDistance))
        {
            Debug.DrawRay(m_rayOrigin.position, Camera.main.transform.forward * m_maxRayDistance, Color.green);
            //Debug.Log($"HitDistance : {Vector3.Distance(m_rayOrigin.position, m_hit.point)}");
        }
    }

    void Attack()
    {
        m_attackRangeObj.transform.rotation = Camera.main.transform.rotation;

        if (Input.GetButton("Fire1") || isIKTest)
        {
            RayOfAttack();
            m_attackRangeObj.SetActive(true);
            isSprayAttacking = true;
        }
        else if (Input.GetButtonUp("Fire1") || !isIKTest)
        {
            m_attackRangeObj.SetActive(false);
            isSprayAttacking = false;
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
        m_anim.SetFloat("Speed", Mathf.Abs(m_input.x));
        m_anim.SetFloat("Speed", Mathf.Abs(m_input.y));
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
