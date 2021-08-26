using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;


/// <summary>
/// ゴキブリのUI
/// </summary>
public class CockroachUINetWork : MonoBehaviourPunCallbacks
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

    GameObject m_ui;

    private void Start()
    {
        if (!m_cockroachUiPrefab)
        {
            Debug.LogError("m_cockroachUiPrefab が NULL です。CockroachUI スクリプトに cockroachUiPrefab がアサインされているか確認して下さい。", this);
        }
        else
        {
            Transform t = GameObject.Find("Canvas").transform;
            m_ui = Instantiate(m_cockroachUiPrefab, t);

            if (m_ui)
            {
                m_satietyGaugeImage = m_ui.transform.Find("Gauge").transform.Find("SatietyGauge").GetComponent<Image>();
                m_hpSlider = m_ui.transform.Find("HPSlider").GetComponent<Slider>();
                m_damageImage = m_ui.transform.Find("DamageImage").GetComponent<Image>();

                if (m_satietyGaugeImage)
                {
                    m_satietyGaugeImage.fillAmount = 1;
                }

                if (m_hpSlider)
                {
                    m_hpSlider.value = 1;
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
                UiSetActiveFalse();
            }
        }
    }

    /// <summary>
    /// ダメージを受けた時に画面を赤くする
    /// </summary>
    /// <returns></returns>
    public IEnumerator DamageColor()
    {
        m_damageImage.DOColor(m_originDamageColor, m_afterSeconds);
        yield return new WaitForSeconds(m_afterSeconds);
        m_damageImage.DOColor(m_saveDamageColor, m_afterSeconds);
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

    /// <summary>
    /// Cockroach の UI を非表示にします
    /// </summary>
    public void UiSetActiveFalse() => m_ui.SetActive(false);
}