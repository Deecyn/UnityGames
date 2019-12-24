using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    // 对UI界面的生命值进行调整
    // 对UI界面中的
    public int high, score;

    public List<Image> lives = new List<Image>(3);

    Text txt_score, txt_high, txt_level;

    // Use this for initialization
    void Start()
    {
        // 取出成绩最高分以及等级
        txt_score = GetComponentsInChildren<Text>()[1];
        txt_high = GetComponentsInChildren<Text>()[0];
        txt_level = GetComponentsInChildren<Text>()[2];

        for (int i = 0; i < 3 - GameManager.lives; i++)
        {
            // 把图片消除
            Destroy(lives[lives.Count - 1]);
            lives.RemoveAt(lives.Count - 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 把score中的最高成绩调出输出
        high = GameObject.Find("Game Manager").GetComponent<ScoreManager>().High();

        // 更新UI界面text
        score = GameManager.score;
        txt_score.text = "Score\n" + score;
        txt_high.text = "High Score\n" + high;
        txt_level.text = "Level\n" + (GameManager.Level + 1);

    }


}
