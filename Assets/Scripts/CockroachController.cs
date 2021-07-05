using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class CockroachController : MonoBehaviour
{
    /// <summary>移動速度</summary>
    [SerializeField] float _moveSpeed = 5f;
    /// <summary>ジャンプ力</summary>
    [SerializeField] float _jumpPower = 1f;
    /// <summary>方向転換の速度</summary>
    [SerializeField] float _turnSpeed = 5;
    /// <summary>重力</summary>
    [SerializeField] float _gravityPower = 1f;
    /// <summary>Rayを飛ばす距離</summary>
    [SerializeField] float _maxRayDistance = 1f;
    /// <summary>Rayを飛ばす始点</summary>
    [SerializeField] Transform _rayOriginPos = null;
    /// <summary>回転する時に判定するためのRayをとばす位置</summary>
    [SerializeField] Transform _rotateRayPos = null;
    /// <summary>Rigidbody</summary>
    Rigidbody _rb;
    /// <summary>Velocity</summary>
    Vector3 _velo;
    /// <summary>Direction</summary>
    Vector3 _dir;
    /// <summary>重力方向</summary>
    Vector3 _gravityDir;
    /// <summary>法線ベクトルを取得するための変数</summary>
    RaycastHit _rotateHit;

    Vector3 _jumpDir;
    public bool _changeing { get; private set; }
    bool _isGround = true;
    bool _isJump = false;
    [SerializeField] bool _isGravity = false;

    float _mouse_move_x;

    [SerializeField] float _mouseSensitivity = 1f; // いわゆるマウス感度

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.GetComponent<Rigidbody>().useGravity = false;
        _rb = this.gameObject.GetComponent<Rigidbody>();
        _gravityDir = Vector3.down;
        _isGravity = true;
        _changeing = false;
    }

    void FixedUpdate()
    {
        Move(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    // Update is called once per frame
    void Update()
    {
        Ray();
        Gravity();
        Jump(_jumpPower);
        FallForce();
        MouseMove();
    }

    void Move(float h, float v)
    {
        _dir = transform.forward;

        if (_isJump) return;

        if (v > 0) // 進む処理
        {
            _velo = _dir.normalized * _moveSpeed;

            if (_gravityDir == Vector3.up || _gravityDir == Vector3.down)
            {
                _velo.y = _rb.velocity.y;
            }
            else if (_gravityDir == Vector3.left || _gravityDir == Vector3.right)
            {
                _velo.x = _rb.velocity.x;
            }
            else if (_gravityDir == Vector3.forward || _gravityDir == Vector3.back)
            {
                _velo.z = _rb.velocity.z;
            }
        }
        else // 止まる処理
        {
            if (_gravityDir == Vector3.up || _gravityDir == Vector3.down)
            {
                _velo = new Vector3(0f, _rb.velocity.y, 0f);
            }
            else if (_gravityDir == Vector3.left || _gravityDir == Vector3.right)
            {
                _velo = new Vector3(_rb.velocity.x, 0f, 0f);
            }
            else if (_gravityDir == Vector3.forward || _gravityDir == Vector3.back)
            {
                _velo = new Vector3(0f, 0f, _rb.velocity.z);
            }
        }

        if (_changeing && !_isJump)
        {
            _velo = Vector3.zero;
        }

        _rb.velocity = _velo;

        //if (h != 0)
        //{
        //    transform.Rotate(new Vector3(0f, h * _turnSpeed, 0f));
        //}
    }

    private void MouseMove()
    {
        _mouse_move_x = Input.GetAxis("Mouse X") * _mouseSensitivity;
        transform.Rotate(new Vector3(0f, _mouse_move_x, 0f));
    }

    void Gravity()
    {
        if (_isGravity)
        {
            _rb.AddForce(_gravityDir * _gravityPower, ForceMode.Force);
        }
    }

    /// <summary>ジャンプする</summary>
    /// <param name="jumpPower">ジャンプする力</param>
    void Jump(float jumpPower)
    {
        if (Input.GetButtonDown("Jump") && _isGround)
        {
            _isJump = true;

            _isGround = false;

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
        if (_isJump && _gravityDir != Vector3.down)
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
        if (_isJump) return;

        if (isGround)
        {
            _isGround = true;
        }
        else
        {
            _isGround = false;
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
        _changeing = true;

        if (!_isJump) // 壁に上ったりする時だけ使う
        {
            Quaternion toRotate = Quaternion.FromToRotation(transform.up, nomal) * transform.rotation; // https://teratail.com/questions/290578
            transform.DORotateQuaternion(toRotate, 0.25f).OnComplete(() =>
            {
                // 回転した時にできた隙間を強制的に埋める
                _rb.AddForce(_gravityDir * _gravityPower, ForceMode.Impulse);
            });
        }

        yield return new WaitForSeconds(waitTime); // 回転する時間を稼ぐ

        if (_isJump)
        {
            Quaternion toRotate = Quaternion.FromToRotation(transform.up, nomal) * transform.rotation; // https://teratail.com/questions/290578
            transform.DORotateQuaternion(toRotate, 0.25f);
        }

        _changeing = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        _isJump = false;

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
        if (_isJump || _changeing) return;

        if (_rotateHit.collider)
        {
            if (_rotateHit.normal != -_gravityDir)
            {
                ChangeGravity(-_rotateHit.normal);
                StartCoroutine(ChangeRotate(_rotateHit.normal, 0.05f));
            }
        }

        if (!_isGround)
        {
            ChangeGravity(Vector3.down);
            StartCoroutine(ChangeRotate(Vector3.up, 0.05f));
        }
    }
}
