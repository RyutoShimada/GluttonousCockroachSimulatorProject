using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

namespace Photon.Pun.Demo.PunBasics
{
    public class NetWorkGameManager : MonoBehaviourPunCallbacks
    {
        #region Public Fields

        static public NetWorkGameManager m_Instance = null;

        #endregion

        #region Private Fields

        GameObject m_instance;

        [Tooltip("CockroachNetWork の Prefab")]
        [SerializeField] GameObject m_cockroachPrefab = null;

        [Tooltip("Cockroach の 生成場所")]
        [SerializeField] Transform m_cockroachSpawnPos = null;

        [Tooltip("HumanNetWork の Prefab")]
        [SerializeField] GameObject m_humanPrefab = null;

        [Tooltip("Human の 生成場所")]
        [SerializeField] Transform m_humanSpawnPos = null;

        byte m_maxPlayerCount = 2;

        #endregion

        #region MonoBehaviour CallBacks

        void Start()
        {
            m_Instance = this;

            if (!m_cockroachPrefab)
            {
                Debug.LogError("CockroachPrefab が NULL です。NetWorkManager にアサインしてください。", this);
            }
            else if (!m_humanPrefab)
            {
                Debug.LogError("HumanPrefab が NULL です。NetWorkManager にアサインしてください。", this);
            }
            else
            {
                //if (!CockroachNetWork.m_localInstance)
                //{
                //    Debug.LogFormat($"{SceneManagerHelper.ActiveSceneName}から LocalCockroach をインスタンス化しています。");

                //    // 部屋の中で、ローカルプレーヤー用の Cockroach を生成。PhotonNetwork.Instantiate()で同期。
                //    PhotonNetwork.Instantiate(this.m_cockroachPrefab.name, m_cockroachSpawnPos.position, Quaternion.identity, 0);
                //}
                //else if (!HumanMoveControllerNetWork.m_localInstance)
                //{
                //    Debug.LogFormat($"{SceneManagerHelper.ActiveSceneName}から LocalHuman をインスタンス化しています。");

                //    // 部屋の中で、ローカルプレーヤー用の Human を生成。PhotonNetwork.Instantiate()で同期。
                //    PhotonNetwork.Instantiate(this.m_humanPrefab.name, m_humanSpawnPos.position, Quaternion.identity, 0);
                //}
                //else
                //{
                //    Debug.LogFormat($"{SceneManagerHelper.ActiveSceneName} のシーンロードを無視しています。");
                //}
            }

            // Photon に接続する
            Connect("1.0"); // 1.0 はバージョン番号（同じバージョンを指定したクライアント同士が接続できる）
        }


        void Update()
        {

        }

        #endregion

        #region Photon Callbacks

        /// <summary>
        /// Photonに接続する
        /// </summary>
        private void Connect(string gameVersion)
        {
            if (PhotonNetwork.IsConnected == false)
            {
                PhotonNetwork.GameVersion = gameVersion;    // 同じバージョンを指定したもの同士が接続できる
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        /// <summary>
        /// ニックネームを付ける
        /// </summary>
        private void SetMyNickName(string nickName)
        {
            if (PhotonNetwork.IsConnected)
            {
                Debug.Log("nickName: " + nickName);
                PhotonNetwork.LocalPlayer.NickName = nickName;
            }
        }

        /// <summary>
        /// ロビーに入る
        /// </summary>
        private void JoinLobby()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinLobby();
            }
        }

