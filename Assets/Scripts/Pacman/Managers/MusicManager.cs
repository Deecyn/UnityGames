using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{



    private static MusicManager _instance;

    public static MusicManager instance
    {
        get
        {
            if (_instance == null)
            {
                // 在每次调用DontDestroyOnLoad的时候，都去判断场景中是否有对应的物体，如果没有再去创建
                _instance = GameObject.FindObjectOfType<MusicManager>();

                // 保证Gameobject以及上面绑定的组件不会销毁，在处理全局控制的时候有用。
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != _instance)
                Destroy(gameObject);
        }

    }




}
