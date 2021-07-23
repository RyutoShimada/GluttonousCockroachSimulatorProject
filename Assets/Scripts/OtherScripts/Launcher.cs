using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

namespace Photon.Pun.Demo.PunBasics
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        #region Private Serializable Fields

        [Tooltip("プレイヤーがキャラクターを選択するための UI Panel")]
        [SerializeField] GameObject m_controlPanel = null;

        [Tooltip("接続の進捗状況をユーザーに知らせるUIテキスト")]
        [SerializeField] Text m_feedbackText = null;

        [Tooltip("1部屋あたりの最大プレイヤー数")]
        [SerializeField] byte m_maxPlayersPerRoom = 2;

        // todo:
        //[Tooltip("ロード中に表示させるアニメ")]
        //[SerializeField] LoaderAnime m_loaderAnime;

        #endregion

        #region Private Fields

        /// <summary>
        /// 現在のプロセスを追跡。接続は非同期で、Photonからのいくつかのコールバックに基づく。
        /// Photonからのコールバックを受けたときの動作を適切に調整するために、記録しておく必要がある。
        /// 一般的には、OnConnectedToMaster()コールバックに使用される。
        /// </summary>
        bool m_isConnecting;

        /// <summary>
        /// このクライアントのバージョン番号。ユーザーはgameVersionで区切られている。（これにより、破壊的な変更を行うことができる）
        /// </summary>
        string m_gameVersion = "1";

        #endregion

        #region MonoBehaviour CallBacks

        void Awake()
        {
            // todo:
            //if (loaderAnime == null)
            //{
            //    Debug.LogError("<Color=Red><b>Missing</b></Color> loaderAnime Reference.", this);
            //}

            // これにより、マスタークライアントでPhotonNetwork.LoadLevel()を使用すると、
            // 同じ部屋にいるすべてのクライアントが自動的にレベルを同期することができます。
            PhotonNetwork.AutomaticallySyncScene = true;

        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 接続処理を開始します。
        /// すでに接続されている場合は、ランダムな部屋への参加を試みます。
        /// まだ接続されていない場合、このアプリケーションのインスタンスをPhoton Cloud Networkに接続します。
        /// </summary>
        public void Connect()
        {
            m_feedbackText.text = "";
            m_isConnecting = true;
            m_controlPanel.SetActive(false);

            // 視覚効果のためにローダーのアニメーションを開始します。// todo:
            //if (m_loaderAnime != null)
            //{
            //	m_loaderAnime.StartLoaderAnimation();
            //}

            // 接続されているかどうかをチェックし、接続されていれば参加し、そうでなければサーバーへの接続を開始します。
            if (PhotonNetwork.IsConnected)
            {
                LogFeedback("部屋へ接続...");

                // 既にあるランダムなルームへ接続。失敗すると新しくルームを作成する。
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                LogFeedback("マスターサーバーへ接続...");

                PhotonNetwork.ConnectUsingSettings(); // マスターサーバーへ接続する
                PhotonNetwork.GameVersion = this.m_gameVersion;
            }
        }

        /// <summary>
        /// 開発者用のUnity Editor内ではなく、プレイヤー用のUIビューにフィードバックを記録します。
        /// </summary>
        /// <param name="message">表示内容</param>
        void LogFeedback(string message)
        {
            if (!m_feedbackText) return;

            // 新しいメッセージを改行して表示する。
            m_feedbackText.text += System.Environment.NewLine + message;
        }

        #endregion

        #region MonoBehaviourPunCallbacks CallBacks

        /// <summary>
        /// マスターサーバーへの接続に成功した時に呼ばれる
        /// </summary>
        public override void OnConnectedToMaster()
        {
            if (m_isConnecting)
            {
                LogFeedback("マスターサーバーへの接続に成功 -> ランダムな部屋への接続を開始");
                Debug.Log("マスターサーバーへの接続に成功 -> ランダムな部屋への接続を開始");
                PhotonNetwork.JoinRandomRoom();
            }
        }

        /// <summary>
        /// ランダムな部屋の入室に失敗した時に呼ばれる
        /// </summary>
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            LogFeedback("ランダムな部屋への接続に失敗 -> 部屋を作成");
            Debug.Log("ランダムな部屋への接続に失敗 -> 部屋を作成");
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = this.m_maxPlayersPerRoom });
        }


        /// <summary>
        /// Photonサーバーから失敗した場合や、意図的に切断した後に呼ばれる
        /// </summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            LogFeedback($"サーバーとの接続が切断されました : {cause}");
            Debug.LogError($"サーバーとの接続が切断されました : {cause}");

            //m_loaderAnime.StopLoaderAnimation(); // todo:

            m_isConnecting = false;
            m_controlPanel.SetActive(true);

        }

        /// <summary>
        /// 部屋の入室に成功したときに呼ばれる
        /// </summary>
        /// <remarks>
        /// このメソッドは、プレイヤーキャラクターをインスタンス化するためによく使われます。
        /// 試合を「能動的」に開始する必要がある場合は、ユーザーのボタン操作やタイマーで起動する[PunRPC]を呼び出すことができます。
        /// これが呼び出されたとき、通常はすでにPhotonNetwork.PlayerListを介して部屋にいる既存のプレーヤーにアクセスできます。
        /// また、すべてのカスタムプロパティは、Room.customProperties としてすでに利用可能なはずです。Room.PlayerCountを確認すると、以下のことがわかります。
        /// 充分な数のプレーヤーが部屋にいる状態でプレイを開始します。
        /// </remarks>
        public override void OnJoinedRoom()
        {
            LogFeedback($"部屋への接続に成功 : 現在の部屋の人数は { PhotonNetwork.CurrentRoom.PlayerCount} 人");
            Debug.Log($"部屋への接続に成功 : 現在の部屋の人数は { PhotonNetwork.CurrentRoom.PlayerCount} 人");

            // 最初のプレーヤーである場合のみロードし、そうでない場合はPhotonNetwork.AutomaticallySyncSceneに頼ってインスタンスシーンを同期させます。
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("NetWorkGameScene をロードします");
                PhotonNetwork.LoadLevel("NetWorkGameScene");
            }

            // 部屋が満員になったら、部屋を閉じる
            if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }

        #endregion
    }
}