        /// <summary>
        /// 既に存在する部屋に参加する
        /// </summary>
        private void JoinExistingRoom()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRandomRoom();
            }
        }

        /// <summary>
        /// ランダムな名前のルームを作って参加する
        /// </summary>
        private void CreateRandomRoom()
        {
            if (PhotonNetwork.IsConnected)
            {
                RoomOptions roomOptions = new RoomOptions();
                roomOptions.IsVisible = true;   // 誰でも参加できるようにする
                /* **************************************************
                 * spawPositions の配列長を最大プレイ人数とする。
                 * 無料版では最大20まで指定できる。
                 * MaxPlayers の型は byte なのでキャストしている。
                 * MaxPlayers の型が byte である理由はおそらく1ルームのプレイ人数を255人に制限したいためでしょう。
                 * **************************************************/
                roomOptions.MaxPlayers = m_maxPlayerCount;
                PhotonNetwork.CreateRoom(null, roomOptions); // ルーム名に null を指定するとランダムなルーム名を付ける
            }
        }

        /// <summary>
        /// プレイヤーを生成する
        /// </summary>
        private void SpawnPlayer()
        {
            // プレイヤーをどこに spawn させるか決める
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;    // 自分の ActorNumber を取得する。なお ActorNumber は「1から」入室順に振られる。
            Debug.Log("My ActorNumber: " + actorNumber);

            if (actorNumber == 1)
            {
                // 部屋の中で、ローカルプレーヤー用の Cockroach を生成。PhotonNetwork.Instantiate()で同期。
                PhotonNetwork.Instantiate(this.m_cockroachPrefab.name, m_cockroachSpawnPos.position, Quaternion.identity, 0);
            }
            else
            {
                // 部屋の中で、ローカルプレーヤー用の Human を生成。PhotonNetwork.Instantiate()で同期。
                PhotonNetwork.Instantiate(this.m_humanPrefab.name, m_humanSpawnPos.position, Quaternion.identity, 0);
            }

            /* **************************************************
             * ルームに参加している人数が最大に達したら部屋を閉じる（参加を締め切る）
             * 部屋を閉じないと、最大人数から減った時に次のユーザーが入ってきてしまう。
             * 現状のコードではユーザーが最大人数から減った際の追加入室を考慮していないため、追加入室させたい場合は実装を変更する必要がある。
             * **************************************************/
            if (actorNumber > PhotonNetwork.CurrentRoom.MaxPlayers - 1)
            {
                Debug.Log("Closing Room");
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }

        /// <summary>Photon に接続した時</summary>
        public override void OnConnected()
        {
            Debug.Log("OnConnected");
            SetMyNickName(System.Environment.UserName + "@" + System.Environment.MachineName);
        }

        /// <summary>Photon との接続が切れた時</summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("OnDisconnected");
        }

        /// <summary>マスターサーバーに接続した時</summary>
        public override void OnConnectedToMaster()
        {
            Debug.Log("OnConnectedToMaster");
            JoinLobby();
        }

        /// <summary>ロビーに参加した時</summary>
        public override void OnJoinedLobby()
        {
            Debug.Log("OnJoinedLobby");
            JoinExistingRoom();
        }

        /// <summary>ロビーから出た時</summary>
        public override void OnLeftLobby()
        {
            Debug.Log("OnLeftLobby");
        }

        /// <summary>部屋を作成した時</summary>
        public override void OnCreatedRoom()
        {
            Debug.Log("OnCreatedRoom");
        }

        /// <summary>部屋の作成に失敗した時</summary>
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.Log("OnCreateRoomFailed: " + message);
        }

        /// <summary>部屋に入室した時</summary>
        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom");
            SpawnPlayer();
        }

        /// <summary>指定した部屋への入室に失敗した時</summary>
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log("OnJoinRoomFailed: " + message);
        }

        /// <summary>ランダムな部屋への入室に失敗した時</summary>
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("OnJoinRandomFailed: " + message);
            CreateRandomRoom();
        }

        /// <summary>
        /// ローカルプレーヤーが部屋を出たときに呼び出される。ランチャーシーンをロードする必要があります。
        /// </summary>
        public override void OnLeftRoom()
        {
            // ここでキャラクター選択のシーンにロードするようにする。
            //SceneManager.LoadScene("PunBasics-Launcher");
        }

        /// <summary>
        /// Photon Player が接続されたときに呼び出されます。その後、より大きなシーンをロードする必要があります。
        /// </summary>
        /// <param name="other">Other.</param>
        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.Log("OnPlayerEnteredRoom() " + other.NickName); // プレイヤーが接続している場合は表示されません。

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat($"OnPlayerEnteredRoom IsMasterClient { PhotonNetwork.IsMasterClient }"); // OnPlayerLeftRoom の前に呼び出される

                LoadArena();
            }
        }

        /// <summary>
        /// Photon Player が切断されたときに呼び出されます。小さなシーンをロードする必要があります。
        /// </summary>
        /// <param name="other">Other.</param>
        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.Log("OnPlayerLeftRoom() " + other.NickName); // 他者の切断時に見られる

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat($"OnPlayerEnteredRoom IsMasterClient { PhotonNetwork.IsMasterClient }"); // OnPlayerLeftRoomの前に呼び出される

                LoadArena();
            }
        }

        #endregion

        #region Private Methods

        void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : レベルをロードしようとしているが、マスタークライアントではありません。");
            }

            Debug.LogFormat($"PhotonNetwork : Loading Level : { PhotonNetwork.CurrentRoom.PlayerCount }");

            PhotonNetwork.LoadLevel($"PunBasics - Room for { PhotonNetwork.CurrentRoom.PlayerCount }");
        }

        #endregion
    }
}
