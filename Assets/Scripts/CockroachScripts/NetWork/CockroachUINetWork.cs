using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Photon.Pun.Demo.PunBasics
{
    /// <summary>
    /// ゴキブリのUI
    /// </summary>
    public class CockroachUINetWork : MonoBehaviourPunCallbacks, IPunObservable
    {
        /// <summary>満腹ゲージのUI</summary>
        Image m_satietyGaugeImage = null;
        /// <summary>HPバーのUI</summary>
        Slider m_hpSlider = null;
        /// <summary>ダメージを受けた時のUI</summary>
        Image m_damageImage = null;

        [SerializeField] GameObject m_cockroachUiPrefab = null;

        /// <summary>UIの動きを何秒かけて行うか</summary>
        [SerializeField, Range(0.1f, 1.0f)] float m_afterSeconds = 0.2f;
        /// <summary>m_damageImageの初期colorを保存しておく変数</summary>
        Color m_originDamageColor;
        /// <summary>Alfa値が0のm_damageImageを保存しておく変数</summary>
        Color m_saveDamageColor;

        Color m_color;

        private void Start()
        {
            if (!m_cockroachUiPrefab)
            {
                Debug.LogError("m_cockroachUiPrefab が NULL です。CockroachUI スクリプトに cockroachUiPrefab がアサインされているか確認して下さい。", this);
            }
            else
            {
                Transform t = GameObject.Find("Canvas").transform;
                GameObject go = Instantiate(m_cockroachUiPrefab, t);

                if (go)
                {
                    m_satietyGaugeImage = go.transform.Find("Gauge").transform.Find("SatietyGauge").GetComponent<Image>();
                    m_hpSlider = go.transform.Find("HPSlider").GetComponent<Slider>();
                    m_damageImage = go.transform.Find("DamageImage").GetComponent<Image>();

                    if (m_satietyGaugeImage)
                    {
                        m_satietyGaugeImage.fillAmount = 1;
                    }

                    if (m_hpSlider)
                    {
                        m_hpSlider.value = 1;
                    }

                    if (m_damageImage)
                    {
                        m_color = m_damageImage.color;
                    }
                }
                else
                {
                    Debug.LogError("m_cockroachUiPrefab がインスタンス化されていません。", this);
                }

                if (m_damageImage)
                {
                    m_originDamageColor = m_damageImage.color;

                    // アルファ値を0にする
                    m_saveDamageColor = m_damageImage.color;
                    m_saveDamageColor.a = 0;
                    m_damageImage.color = m_saveDamageColor;
                    m_damageImage.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogError("m_damageImage を GetComponent 出来ませんでした。", this);
                }

                if (!photonView.IsMine)
                {
                    go.SetActive(false);
                }
            }
        }

        /// <summary>
        /// ダメージを受けた時に画面を赤くする
        /// </summary>
        /// <returns></returns>
        public IEnumerator DamageColor()
        {
            //photonView.RPC(nameof(DoTweenDamegeColor), RpcTarget.All, m_originDamageColor.r, m_originDamageColor.g, m_originDamageColor.b, m_originDamageColor.a, m_afterSeconds);
            m_damageImage.DOColor(m_originDamageColor, m_afterSeconds);
            yield return new WaitForSeconds(m_afterSeconds);
            m_damageImage.DOColor(m_saveDamageColor, m_afterSeconds);
            //photonView.RPC(nameof(DoTweenDamegeColor), RpcTarget.All, m_color.r, m_color.g, m_color.b, m_color.a, m_afterSeconds);
        }

        /// <summary>
        /// RPCでダメージのアルファ値を変更する
        /// </summary>
        /// <param name="c"></param>
        /// <param name="seconds"></param>
        [PunRPC]
        void DoTweenDamegeColor(float r, float g, float b, float a, float seconds)
        {
            Color c = new Color(r, g, b, a);
            m_damageImage.DOColor(c, seconds);
        }

        /// <summary>
        /// Cockroachから満腹ゲージが減った時に呼ぶ
        /// </summary>
        /// <param name="satietyGauge">満腹ゲージ</param>
        /// <param name="maxSatietyGauge">満腹ゲージの最大値</param>
        public void ReflectGauge(int satietyGauge, int maxSatietyGauge)
        {
            m_satietyGaugeImage.DOFillAmount((float)satietyGauge / (float)maxSatietyGauge, m_afterSeconds);
        }

        [PunRPC]
        /// <summary>
        /// Cockroachから体力が減った時に呼ぶ
        /// </summary>
        /// <param name="hp">体力</param>
        /// <param name="maxHp">体力の最大値</param>
        public void ReflectHPSlider(int hp, int maxHp)
        {
            m_hpSlider.DOValue((float)hp / (float)maxHp, m_afterSeconds);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting && !photonView.IsMine)
            {
                stream.SendNext(m_damageImage.color.r);
                stream.SendNext(m_damageImage.color.g);
                stream.SendNext(m_damageImage.color.b);
                stream.SendNext(m_damageImage.color.a);
            }
            else
            {
                if (m_color != null)
                {
                    Debug.Log(info);
                    m_color.r = (float)stream.ReceiveNext();
                    m_color.g = (float)stream.ReceiveNext();
                    m_color.b = (float)stream.ReceiveNext();
                    m_color.a = (float)stream.ReceiveNext();
                    m_damageImage.color = m_color;
                }
            }
        }
    }
}
