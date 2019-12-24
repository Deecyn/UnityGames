using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{

    
    private string username;
    private int _highscore;
    private int _lowestHigh;
    private bool _scoresRead;
    private bool _isTableFound;


    public class Score
    {
        public string name { get; set; }
        public int score { get; set; }

        public Score(string n, int s)
        {
            name = n;
            score = s;
        }

        public Score(string n, string s)
        {
            name = n;
            score = Int32.Parse(s);
        }
    }

    List<Score> scoreList = new List<Score>(10);

    void OnLevelWasLoaded(int level)
    {
        //StartCoroutine("ReadScoresFromDB");

        if (level == 2) StartCoroutine("UpdateGUIText");    
        if (level == 1) _lowestHigh = _highscore = 99999;
        //if (level == 1) StartCoroutine("GetHighestScore");  
    }

    IEnumerator GetHighestScore()
    {
        Debug.Log("GETTING HIGHEST SCORE");

        // 等到数据从数据库里pull出
        float timeOut = Time.time + 4;
        while (!_scoresRead)
        {
            yield return new WaitForSeconds(0.01f);
            if (Time.time > timeOut)
            {
                Debug.Log("Timed out");
                //scoreList.Clear();
                //scoreList.Add(new Score("GetHighestScore:: DATABASE CONNECTION TIMED OUT", -1));
                break;
            }
        }

        //_highscore = scoreList[0].score;
        _highscore = 99999;
        _lowestHigh = scoreList[scoreList.Count - 1].score;
    }

    IEnumerator UpdateGUIText()
    {
        
        scoreList.Clear();
        scoreList.Add(new Score("DATABASE TEMPORARILY UNAVAILABLE", 999999));

        GameObject.FindGameObjectWithTag("ScoresText").GetComponent<Scores>().UpdateGUIText(scoreList);
        yield return new WaitForSeconds(0f);
    }

    

    public int High()
    {
        return _highscore;
    }

    public int LowestHigh()
    {
        return _lowestHigh;
    }
}
