using System;
using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public class GameGUINavigation : MonoBehaviour
{



    private bool _paused;
    private bool quit;
    private string _errorMsg;
    //public bool initialWaitOver = false;

    public float initialDelay;

    // canvas
    public Canvas PauseCanvas;
    public Canvas QuitCanvas;
    public Canvas ReadyCanvas;
    public Canvas ScoreCanvas;
    public Canvas ErrorCanvas;
    public Canvas GameOverCanvas;

    // button
    public Button MenuButton;




    // 初始化
    void Start()
    {
        //能暂停执行，暂停后立即返回，直到中断指令完成后继续执行
        StartCoroutine("ShowReadyScreen", initialDelay);
    }



    void Update()
    {
        // 帧更新时只有Esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Esc 退出 在Score状态的时候
            if (GameManager.gameState == GameManager.GameState.Scores)
                Menu();

            // 否则就触发退出或者暂停
            else
            {
                if (quit == true)
                    ToggleQuit();
                else
                    TogglePause();
            }
        }
    }


    // 协同程序 进入准备界面 有一定的等待时间
    public void H_ShowReadyScreen()
    {
        StartCoroutine("ShowReadyScreen", initialDelay);
    }

    public void H_ShowGameOverScreen()
    {
        StartCoroutine("ShowGameOverScreen");
    }


    // 用枚举器来迭代对象 将其建立后过几秒让其消失
    IEnumerator ShowReadyScreen(float seconds)
    {
        //initialWaitOver = false;
        GameManager.gameState = GameManager.GameState.Init;
        ReadyCanvas.enabled = true;
        yield return new WaitForSeconds(seconds);
        ReadyCanvas.enabled = false;
        GameManager.gameState = GameManager.GameState.Game;
        //initialWaitOver = true;
    }


    //gameover 同上
    IEnumerator ShowGameOverScreen()
    {
        Debug.Log("Showing GAME OVER Screen");
        GameOverCanvas.enabled = true;
        yield return new WaitForSeconds(2);
        Menu();

    }


    //游戏结束后暂停一切 如果分数高的话隐藏菜单显示分数
    public void getScoresMenu()
    {
        Time.timeScale = 0f;        
        GameManager.gameState = GameManager.GameState.Scores;
        MenuButton.enabled = false;
        ScoreCanvas.enabled = true;
    }




    // 按键的功能

    public void TogglePause()
    {
        // 如果已经暂停了按键就取消暂停
        if (_paused)
        {
            //让游戏正常进行
            Time.timeScale = 1;
            PauseCanvas.enabled = false;
            _paused = false;
            MenuButton.enabled = true;
        }

        // 选择直接暂停
        else
        {
            PauseCanvas.enabled = true;
            //让游戏暂停
            Time.timeScale = 0.0f;
            _paused = true;
            MenuButton.enabled = false;
        }


        Debug.Log("PauseCanvas enabled: " + PauseCanvas.enabled);
    }

    // canvas效果
    public void ToggleQuit()
    {
        if (quit)
        {
            PauseCanvas.enabled = true;
            QuitCanvas.enabled = false;
            quit = false;
        }

        else
        {
            QuitCanvas.enabled = true;
            PauseCanvas.enabled = false;
            quit = true;
        }
    }

    // 加载Menu场景
    public void Menu()
    {
        Application.LoadLevel("menu");
        Time.timeScale = 1.0f;

        // 注意需要清零！！！
        GameManager.DestroySelf();
    }

    

  

    public void LoadLevel()
    {
        GameManager.Level++;
        Application.LoadLevel("game");
    }

    
}
