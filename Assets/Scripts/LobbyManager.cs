using System.Collections;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using System;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField playerNameField;
    public GameObject loadingPanel;
    public TMP_Text loadingText, pointTxt;
    public GameObject loadingCancelBtn;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected)
            return;

        PhotonNetwork.ConnectUsingSettings();

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    //Player Connected the server
    public override void OnConnected()
    {
        base.OnConnected();
        //get saved player name if exist
        string _playerName = PlayerPrefs.GetString("PlayerName");

        if (_playerName != string.Empty)
        {
            playerNameField.text = _playerName;
        }

        int point = PlayerPrefs.GetInt("Point");

        pointTxt.SetText("Puan: " + point);
        pointTxt.gameObject.SetActive(true);

        //Remove loading panel
        loadingPanel.SetActive(false);
    }

    public void StartGame()
    {
        //save player name
        PlayerPrefs.SetString("PlayerName", playerNameField.text);
        //Setup loading Panel
        PhotonNetwork.NickName = playerNameField.text;
        loadingCancelBtn.SetActive(true);
        loadingText.SetText("Oyun Bulunuyor");
        loadingPanel.SetActive(true);
        //Join random room
        PhotonNetwork.JoinRandomRoom();
    }

    public void CancelFindGame()
    {
        //stop searching
        PhotonNetwork.LeaveRoom();
        //remove loading panel
        loadingPanel.SetActive(false);
    }

    //if photon can't find created room, it create a room
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CreateRoom();
    }

    private void CreateRoom()
    {
        // room settings
        RoomOptions options = new RoomOptions() { IsOpen = true, IsVisible = true, MaxPlayers = 2 };
        //create room
        PhotonNetwork.CreateRoom("Oda " + UnityEngine.Random.Range(1, 1000), options);
    }


}
