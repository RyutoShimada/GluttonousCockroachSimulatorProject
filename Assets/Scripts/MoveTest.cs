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
    Rigidbody _rb;
    Vector3 _gravityDir;
    Vector3 _velocity;
    Vector3 _direction;
    bool _isFall = false;
    bool _isground = false;

    Ray _forwardRay;
    Ray _downRay;
    RaycastHit _forwardHit;

    /// <summary>Jumpした時にRayを飛ばす距離</summary>
    const float _jumpRayDis = 1f;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.GetComponent<Rigidbody>().useGravity = false;
        _rb = this.gameObject.GetComponent<Rigidbody>();
        _gravityDir = Vector3.down;
        _forwardHit = new RaycastHit();
    }

    void FixedUpdate()
    {
        Move(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Jump(_jumpPower);
        IsFall();
        IsGround();
    }

    // Update is called once per frame
    void Update()
    {
        _downRay = new Ray(transform.position, _gravityDir);
        Physics.Raycast(_forwardRay, out _forwardHit, _maxRayDistance);
        Debug.DrawRay(_downRay.origin, _downRay.direction * _jumpRayDis, Color.blue, 0, false);
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
        if (Input.GetButtonDown("Jump") && _isground)
        {
            _rb.AddForce(-_gravityDir.normalized * jumpPower, ForceMode.Impulse);
        }
    }

    void IsGround()
    {
        if (Physics.Raycast(_downRay, _jumpRayDis))
        {
            _isground = true;
        }
        else
        {
            _isground = false;
            _gravityDir = Vector3.down;
        }
    }

    void IsFall()
    {
        if (_rb.velocity.y < 0 && !_isground)
        {
            _isFall = true;
            _gravityDir = Vector3.down;
            Vector3 v3 = _rb.velocity;
            v3.y = -_fallSpeed;
            _rb.velocity = v3;
        }
        else
        {
            _isFall = false;
        }
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
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if (collision.collider.tag == "Floor")
        //{
        //    ChangeRotate(Vector3.up);
        //}

        foreach (ContactPoint point in collision.contacts)
        {
            ChangeGravity(point.normal);
        }

        //if (!_forwardHit.collider) return;

        //if (_forwardHit.collider.name == collision.collider.name)
        //{
        //    ChangeGravity(_forwardHit.normal);
        //}
    }
}
