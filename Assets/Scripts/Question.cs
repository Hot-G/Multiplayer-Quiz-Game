using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Answer
{
    public string answertxt;
    public bool isTrue;
}
[System.Serializable]
public class Question
{
    public string question;
    public Answer answer1, answer2, answer3, answer4;
}
