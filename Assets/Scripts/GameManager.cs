using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public GameObject startTimerPanel;
    public TMP_Text player1NameTxt, player2NameTxt, QuestionTxt, Answer1Txt,
        Answer2Txt, Answer3Txt, Answer4Txt, TimerTxt, StartCounterTxt;
    public Image[] P1_InfoBox;
    public Image[] P2_InfoBox;
    public Animator p1answeranim, p2answeranim;
    public Button[] answerButtons;

    [Header("End UI")]
    public GameObject endGamePanel;
    public TMP_Text endInfoTxt, endCounterTxt;

    [HideInInspector]
    public bool isInGame;

    private const byte ShowAnswer = 0;
    private const byte GiveAnswer = 1;
    private const byte WrongAnswer = 2;

    public static GameManager instance;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PreparePlayers(Player[] players)
    {
        player1NameTxt.text = players[0].NickName;
        player2NameTxt.text = players[1].NickName;
    }

    public void Next()
    {
        QuestionManager.Instance.questionCounter++;

        if(QuestionManager.Instance.questionCounter == 3)
        {
            GameEnd();
            return;
        }
        //SHOW NEXT QUESTION
        QuestionManager.Instance.NextQuestion();
        
        StopAllCoroutines();
        StartCoroutine(QuestionTimer());
    }

    public void OnAnswerClick(Button button)
    {
        QuestionManager.Instance.ShowGivingAnswer(button);
        QuestionManager.Instance.AnswerButtonsEnabled(false);
        RaiseAnswerEvent((byte)button.tag[0]);
    }

    private void RaiseAnswerEvent(byte answer)
    {
        string playerName = PhotonNetwork.NickName;
        var playerID = RoomManager.GetPlayerID(playerName);
        object[] content = { playerName, answer };
    
        RaiseEvent(ShowAnswer, content);
    }

    private void RaiseEvent(byte code, object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(code, content, raiseEventOptions, sendOptions);
    }

    private IEnumerator QuestionTimer()
    {
        for(int i = 10;i >= 0; i--)
        {
            TimerTxt.text = i.ToString();
            yield return new WaitForSeconds(1);
        }

        QuestionManager.Instance.AnswerButtonsEnabled(false);

        RaiseAnswerEvent(ShowAnswer);

        StartCoroutine(WaitStart());
    }

    private IEnumerator WaitStart()
    {
        for (int i = 3; i >= 0; i--)
        {
           UpdateTimer(i);
            yield return new WaitForSeconds(1);
        }

        Next();
    }

    public void UpdateTimer(int time)
    {
        TimerTxt.SetText(time.ToString());
    }

    public void UpdateStartTimer(int time)
    {
        StartCounterTxt.SetText(time.ToString());
    }

    public void StartPanelActive(bool active)
    {
        startTimerPanel.SetActive(active);
        //SHOW TIMER TEXT
        TimerTxt.gameObject.SetActive(!active);
    }

    public void ShowAnswerPanel(byte id)
    {
        if (id == 0)
            p1answeranim.SetTrigger("Fade");
        else
            p2answeranim.SetTrigger("Fade");
    }

    public void UpdateScore(string playerName, bool isTrue)
    {
        if (RoomManager.GetPlayerID(playerName) == 0)
        {
            P1_InfoBox[QuestionManager.Instance.questionCounter].color = isTrue ? GlobalResources.TrueAnswerColor : GlobalResources.WrongAnswerColor;
        }
        else
        {
            P2_InfoBox[QuestionManager.Instance.questionCounter].color = isTrue ? GlobalResources.TrueAnswerColor : GlobalResources.WrongAnswerColor;
        }

        QuestionManager.Instance.ShowTrueAnswer();
    }
    
    private void GameEnd()
    {
        RaiseAnswerEvent(3);
    }

    public void FinishGame(byte state)
    {
        StopAllCoroutines();

        isInGame = false;

        switch (state)
        {
            case 0:
                endInfoTxt.SetText("KAZANDIN !");
                break;
            case 1:
                endInfoTxt.SetText("KAYBETTİN !");
                break;
            case 2:
                endInfoTxt.SetText("BERABERE !");
                break;
        }
        

        endGamePanel.SetActive(true);

        StartCoroutine(EndCounter());
    }

    private IEnumerator EndCounter()
    {
        for (int i = 10; i >= 0; i--)
        {
            endCounterTxt.SetText("" + i);
            yield return new WaitForSeconds(1);
        }

        ReturnMainMenu();
    }

    public void ReturnMainMenu()
    {
        StopAllCoroutines();
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel(0);
    }
}
