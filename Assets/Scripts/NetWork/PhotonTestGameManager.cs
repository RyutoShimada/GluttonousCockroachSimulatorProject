using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;

namespace Photon.Pun.Demo.PunBasics
{
    public class PhotonTestGameManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] byte m_maxPlayers = 2;
        [SerializeField] GameObject m_playerPrefab = null;
        [SerializeField] Transform m_spawnPos1 = null;
        [SerializeField] Transform m_spawnPos2 = null;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("マスターサーバーへの接続を開始します...");
            // マスターサーバーへ接続する
            PhotonNetwork.ConnectUsingSettings();
        }

        /// <summary>
        /// マスターサーバーへの接続に成功した時に呼ばれる
        /// </summary>
        public override void OnConnectedToMaster()
        {
            Debug.Log("マスターサーバーに接続しました");

            PhotonNetwork.JoinLobby();
        }

        /// <summary>
        /// Photonサーバーから失敗した場合や、意図的に切断した後に呼ばれる
        /// </summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogError($"サーバーとの接続が切断されました : {cause}");
        }

        /// <summary>
        /// ロビーに入るのに成功した時に呼ばれる
        /// </summary>
        public override void OnJoinedLobby()
        {
            Debug.Log($"ロビーに入りました");

            Debug.Log("ランダムなルームへ参加します...");
            PhotonNetwork.JoinRandomRoom();
        }

        /// <summary>
        /// 部屋の入室に成功したときに呼ばれる
        /// </summary>
        public override void OnJoinedRoom()
        {
            int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

            Debug.Log($"ActorNumber : {actorNumber} がルームに参加しました");

            if (actorNumber == 1)
            {
                PhotonNetwork.Instantiate(m_playerPrefab.name, m_spawnPos1.position, Quaternion.identity);
                
            }
            else
            {
                PhotonNetwork.Instantiate(m_playerPrefab.name, m_spawnPos2.position, Quaternion.identity);
            }

            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                Debug.Log("あなたはマスタークライアントです");
            }

            if (PhotonNetwork.CurrentRoom.PlayerCount == m_maxPlayers)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }

        /// <summary>
        /// ランダムな部屋の入室に失敗した時に呼ばれる
        /// </summary>
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.LogError($"ルームに参加できませんした : {message}");

            Debug.Log("ルームを作成します");
            var roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = m_maxPlayers;
            PhotonNetwork.CreateRoom(null, roomOptions);
        }

        /// <summary>
        /// 部屋の作成に成功した時に呼ばれる
        /// </summary>
        public override void OnCreatedRoom()
        {
            Debug.Log($"ルームの作成に成功しました : {PhotonNetwork.CurrentRoom}");
        }

        /// <summary>
        /// 部屋の作成に失敗した時に呼ばれる
        /// </summary>
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"ルームの作成に失敗しました : {message}");
        }

        /// <summary>
        /// 同じ部屋にいたプレイヤーが参加した時に呼ばれる
        /// </summary>
        public override void OnPlayerEnteredRoom(Realtime.Player newPlayer)
        {
            Debug.Log($"ActorNumber : {newPlayer.ActorNumber} がルームに参加しました");
        }

        /// <summary>
        /// 同じ部屋にいたプレイヤーが退出した時に呼ばれる
        /// </summary>
        public override void OnPlayerLeftRoom(Realtime.Player player)
        {
            Debug.Log($"ActorNumber : {player.ActorNumber} がルームに退出しました");
        }
    }
}
