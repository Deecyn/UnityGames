﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonLoading : MonoBehaviour
{
    public void StartJump(){
        SceneManager.LoadScene("JumpJump");
    }
    public void StartPacman(){
        SceneManager.LoadScene("menu");
    }

    public void ReturnMenu(){
        SceneManager.LoadScene("MenuScene");
    }
}
