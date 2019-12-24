using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GhostMove : MonoBehaviour
{


    // AI 路线
    private Vector3 waypoint;
    // 用队列来存放路径 用来初始化与scatter的状态
    private Queue<Vector3> waypoints;

    // 在 AI 中获取到的方向
    public Vector3 _direction;
    public Vector3 direction
    {
        get
        {
            return _direction;
        }

        set
        {
            // 将路径点设为当前路径加上方向
            _direction = value;
            Vector3 pos = new Vector3((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
            waypoint = pos + _direction;
            //Debug.Log ("waypoint (" + waypoint.position.x + ", " + waypoint.position.y + ") set! _direction: " + _direction.x + ", " + _direction.y);

        }
    }

    public float speed = 0.3f;



    public float scatterLength = 5f;
    public float waitLength = 0.0f;

    // 给每一种状态设置一定的结束时间
    private float timeToEndScatter;
    private float timeToEndWait;

    // ghost总共有五种状态
    enum State { Wait, Init, Scatter, Chase, Run };
    State state;

    // 鬼变色过程参数
    private Vector3 _startPos;
    private float _timeToWhite;
    private float _timeToToggleWhite;
    private float _toggleInterval;
    private bool isWhite = false;

    // 对鬼进行控制
    public GameGUINavigation GUINav;
    public PlayerController pacman;
    private GameManager _gm;



    void Start()
    {
        _gm = GameObject.Find("Game Manager").GetComponent<GameManager>();
        // 初始化鬼变的时常 以及初始化鬼
        _toggleInterval = _gm.scareLength * 0.33f * 0.20f;
        InitializeGhost();
    }

    public float DISTANCE;

    void FixedUpdate()
    {
        // 更新与路径点的距离
        DISTANCE = Vector3.Distance(transform.position, waypoint);

        // 在游戏状态中时 选择鬼现在的状态
        if (GameManager.gameState == GameManager.GameState.Game)
        {
            animate();

            switch (state)
            {
                case State.Wait:
                    Wait();
                    break;

                case State.Init:
                    Init();
                    break;

                case State.Scatter:
                    Scatter();
                    break;

                case State.Chase:
                    ChaseAI();
                    break;

                case State.Run:
                    RunAway();
                    break;
            }
        }
    }




    // 初始化鬼 以及鬼下一步的位置
    public void InitializeGhost()
    {
        _startPos = getStartPosAccordingToName();
        // 避免闪烁动画
        waypoint = transform.position;
        state = State.Wait;
        timeToEndWait = Time.time + waitLength + GUINav.initialDelay;
        InitializeWaypoints(state);
    }

    public void InitializeGhost(Vector3 pos)
    {
        transform.position = pos;
        waypoint = transform.position;
        state = State.Wait;
        timeToEndWait = Time.time + waitLength + GUINav.initialDelay;
        InitializeWaypoints(state);
    }


    private void InitializeWaypoints(State st)
    {

        // Init和Scatter状态下的路径
        // 根据名字 对鬼的位置进行直接编码 读行即可
        string data = "";
        switch (name)
        {
            case "blinky":
                data = @"22 20
22 26

27 26
27 30
22 30
22 26";
                break;
            case "pinky":
                data = @"14.5 17
14 17
14 20
7 20

7 26
7 30
2 30
2 26";
                break;
            case "inky":
                data = @"16.5 17
15 17
15 20
22 20

22 8
19 8
19 5
16 5
16 2
27 2
27 5
22 5";
                break;
            case "clyde":
                data = @"12.5 17
14 17
14 20
7 20

7 8
7 5
2 5
2 2
13 2
13 5
10 5
10 8";
                break;

        }



        // 从编码中读入
        string line;

        waypoints = new Queue<Vector3>();
        Vector3 wp;

        // 在建立的状态下
        if (st == State.Init)
        {
            using (StringReader reader = new StringReader(data))
            {
                // 空了就不读了
                while ((line = reader.ReadLine()) != null)
                {

                    if (line.Length == 0) break;

                    string[] values = line.Split(' ');
                    float x = float.Parse(values[0]);
                    float y = float.Parse(values[1]);

                    wp = new Vector3(x, y, 0);
                    waypoints.Enqueue(wp);
                }
            }
        }

        // 在Scatter的状态下
        if (st == State.Scatter)
        {

            // 如果已经读空编码 然后直接读取坐标
            bool scatterWps = false;

            using (StringReader reader = new StringReader(data))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length == 0)
                    {
                        // 直到空行以后开始读取
                        // 为了避免初始位置
                        scatterWps = true;
                        continue;
                    }

                    if (scatterWps)
                    {
                        string[] values = line.Split(' ');
                        int x = Int32.Parse(values[0]);
                        int y = Int32.Parse(values[1]);

                        wp = new Vector3(x, y, 0);
                        waypoints.Enqueue(wp);
                    }
                }
            }
        }


        // 在等待状态下
        if (st == State.Wait)
        {
            Vector3 pos = transform.position;

            // 左右两只先下后上
            if (transform.name == "inky" || transform.name == "clyde")
            {
                waypoints.Enqueue(new Vector3(pos.x, pos.y - 0.5f, 0f));
                waypoints.Enqueue(new Vector3(pos.x, pos.y + 0.5f, 0f));
            }
            // 中间先上后下
            else
            {
                waypoints.Enqueue(new Vector3(pos.x, pos.y + 0.5f, 0f));
                waypoints.Enqueue(new Vector3(pos.x, pos.y - 0.5f, 0f));
            }
        }

    }

    // 根据鬼的名字来对鬼的位置进行初始化
    private Vector3 getStartPosAccordingToName()
    {
        switch (gameObject.name)
        {
            case "blinky":
                return new Vector3(15f, 20f, 0f);

            case "pinky":
                return new Vector3(14.5f, 17f, 0f);

            case "inky":
                return new Vector3(16.5f, 17f, 0f);

            case "clyde":
                return new Vector3(12.5f, 17f, 0f);
        }

        return new Vector3();
    }


    // 对状态机进行改变
    void animate()
    {
        Vector3 dir = waypoint - transform.position;
        GetComponent<Animator>().SetFloat("DirX", dir.x);
        GetComponent<Animator>().SetFloat("DirY", dir.y);
        GetComponent<Animator>().SetBool("Run", false);
    }

    // 
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name == "pacman")
        {
            //Destroy(other.gameObject);
            if (state == State.Run)
            {
                Calm();
                InitializeGhost(_startPos);
                pacman.UpdateScore();
            }

            else
            {
                _gm.LoseLife();
            }

        }
    }


    // 等待中建立好状态 将路径清空
    void Wait()
    {
        if (Time.time >= timeToEndWait)
        {
            state = State.Init;
            waypoints.Clear();
            InitializeWaypoints(state);
        }

        // 往下一个路径点走
        MoveToWaypoint(true);
    }

    // 
    void Init()
    {
        _timeToWhite = 0;

        // 更新状态 恢复Scatter状态
        if (waypoints.Count == 0)
        {
            state = State.Scatter;

            // 获取姓名 改变其方向
            string name = GetComponent<SpriteRenderer>().sprite.name;
            if (name[name.Length - 1] == '0' || name[name.Length - 1] == '1') direction = Vector3.right;
            if (name[name.Length - 1] == '2' || name[name.Length - 1] == '3') direction = Vector3.left;
            if (name[name.Length - 1] == '4' || name[name.Length - 1] == '5') direction = Vector3.up;
            if (name[name.Length - 1] == '6' || name[name.Length - 1] == '7') direction = Vector3.down;

            InitializeWaypoints(state);
            timeToEndScatter = Time.time + scatterLength;

            return;
        }

        // 向下一方向行动
        MoveToWaypoint();
    }

    // 如果Scatter时间未到 则继续
    // 时间到了 则进入下一状态
    void Scatter()
    {
        if (Time.time >= timeToEndScatter)
        {
            waypoints.Clear();
            state = State.Chase;
            return;
        }

        // 朝下一个路径点运动
        MoveToWaypoint(true);

    }


    void ChaseAI()
    {

        // 如果不是路径 则不断向其逼近
        if (Vector3.Distance(transform.position, waypoint) > 0.000000000001)
        {
            Vector2 p = Vector2.MoveTowards(transform.position, waypoint, speed);
            GetComponent<Rigidbody2D>().MovePosition(p);
        }

        // 如果在路径上按照AI运动
        else GetComponent<AI>().AILogic();

    }

    void RunAway()
    {
        GetComponent<Animator>().SetBool("Run", true);

        if (Time.time >= _timeToWhite && Time.time >= _timeToToggleWhite) ToggleBlueWhite();

        // 如果不在路径上 就朝目标移动
        if (Vector3.Distance(transform.position, waypoint) > 0.000000000001)
        {
            Vector2 p = Vector2.MoveTowards(transform.position, waypoint, speed);
            GetComponent<Rigidbody2D>().MovePosition(p);
        }

        // 如果在路径上按照AI运动
        else GetComponent<AI>().RunLogic();

    }


    // 从队列中读取出路径位置
    void MoveToWaypoint(bool loop = false)
    {
        waypoint = waypoints.Peek();		// 读取路径点
        // 没到路径点
        if (Vector3.Distance(transform.position, waypoint) > 0.000000000001)	
        {									                        
            _direction = Vector3.Normalize(waypoint - transform.position);	
            Vector2 p = Vector2.MoveTowards(transform.position, waypoint, speed);
            GetComponent<Rigidbody2D>().MovePosition(p);
        }
        else 	// 如果在路径上从队列中出来
        {
            if (loop) waypoints.Enqueue(waypoints.Dequeue());
            else waypoints.Dequeue();
        }
    }

    // 处于受惊状态下时 立即往回走 并设置状态机
    public void Frighten()
    {
        state = State.Run;
        _direction *= -1;

        _timeToWhite = Time.time + _gm.scareLength * 0.66f;
        _timeToToggleWhite = _timeToWhite;
        GetComponent<Animator>().SetBool("Run_White", false);

    }

    public void Calm()
    {
        // 如果鬼不在奔跑状态就直接返回 即保持不动
        if (state != State.Run) return;

        waypoints.Clear();
        state = State.Chase;
        _timeToToggleWhite = 0;
        _timeToWhite = 0;
        GetComponent<Animator>().SetBool("Run_White", false);
        GetComponent<Animator>().SetBool("Run", false);
    }

    // 改变状态机 将里面的状态根据一小段时间来回替换
    public void ToggleBlueWhite()
    {
        isWhite = !isWhite;
        GetComponent<Animator>().SetBool("Run_White", isWhite);
        _timeToToggleWhite = Time.time + _toggleInterval;
    }

}
