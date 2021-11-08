using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlayerController : MonoBehaviour, IOnEventCallback
{    
    
    public int PlayerID { get; set; }
    [SerializeField]
    private string _playerName;
    public string PlayerName
    {
        get => _playerName;

        set
        {
            _playerName = value;
            gameObject.name = _playerName;
        }
    }

    [SerializeField]
    private PhotonView _photonView;
    //IN GAME
    private Answers givingAnswer;

    private void Start()
    {
        if (_photonView.IsMine)
            _photonView.RPC("SetPlayer", RpcTarget.All, PlayerPrefs.GetString("PlayerName"));

        if (PhotonNetwork.IsMasterClient)
        {
            PreparePlayers();
            _photonView.RPC("PreparePlayers", RpcTarget.Others);
        }
    }

    public void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    [PunRPC]
    public void SetPlayer(string name)
    {
        PlayerName = name;
    }

    [PunRPC]
    private void PreparePlayers()
    {
        GameManager.instance.PreparePlayers(PhotonNetwork.PlayerList);

        if(_photonView.IsMine)
            StartCoroutine(WaitStart());
    }

    private IEnumerator WaitStart()
    {
        for(int i = 3;i > 0; i--)
        {
            GameManager.instance.UpdateStartTimer(i);
            yield return new WaitForSeconds(1);
        }
        //CLOSE START PANEL
        GameManager.instance.StartPanelActive(false);
        //SHOW ANOTHER QUESTION
        GameManager.instance.Next();
    }


    public void OnEvent(EventData photonEvent)
    {
        if (!_photonView.IsMine)
            return;

        if (photonEvent.Code > 3)
            return;

        string playerName = ((object[])photonEvent.CustomData)[0] as string;

        switch (photonEvent.Code)
        {
            case 0:
                GameManager.instance.UpdateScore(playerName, (bool)((object[])photonEvent.CustomData)[2]);

                break;
            case 1:
                GameManager.instance.ShowAnswerPanel((byte)((object[])photonEvent.CustomData)[1]);

                break;
            case 2:
                GameManager.instance.ShowAnswerPanel((byte)((object[])photonEvent.CustomData)[1]);
                
                break;
            case 3:
                ShowEndScreen(playerName);
                break;
        }
    }

    private void ShowEndScreen(string playerName)
    {
        Debug.Log(_photonView.IsMine + "   " + playerName);
        if (!_photonView.IsMine)
            return;
/*        PhotonNetwork.LocalPlayer.CustomProperties = new Hashtable()
        {
            { "TrueAnswer", 0 }
        }; */
        
        //ADD POINT
        var user1 = PhotonNetwork.PlayerList[PhotonNetwork.IsMasterClient ? 0 : 1];
        var user2 = PhotonNetwork.PlayerList[PhotonNetwork.IsMasterClient ? 1 : 0];
        
        if((int)user1.CustomProperties["TrueAnswer"] > (int)user2.CustomProperties["TrueAnswer"])
        {
            int value = PlayerPrefs.GetInt("Point") + 1;
            PlayerPrefs.SetInt("Point", value);
            GameManager.instance.FinishGame(0);
        }
        else if((int)user1.CustomProperties["TrueAnswer"] == (int)user2.CustomProperties["TrueAnswer"])
        {
            int value = PlayerPrefs.GetInt("Point") + 1;
            Debug.Log("Master : " + value);
            GameManager.instance.FinishGame(2);
        }
        else
        {
            GameManager.instance.FinishGame(1);
        }
    }
}
