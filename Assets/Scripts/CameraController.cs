using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float _sensitivity = 1f; // いわゆるマウス感度
    [SerializeField] float _mouseYMaxRange = 10f;
    [SerializeField] float _mouseYMinRange = -10f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float mouse_move_y = Input.GetAxis("Mouse Y") * _sensitivity;

        transform.Rotate(new Vector3(-mouse_move_y, 0f, 0f));

        if (transform.localEulerAngles.x < _mouseYMaxRange && transform.localEulerAngles.x > 90)
        {
            Vector3 v3 = transform.localEulerAngles;
            v3.x = _mouseYMaxRange;
            transform.localEulerAngles = v3;
        }
        
        if (transform.localEulerAngles.x > _mouseYMinRange && transform.localEulerAngles.x < 90)
        {
            Vector3 v3 = transform.localEulerAngles;
            v3.x = _mouseYMinRange;
            transform.localEulerAngles = v3;
        }
    }
}
