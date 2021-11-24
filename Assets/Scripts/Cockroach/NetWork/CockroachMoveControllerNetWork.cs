using System.Collections;
using UnityEngine;
using DG.Tweening;
using Photon.Pun;

/// <summary>
/// ゴキブリの動きに関するスクリプト
/// </summary>
public class CockroachMoveControllerNetWork : MonoBehaviourPunCallbacks, IIsCanMove
{
    #region Private Serializable Fields
    /// <summary>移動速度</summary>
    [SerializeField] float m_moveSpeed = 7f;
    /// <summary>現在の移動速度</summary>
    float m_currentMoveSpeed = 0;
    /// <summary>ジャンプ力</summary>
    [SerializeField] float m_jumpPower = 4f;
    /// <summary>現在のジャンプ力</summary>
    float m_currentJumpPower = 0;
    /// <summary>重力</summary>
    [SerializeField] float m_gravityPower = 10f;
    /// <summary>Rayを飛ばす距離</summary>
    [SerializeField] float m_maxRayDistance = 1f;
    /// <summary>Rayを飛ばす始点</summary>
    [SerializeField] Transform m_rayOriginPos = null;
    /// <summary>回転する時に判定するためのRayをとばす位置</summary>
    [SerializeField] Transform m_rotateRayPos = null;
    // <summary>マウスの感度</summary>
    [SerializeField, Range(1f, 100)] float m_mouseSensitivity = 50f;
    #endregion


    /// <summary>Rigidbody</summary>
    Rigidbody m_rb;
    /// <summary>Velocity</summary>
    Vector3 m_velo;
    /// <summary>Direction</summary>
    Vector3 m_dir;
    /// <summary>重力方向</summary>
    Vector3 m_gravityDir = Vector3.down;
    /// <summary>ジャンプする方向(床以外でのジャンプ時のみ)</summary>
    Vector3 m_jumpDir;
    /// <summary>回転する時に必要な法線ベクトルを取得するためのRay</summary>
    RaycastHit m_rotateHit;
    Animator m_anim;
    /// <summary>Vertical</summary>
    float m_v;
    /// <summary>マウスのX軸の動いた量</summary>
    float m_mouseMoveX;
    /// <summary>回転中かどうか</summary>
    bool m_isRotate = false;
    /// <summary>接地しているかどうか</summary>
    bool m_isGrounded = true;
    /// <summary>ジャンプ中かどうか</summary>
    bool m_isJumping = false;
    /// <summary>死んでいるかどうか</summary>
    bool m_isDed = false;

    [HideInInspector]
    public bool m_canMove = true;

    /// <summary>死んでいるかどうか</summary>
    public bool IsDed
    {
        get => m_isDed;

        set
        {
            m_isDed = value;
        }
    }

    void Start()
    {
        if (!photonView.IsMine) return;
        m_currentMoveSpeed = m_moveSpeed;
        m_currentJumpPower = m_jumpPower;

        try
        {
            m_rb = GetComponent<Rigidbody>();
            m_anim = GetComponent<Animator>();
        }
        catch (System.NullReferenceException)
        {
            Debug.LogError("オブジェクトがアサインされていません。", this);
            throw;
        }

        MenuController.IsMove += IsMove;
        m_rb.useGravity = false;
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        if (!m_canMove) return;
        if (m_isDed) return;
        Gravity();
        Move();
        FallForce();
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;
        if (!m_canMove) return;
        if (m_isDed) return;

        m_v = Input.GetAxisRaw("Vertical");
        Ray();
        Jump(m_currentJumpPower);
        MouseMove();
    }

    void Move()
    {
        m_dir = transform.forward;
        if (m_isJumping) return;

        if (m_v > 0) // 進む処理
        {
            m_velo = m_dir.normalized * m_currentMoveSpeed;

            if (m_gravityDir == Vector3.up || m_gravityDir == Vector3.down)
            {
                // 床か天井にいる時は、Y軸方向の速度を維持
                m_velo.y = m_rb.velocity.y;
            }
            else if (m_gravityDir == Vector3.left || m_gravityDir == Vector3.right)
            {
                // X軸の壁にいる時は、X軸方向の速度を維持
                m_velo.x = m_rb.velocity.x;
            }
            else if (m_gravityDir == Vector3.forward || m_gravityDir == Vector3.back)
            {
                // Z軸の壁にいる時は、Z軸方向の速度を維持
                m_velo.z = m_rb.velocity.z;
            }
        }
        else // 止まる処理
        {
            if (m_gravityDir == Vector3.up || m_gravityDir == Vector3.down)
            {
                // 床か天井にいる時は、Y軸方向の速度以外を0
                m_velo = new Vector3(0f, m_rb.velocity.y, 0f);
            }
            else if (m_gravityDir == Vector3.left || m_gravityDir == Vector3.right)
            {
                // X軸の壁にいる時は、X軸方向の速度以外を0
                m_velo = new Vector3(m_rb.velocity.x, 0f, 0f);
            }
            else if (m_gravityDir == Vector3.forward || m_gravityDir == Vector3.back)
            {
                // Z軸の壁にいる時は、Z軸方向の速度以外を0
                m_velo = new Vector3(0f, 0f, m_rb.velocity.z);
            }
        }

        if (!m_isJumping && m_isRotate)
        {
            m_velo *= 0.5f;
        }

        m_rb.velocity = m_velo;

        // アニメーション
        if (m_isGrounded)
        {
            m_anim.SetFloat("Velocity", m_v);
        }
        else
        {
            m_anim.SetFloat("Velocity", 0);
        }
    }

