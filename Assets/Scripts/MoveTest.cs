using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MoveTest : MonoBehaviour
{
    /// <summary>移動速度</summary>
    [SerializeField] float _moveSpeed = 5f;
    /// <summary>ジャンプ力</summary>
    [SerializeField] float _jumpPower = 1f;
    /// <summary>方向転換の速度</summary>
    [SerializeField] float _turnSpeed = 5;
    /// <summary>落下速度</summary>
    [SerializeField] float _fallSpeed = 5;
    /// <summary>重力</summary>
    [SerializeField] float _gravityPower = 1f;
    /// <summary>Rayを飛ばす距離</summary>
    [SerializeField] float _maxRayDistance = 1f;
    /// <summary>Rayを飛ばす始点</summary>
    [SerializeField] Transform _rayOriginPos = null;
    /// <summary>回転する時に判定するためのRayをとばす位置</summary>
    [SerializeField] Transform _rotateRayPos = null;
    /// <summary>接地判定する時に判定するためのRayをとばす位置</summary>
    [SerializeField] Transform _groundRayOriginPos = null;
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
    bool _changeing = false;

    /// <summary>Jumpした時にRayを飛ばす距離</summary>
    const float _jumpRayDis = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.GetComponent<Rigidbody>().useGravity = false;
        _rb = this.gameObject.GetComponent<Rigidbody>();
        _gravityDir = Vector3.down;
    }

    void FixedUpdate()
    {
        Move(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    // Update is called once per frame
    void Update()
    {
        Ray();
        Jump(_jumpPower);
        IsFall();
    }

    void Move(float h, float v)
    {
        _rb.AddForce(_gravityDir * _gravityPower, ForceMode.Force);//重力

        _dir = transform.forward;

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
            else if (_gravityDir == Vector3.forward || _gravityDir == Vector3.down)
            {
                _velo.z = _rb.velocity.z;
            }
        }
        else // 止まる処理
        {
            if (_gravityDir == Vector3.up || _gravityDir == Vector3.down)
            {
                _velo = new Vector3(0, _rb.velocity.y, 0);
            }
            else if (_gravityDir == Vector3.left || _gravityDir == Vector3.right)
            {
                _velo = new Vector3(_rb.velocity.x, 0, 0);
            }
            else if (_gravityDir == Vector3.forward || _gravityDir == Vector3.down)
            {
                _velo = new Vector3(0, 0, _rb.velocity.z);
            }
        }

        if (_changeing)
        {
            _velo = Vector3.zero;
        }

        _rb.velocity = _velo;

        if (h != 0)
        {
            transform.Rotate(new Vector3(0f, h * _turnSpeed, 0f));
        }
    }

    /// <summary>ジャンプする</summary>
    /// <param name="jumpPower">ジャンプする力</param>
    void Jump(float jumpPower)
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (IsGround())
            {
                _rb.AddForce(-_gravityDir.normalized * jumpPower, ForceMode.Impulse);
            }
        }
    }

    bool IsGround()
    {
        if (Physics.Raycast(_rayOriginPos.position, _groundRayOriginPos.position - _rayOriginPos.position, _maxRayDistance))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void IsFall()
    {
        if (!IsGround())
        {
            _gravityDir = Vector3.down;
            _rb.AddForce(_gravityDir * _fallSpeed, ForceMode.Force);
        }
    }

    void Ray()
    {
        Physics.Raycast(_rayOriginPos.position, _rotateRayPos.position - _rayOriginPos.position, out _rotateHit, _maxRayDistance);
        Debug.DrawRay(_rayOriginPos.position, (_rotateRayPos.position - _rayOriginPos.position).normalized * _maxRayDistance, Color.green, 0, false);
        Debug.DrawRay(_rayOriginPos.position, (_groundRayOriginPos.position - _rayOriginPos.position).normalized * _maxRayDistance, Color.blue, 0, false);
        //Debug.Log(_downHit.collider.name);
    }

    void ChangeGravity(Vector3 nomal)
    {
        _gravityDir = -nomal;
        _changeing = true;
        StartCoroutine(ChangeRotate(nomal));
    }

    IEnumerator ChangeRotate(Vector3 nomal)
    {
        //https://teratail.com/questions/290578
        Quaternion toRotate = Quaternion.FromToRotation(transform.up, nomal) * transform.rotation;
        transform.rotation = toRotate;
        _rb.AddForce(_gravityDir * _gravityPower, ForceMode.Impulse);
        yield return new WaitForSeconds(0.025f);
        _changeing = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint point in collision.contacts)
        {
            ChangeGravity(point.normal);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (_rotateHit.collider)
        {
            if (_rotateHit.normal != -_gravityDir)
            {
                ChangeGravity(_rotateHit.normal);
            }
        }
        else
        {
            ChangeRotate(transform.forward);
        }
    }
}
