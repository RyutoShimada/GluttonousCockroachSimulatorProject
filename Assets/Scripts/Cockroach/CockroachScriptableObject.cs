using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/CockroachData")]
public class CockroachScriptableObject : ScriptableObject
{
    /// <summary>最大の体力値</summary>
    [SerializeField] int m_maxHp = 100;
    /// <summary>移動速度</summary>
    [SerializeField] float m_moveSpeed = 7f;
    /// <summary>ジャンプ力</summary>
    [SerializeField] float m_jumpPower = 4f;

    [SerializeField] float m_minMoveTime = 0.5f;
    [SerializeField] float m_maxMoveTime = 5f;
    [SerializeField] float m_afterMoveWaitTime = 2f;
    [SerializeField] float m_leftRotateRange = -10;
    [SerializeField] float m_rightRotateRange = 10f;
    [SerializeField] float m_minJumpTime = 1f;
    [SerializeField] float m_maxJumpTime = 5f;

    public int MaxHP { get => m_maxHp; }
    public float Speed { get => m_moveSpeed; }
    public float JumpPower { get => m_jumpPower; }
    public float MinMoveTime { get => m_minMoveTime; }
    public float MaxMoveTime { get => m_maxMoveTime; }
    public float AftorMoveWaitTime { get => m_afterMoveWaitTime; }
    public float LeftRotateRange { get => m_leftRotateRange; }
    public float RightRotateRange { get => m_rightRotateRange; }
    public float MinJumpTime { get => m_minJumpTime; }
    public float MaxJumpTime { get => m_maxJumpTime; }
}