    private void MouseMove()
    {
        // キー入力とマウス入力の両方を受け付けている

        if (Input.GetAxis("Horizontal") != 0)
        {
            m_mouseMoveX = Input.GetAxis("Horizontal") * (m_mouseSensitivity * Time.deltaTime);
        }
        else if (Input.GetAxis("Look X") != 0)
        {
            m_mouseMoveX = Input.GetAxis("Look X") * (m_mouseSensitivity * Time.deltaTime);
        }
        else
        {
            m_mouseMoveX = 0;
        }
        
        transform.Rotate(new Vector3(0f, m_mouseMoveX, 0f));
    }

    void Gravity()
    {
        m_rb.AddForce(m_gravityDir * m_gravityPower, ForceMode.Force);
    }

    /// <summary>ジャンプする</summary>
    /// <param name="jumpPower">ジャンプする力</param>
    void Jump(float jumpPower)
    {
        if (Input.GetButtonDown("Jump") && m_isGrounded)
        {
            m_isJumping = true;
            m_isGrounded = false;
            m_rb.AddForce(-m_gravityDir.normalized * jumpPower, ForceMode.Impulse);

            if (m_gravityDir == Vector3.down) return;
            // ジャンプしたら落下する処理
            m_jumpDir = -m_gravityDir;
            ChangeGravity(Vector3.down);
            StartCoroutine(ChangeRotate(Vector3.up, 0.1f));
        }
    }

    void FallForce()
    {
        if (m_isJumping && m_gravityDir != Vector3.down) { m_rb.AddForce(m_jumpDir); }
    }

    /// <summary>
    /// 子オブジェクトのTriggerから呼ぶ
    /// </summary>
    /// <param name="isGround"></param>
    /// <returns>接地判定</returns>
    public void IsGround(bool isGround)
    {
        if (isGround)
        {
            m_isGrounded = true;
        }
        else
        {
            m_isGrounded = false;
        }
    }

    void Ray()
    {
        Physics.Raycast(m_rayOriginPos.position, m_rotateRayPos.position - m_rayOriginPos.position, out m_rotateHit, m_maxRayDistance);
        Debug.DrawRay(m_rayOriginPos.position, (m_rotateRayPos.position - m_rayOriginPos.position).normalized * m_maxRayDistance, Color.red);

        if (!m_rotateHit.collider)
        {
            m_rotateHit.normal = -Vector3.down;
        }
    }

    void ChangeGravity(Vector3 nomal) => m_gravityDir = nomal;

    IEnumerator ChangeRotate(Vector3 nomal, float waitTime)
    {
        if (m_isRotate) yield return null;

        m_isRotate = true;

        if (!m_isJumping) // 壁に上ったりする時だけ使う
        {
            // https://teratail.com/questions/290578
            Quaternion toRotate = Quaternion.FromToRotation(transform.up, nomal) * transform.rotation;
            transform.DORotateQuaternion(toRotate, 0.25f).OnComplete(() =>
            {
                // 回転した時にできた隙間を強制的に埋める
                m_rb.AddForce(m_gravityDir * m_gravityPower, ForceMode.Impulse);
            });
        }
        else
        {
            Quaternion toRotate = Quaternion.FromToRotation(transform.up, nomal) * transform.rotation;
            transform.DORotateQuaternion(toRotate, 0.25f);
        }

        yield return new WaitForSeconds(waitTime); // 回転する時間を稼ぐ

        m_isRotate = false;
    }

    public void IsMove(bool isMove) => m_canMove = isMove;

    /// <summary>
    /// cockroachから呼ばれる
    /// </summary>
    /// <param name="addSpeed">加算する速度</param>
    /// <param name="addJump">加算するジャンプ力</param>
    public void InvincibleMode(bool isMode, float addSpeed, float addJump)
    {
        if (isMode)
        {
            m_currentMoveSpeed += addSpeed;
            m_currentJumpPower += addJump;
        }
        else
        {
            m_currentMoveSpeed = m_moveSpeed;
            m_currentJumpPower = m_jumpPower;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        m_isJumping = false;
        m_jumpDir = Vector3.zero;

        // 衝突したオブジェクトの法線ベクトルを元に、重力方向を決める
        foreach (ContactPoint point in collision.contacts)
        {
            if (m_gravityDir == Vector3.down && point.normal == Vector3.up) return;
            ChangeGravity(-point.normal);
            StartCoroutine(ChangeRotate(point.normal, 0.5f));
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (m_isJumping || m_isRotate) return;

        if (!m_rotateHit.collider) { m_rotateHit.normal = -Vector3.down; }

        // 回転するためのRayが当たっているオブジェクトの法線ベクトルと、
        // 現在の重力方向が違っていたら
        // 重力方向をRayが当たっている方向に変え、自身を回転させる
        if (m_rotateHit.normal != -m_gravityDir)
        {
            ChangeGravity(-m_rotateHit.normal);
            StartCoroutine(ChangeRotate(m_rotateHit.normal, 0.5f));
        }
    }
}
