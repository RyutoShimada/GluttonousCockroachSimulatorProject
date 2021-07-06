using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class CockroachMoveController : MonoBehaviour
{
    /// <summary>移動速度</summary>
    [SerializeField] float _moveSpeed = 7f;
    /// <summary>ジャンプ力</summary>
    [SerializeField] float _jumpPower = 4f;
    /// <summary>重力</summary>
    [SerializeField] float _gravityPower = 10f;
    /// <summary>Rayを飛ばす距離</summary>
    [SerializeField] float _maxRayDistance = 1f;
    /// <summary>Rayを飛ばす始点</summary>
    [SerializeField] Transform _rayOriginPos = null;
    /// <summary>回転する時に判定するためのRayをとばす位置</summary>
    [SerializeField] Transform _rotateRayPos = null;
    /// <summary>マウスの感度</summary>
    [SerializeField] float _mouseSensitivity = 5f;
    /// <summary>Rigidbody</summary>
    Rigidbody _rb;
    /// <summary>Velocity</summary>
    Vector3 _velo;
    /// <summary>Direction</summary>
    Vector3 _dir;
    /// <summary>重力方向</summary>
    Vector3 _gravityDir = Vector3.down;
    /// <summary>ジャンプする方向(床以外でのジャンプ時のみ)</summary>
    Vector3 _jumpDir;
    /// <summary>回転する時に必要な法線ベクトルを取得するためのRay</summary>
    RaycastHit _rotateHit;
    /// <summary>Vertical</summary>
    float _v;
    /// <summary>マウスのX軸の動いた量</summary>
    float _mouseMoveX;
    /// <summary>回転中かどうか</summary>
    bool _isRotate = false;
    /// <summary>接地しているかどうか</summary>
    bool _isGrounded = true;
    /// <summary>ジャンプ中かどうか</summary>
    bool _isJumping = false;

    void Start()
    {
        this.gameObject.GetComponent<Rigidbody>().useGravity = false;
        _rb = this.gameObject.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Gravity();
        Move();
        FallForce();
    }

    // Update is called once per frame
    void Update()
    {
        _v = Input.GetAxisRaw("Vertical");
        Ray();
        Jump(_jumpPower);
        MouseMove();
    }

    /// <summary>
    /// 移動
    /// </summary>
    void Move()
    {
        _dir = transform.forward;

        if (_isJumping) return;

        if (_v > 0) // 進む処理
        {
            _velo = _dir.normalized * _moveSpeed;

            if (_gravityDir == Vector3.up || _gravityDir == Vector3.down)
            {
                // 床か天井にいる時は、Y軸方向の速度を維持
                _velo.y = _rb.velocity.y;
            }
            else if (_gravityDir == Vector3.left || _gravityDir == Vector3.right)
            {
                // X軸の壁にいる時は、X軸方向の速度を維持
                _velo.x = _rb.velocity.x;
            }
            else if (_gravityDir == Vector3.forward || _gravityDir == Vector3.back)
            {
                // Z軸の壁にいる時は、Z軸方向の速度を維持
                _velo.z = _rb.velocity.z;
            }
        }
        else // 止まる処理
        {
            if (_gravityDir == Vector3.up || _gravityDir == Vector3.down)
            {
                // 床か天井にいる時は、Y軸方向の速度以外を0
                _velo = new Vector3(0f, _rb.velocity.y, 0f);
            }
            else if (_gravityDir == Vector3.left || _gravityDir == Vector3.right)
            {
                // X軸の壁にいる時は、X軸方向の速度以外を0
                _velo = new Vector3(_rb.velocity.x, 0f, 0f);
            }
            else if (_gravityDir == Vector3.forward || _gravityDir == Vector3.back)
            {
                // Z軸の壁にいる時は、Z軸方向の速度以外を0
                _velo = new Vector3(0f, 0f, _rb.velocity.z);
            }
        }

        if (_isRotate && !_isJumping)
        {
            // 回転中かつジャンプ中ではないとき
            _velo = Vector3.zero;
        }

        _rb.velocity = _velo;
    }

    private void MouseMove()
    {
        _mouseMoveX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        transform.Rotate(new Vector3(0f, _mouseMoveX, 0f));
    }

    void Gravity()
    {
        _rb.AddForce(_gravityDir * _gravityPower, ForceMode.Force);
    }

    /// <summary>ジャンプする</summary>
    /// <param name="jumpPower">ジャンプする力</param>
    void Jump(float jumpPower)
    {
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _isJumping = true;

            _isGrounded = false;

            _rb.AddForce(-_gravityDir.normalized * jumpPower, ForceMode.Impulse);

            if (_gravityDir != Vector3.down)
            {
                _jumpDir = -_gravityDir;
                ChangeGravity(Vector3.down);
                StartCoroutine(ChangeRotate(Vector3.up, 0.1f));
            }
        }
    }

    void FallForce()
    {
        if (_isJumping && _gravityDir != Vector3.down)
        {
            _rb.AddForce(_jumpDir);
        }
    }

    /// <summary>
    /// 子オブジェクトのTriggerから呼ぶ
    /// </summary>
    /// <param name="isGround"></param>
    /// <returns></returns>
    public void IsGround(bool isGround)
    {
        //if (_isJump) return;

        if (isGround)
        {
            _isGrounded = true;
        }
        else
        {
            _isGrounded = false;
        }
    }

    void Ray()
    {
        Physics.Raycast(_rayOriginPos.position, _rotateRayPos.position - _rayOriginPos.position, out _rotateHit, _maxRayDistance);
        Debug.DrawRay(_rayOriginPos.position, (_rotateRayPos.position - _rayOriginPos.position).normalized * _maxRayDistance, Color.green);
    }

    void ChangeGravity(Vector3 nomal)
    {
        _gravityDir = nomal;
    }

    IEnumerator ChangeRotate(Vector3 nomal, float waitTime)
    {
        _isRotate = true;

        if (!_isJumping) // 壁に上ったりする時だけ使う
        {
            Quaternion toRotate = Quaternion.FromToRotation(transform.up, nomal) * transform.rotation; // https://teratail.com/questions/290578
            transform.DORotateQuaternion(toRotate, 0.25f).OnComplete(() =>
            {
                // 回転した時にできた隙間を強制的に埋める
                _rb.AddForce(_gravityDir * _gravityPower, ForceMode.Impulse);
            });
        }

        yield return new WaitForSeconds(waitTime); // 回転する時間を稼ぐ

        if (_isJumping)
        {
            Quaternion toRotate = Quaternion.FromToRotation(transform.up, nomal) * transform.rotation; // https://teratail.com/questions/290578
            transform.DORotateQuaternion(toRotate, 0.25f);
        }

        _isRotate = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        _isJumping = false;

        _jumpDir = Vector3.zero; // 着地したらジャンプする方向をリセット

        foreach (ContactPoint point in collision.contacts)
        {
            if (_gravityDir == Vector3.down && point.normal == Vector3.up)
            {
                return;
            }
            else
            {
                ChangeGravity(-point.normal);
                StartCoroutine(ChangeRotate(point.normal, 0.05f));
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (_isJumping || _isRotate) return;

        if (_rotateHit.collider)
        {
            if (_rotateHit.normal != -_gravityDir)
            {
                ChangeGravity(-_rotateHit.normal);
                StartCoroutine(ChangeRotate(_rotateHit.normal, 0.05f));
            }
        }

        if (!_isGrounded)
        {
            ChangeGravity(Vector3.down);
            StartCoroutine(ChangeRotate(Vector3.up, 0.05f));
        }
    }
}
