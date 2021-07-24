﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun.Demo.PunBasics
{
    [RequireComponent(typeof(CockroachMoveControllerNetWork))]
    [RequireComponent(typeof(CockroachUINetWork))]

    /// <summary>
    /// ゴキブリのスクリプト
    /// </summary>
    public class CockroachNetWork : MonoBehaviourPunCallbacks, IPunObservable
    {
        static public GameObject m_Instance;

        /// <summary>最大の体力値</summary>
        [SerializeField] int m_maxSatietyGauge = 100;
        /// <summary>満腹ゲージ</summary>
        [SerializeField] int m_satietyGauge = 100;
        /// <summary>満腹ゲージが1秒間に減少する値</summary>
        [SerializeField] int m_decreaseValueIn1second = 1;
        /// <summary>最大の体力値</summary>
        [SerializeField] int m_maxHp = 100;
        /// <summary>現在の体力値</summary>
        [SerializeField] int m_hp = 100;
        /// <summary>無敵モード時間</summary>
        [SerializeField] float m_invincibleModeTime = 3f;
        /// <summary>加算する速度</summary>
        [SerializeField] float m_addSpeedValue = 5f;
        /// <summary>加算するジャンプ力</summary>
        [SerializeField] float m_addJumpValue = 5f;
        /// <summary>無敵モード</summary>
        [SerializeField] bool m_invincibleMode = false;

        [SerializeField] GameObject m_camera = null;

        CockroachMoveControllerNetWork m_cockroachMoveControllerNetWork = null;
        CockroachUINetWork m_cockroachUINetWork = null;

        /// <summary>1秒間を測るためのタイマー</summary>
        float m_oneSecondTimer = 0f;
        /// <summary>死んだかどうか</summary>
        public bool m_isDed = false;

        private void Awake()
        {
            if (photonView.IsMine)
            {
                m_Instance = gameObject;
            }
        }

        private void Start()
        {
            m_cockroachMoveControllerNetWork = GetComponent<CockroachMoveControllerNetWork>();
            m_cockroachUINetWork = GetComponent<CockroachUINetWork>();

            if (photonView.IsMine)
            {
                m_camera.SetActive(true);

                m_isDed = false;
                m_satietyGauge = m_maxSatietyGauge;
                m_hp = m_maxHp;
            }
            else
            {
                m_camera.SetActive(false);
            }
        }

        private void Update()
        {
            if (photonView.IsMine)
            {
                if (m_isDed) return;
                if (!NetWorkGameManager.m_Instance.m_isGame) return;
                photonView.RPC(nameof(DecreaseHitPoint), RpcTarget.All, m_decreaseValueIn1second);
            }
        }

        [PunRPC]
        void CheckAlive()
        {
            if (m_hp > 0) return;
            m_hp = 0;
            m_isDed = true;
            m_cockroachMoveControllerNetWork.IsDed = true;
            EventSystem.Instance.IsDed(m_isDed);
            Debug.Log("Ded!");
        }

        /// <summary>
        /// 無敵モード
        /// </summary>
        IEnumerator InvincibleMode()
        {
            Debug.Log("無敵モード開始");
            // 無敵モード開始
            m_invincibleMode = true;
            photonView.RPC(nameof(m_cockroachMoveControllerNetWork.InvincibleMode), RpcTarget.All, m_invincibleMode, m_addSpeedValue, m_addJumpValue);

            yield return new WaitForSeconds(m_invincibleModeTime);

            Debug.Log("無敵モード停止");
            // 無敵モード停止
            m_invincibleMode = false;
            photonView.RPC(nameof(m_cockroachMoveControllerNetWork.InvincibleMode), RpcTarget.All, m_invincibleMode, m_addSpeedValue, m_addJumpValue);
        }

        [PunRPC]
        /// <summary>
        /// 1秒おきにHitPointを減らす
        /// </summary>
        /// /// <param name="decreaseValue">減少させる量</param>
        void DecreaseHitPoint(int decreaseValue)
        {
            m_oneSecondTimer += Time.deltaTime;

            if (m_oneSecondTimer < 1f) return;

            if (m_satietyGauge > 0)
            {
                //満腹ゲージを減らす
                m_satietyGauge -= decreaseValue;
            }
            else
            {
                // 体力を減らす
                m_hp -= decreaseValue;
                if (m_cockroachUINetWork)
                {
                    StartCoroutine(m_cockroachUINetWork.DamageColor());
                }
            }

            m_cockroachUINetWork.ReflectGauge(m_satietyGauge, m_maxSatietyGauge);
            m_cockroachUINetWork.ReflectHPSlider(m_hp, m_maxHp);
            photonView.RPC(nameof(CheckAlive), RpcTarget.All);

            m_oneSecondTimer = 0;
        }

        [PunRPC]
        /// <summary>
        /// 食べ物を食べて、満腹ゲージを回復する。
        /// </summary>
        /// <param name="heelValue">体力に加算する値</param>
        void Eat(int heelValue)
        {
            m_satietyGauge += heelValue;

            // 現在のHPがHPの最大値を超えないようにする
            if (m_satietyGauge > m_maxSatietyGauge)
            {
                m_satietyGauge = m_maxSatietyGauge;
            }

            m_cockroachUINetWork.ReflectGauge(m_satietyGauge, m_maxSatietyGauge);
            
            if (PhotonNetwork.IsMasterClient)
            {
                EventSystem.Instance.Generate(1);
            }
            
            Debug.Log("Heel");
        }

        /// <summary>
        /// 攻撃されたときに呼ばれる。体力を減らす。 
        /// </summary>
        /// <param name="damageValue">体力を減算する値</param>
        public void BeAttacked(int damageValue)
        {
            if (m_isDed) return;
            if (m_invincibleMode) return;

            //if (photonView.IsMine)
            //{
            //    m_hp -= damageValue;
            //}
            
            // 生存確認
            photonView.RPC(nameof(CheckAlive), RpcTarget.Others);
            // 無敵モード開始
            photonView.RPC(nameof(StartCoroutineInvicibleMode), RpcTarget.Others);
            // ダメージを受けたUI表示
            photonView.RPC(nameof(StartCoroutineDamegeImageChangeColor), RpcTarget.Others);
            // HPの同期（IsMine ではないオブジェクトからの同期なので OnPhotonSerializeView は使えない）
            photonView.RPC(nameof(RefleshHp), RpcTarget.Others, m_hp, damageValue);
            // HPバーを減少させる
            photonView.RPC(nameof(m_cockroachUINetWork.ReflectHPSlider), RpcTarget.Others, m_hp, m_maxHp);
        }

        [PunRPC]
        void StartCoroutineInvicibleMode()
        {
            StartCoroutine(InvincibleMode());
        }

        [PunRPC]
        void StartCoroutineDamegeImageChangeColor()
        {
            StartCoroutine(m_cockroachUINetWork.DamageColor());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!photonView.IsMine) return;

            if (other.tag == "Food")
            {
                photonView.RPC(nameof(Eat), RpcTarget.All, other.gameObject.GetComponent<Food>().m_heelValue);
            }
        }

        [PunRPC]
        void RefleshHp(int hp, int damage)
        {
            hp -= damage;
            this.m_hp = hp;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(m_isDed);
                stream.SendNext(m_invincibleMode);
            }
            else
            {
                m_isDed = (bool)stream.ReceiveNext();
                m_invincibleMode = (bool)stream.ReceiveNext();
            }
        }
    }
}
