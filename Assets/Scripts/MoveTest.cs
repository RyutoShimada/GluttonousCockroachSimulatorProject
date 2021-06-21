using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MoveTest : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 5f;
    [SerializeField] float _jumpPower = 1f;
    [SerializeField] float _turnSpeed = 5;
    [SerializeField] float _fallSpeed = 5;
    [SerializeField] float _gravityPower = 1f;
    [SerializeField] float _maxRayDistance = 1f;
    [SerializeField] Transform _rayOriginPos = null;
    [SerializeField] Transform _rotateRayPos = null;
    Rigidbody _rb;
    Vector3 _gravityDir;
    Vector3 _velocity;
    Vector3 _direction;
    RaycastHit _downHit;

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

        _direction = transform.forward;

        if (v > 0)
        {
            _velocity = _direction.normalized * _moveSpeed;

            if (_gravityDir == Vector3.up || _gravityDir == Vector3.down)
            {
                _velocity.y = _rb.velocity.y;
            }
            else if (_gravityDir == Vector3.left || _gravityDir == Vector3.right)
            {
                _velocity.x = _rb.velocity.x;
            }
            else if (_gravityDir == Vector3.forward || _gravityDir == Vector3.down)
            {
                _velocity.z = _rb.velocity.z;
            }

            _rb.velocity = _velocity;
        }
        else if (v <= 0)
        {
            //ピタッと止まるようにする
            if (_gravityDir == Vector3.up || _gravityDir == Vector3.down)
            {
                _velocity = new Vector3(0, _rb.velocity.y, 0);
            }
            else
            {
                _velocity = Vector3.zero;
            }

            _rb.velocity = _velocity;
        }

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
        if (Physics.Raycast(transform.position, _gravityDir, _jumpRayDis))
        {
            return  true;
        }
        else
        {
            _gravityDir = Vector3.down;
            return  false;
        }
    }

    void IsFall()
    {
        if (_rb.velocity.y < 0 && !IsGround())
        {
            _gravityDir = Vector3.down;
            _rb.AddForce(_gravityDir * _fallSpeed, ForceMode.Force);
        }
    }

    void Ray()
    {
        Physics.Raycast(_rayOriginPos.position, _rotateRayPos.position - _rayOriginPos.position, out _downHit, _maxRayDistance);
        Debug.DrawRay(_rayOriginPos.position, (_rotateRayPos.position - _rayOriginPos.position).normalized * _maxRayDistance * 2, Color.green, 0, false);
        //Debug.Log(_downHit.collider.name);
    }

    void ChangeGravity(Vector3 nomal)
    {
        _gravityDir = -nomal;
        ChangeRotate(nomal);
    }

    void ChangeRotate(Vector3 nomal)
    {
        //https://teratail.com/questions/290578
        Quaternion toRotate = Quaternion.FromToRotation(transform.up, nomal) * transform.rotation;
        transform.rotation = toRotate;
        _rb.AddForce(_gravityDir * _gravityPower, ForceMode.Impulse);
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
        try
        {
            if (_downHit.normal != -_gravityDir)
            {
                ChangeGravity(_downHit.normal);
            }
        }
        catch (System.NullReferenceException)
        {
            Debug.Log("Null");
            ChangeRotate(transform.forward);
        }
    }
}
