using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scores : MonoBehaviour
{
    public ScoreManager scoreManager;

    Text scores_txt;

    // 对于每个用户的分数 （失败）
    public void UpdateGUIText(List<ScoreManager.Score> scoreList)
    {
        scores_txt = GetComponent<Text>();
        Debug.Log("Updating GUIText: scorelist count=" + scoreList.Count);
        string s = "";
        foreach (ScoreManager.Score sc in scoreList)
        {
            if (sc.score < 1000)
                s += sc.score + "\t\t\t" + sc.name + "\n";
            else
                s += sc.score + "\t\t" + sc.name + "\n";
        }

        scores_txt.text = s;
    }

}
