using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

//跳一跳：王德鑫：游戏整合，角色跳跃，盒子生成，碰撞检测，
//王志强：相机跟随，飘分动画，背景变色

public class Player : MonoBehaviour
{
    // 小人跳跃时，影响远近的一个参数
    public float Factor;

    // 新生成盒子的最远的随机距离
    public float MaxDistance = 5;

    // 初始的盒子物体
    public GameObject Stage;

    // 盒子仓库，可以放上各种盒子的prefab，用于动态生成。
    public GameObject[] BoxTemplates;

    // 左上角总分的 UI 文本框组件
    public Text TotalScoreText;

    // 角色蓄力时的粒子效果
    public GameObject Particle;

    // 角色的头部
    public Transform Head;

    // 角色的身体
    public Transform Body;

    // 跳跃成功时，飘分的UI组件
    public Text SingleScoreText;

    // 游戏结束面板面板
    public GameObject RankPanel;

    // 重新开始按钮
    public Button RestartButton;

    // 退出按钮
    public Button ExitButton;

    // 展示得分的文本框
    public Text ShowScoreText;

    // 角色的刚体
    private Rigidbody _rigidbody;

    // 用户按键以开始游戏的时间
    private float _startTime;
    // 角色当前所在的盒子对象
    private GameObject _currentStage;

    // 相机的相对位置
    private Vector3 _cameraRelativePosition;

    // 当前总分
    private int _score;
    //　是否更新了飘分显示的动画
    private bool _isUpdateScoreAnimation;
    // 飘分动画的开始显示时间
    private float _scoreAnimationStartTime;


    // 设置新盒子的生成的方向，初始为 x 轴的正方向
    Vector3 _direction = new Vector3(1, 0, 0);
    // 上次得分的分数
    private int _lastReward = 1;
    private bool _enableInput = true;

    // Use this for initialization
    void Start()
    {
        // 得到角色的刚体组件
        _rigidbody = GetComponent<Rigidbody>();
        // 设置重心为底部，防止倾倒
        _rigidbody.centerOfMass = new Vector3(0, 0, 0);
        // 设置当前台阶
        _currentStage = Stage;
        SpawnStage();
        // 设置相机的相对位置差
        _cameraRelativePosition = Camera.main.transform.position - transform.position;

        // 初始化重新开始按钮的点击事件
        RestartButton.onClick.AddListener(() => { SceneManager.LoadScene(5); });
        // 初始化退出按钮的点击事件
        ExitButton.onClick.AddListener(() => { SceneManager.LoadScene(0); });
    }

    // Update is called once per frame
    void Update()
    {
        if (_enableInput)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                _startTime = Time.time;
                Particle.SetActive(true);
            }

            if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space))
            {
                // 计算总共按下空格的时长
                var elapse = Time.time - _startTime;
                OnJump(elapse);
                Particle.SetActive(false);

                //还原小人的形状
                Body.transform.DOScale(0.1f, 0.2f);
                Head.transform.DOLocalMoveY(0.29f, 0.2f);

                //还原盒子的形状
                _currentStage.transform.DOLocalMoveY(-0.25f, 0.2f);
                _currentStage.transform.DOScaleY(0.5f, 0.2f);

                _enableInput = false;
            }

            // 处理按下空格时小人和盒子的动画
            if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
            {
                //添加限定，盒子和角色最多缩放一半
                if (_currentStage.transform.localScale.y > 0.3)
                {
                    Body.transform.localScale += new Vector3(1, -1, 1) * 0.05f * Time.deltaTime;
                    Head.transform.localPosition += new Vector3(0, -1, 0) * 0.1f * Time.deltaTime;

                    _currentStage.transform.localScale += new Vector3(0, -1, 0) * 0.15f * Time.deltaTime;
                    _currentStage.transform.localPosition += new Vector3(0, -1, 0) * 0.15f * Time.deltaTime;
                }
            }
        }

        // 是否显示飘分效果
        if (_isUpdateScoreAnimation)
            UpdateScoreAnimation();
    }

    // 跳跃
    void OnJump(float elapse)
    {
        // 向刚体施加力，使其运动
        _rigidbody.AddForce(new Vector3(0, 5f, 0) + (_direction) * elapse * Factor, ForceMode.Impulse);
        // 角色在空中旋转
        transform.DOLocalRotate(new Vector3(0, 0, -360), 0.6f, RotateMode.LocalAxisAdd);
    }

    // 生成盒子
    void SpawnStage()
    {
        GameObject prefab;
        if (BoxTemplates.Length > 0)
        {
            // 从盒子库中随机取盒子进行动态生成
            prefab = BoxTemplates[Random.Range(0, BoxTemplates.Length)];
        }
        else
        {
            prefab = Stage;
        }

        var stage = Instantiate(prefab);
        // 随机盒子的位置
        stage.transform.position = _currentStage.transform.position + _direction * Random.Range(1.1f, MaxDistance);

        // 随机盒子的大小
        var randomScale = Random.Range(0.5f, 1);
        stage.transform.localScale = new Vector3(randomScale, 0.5f, randomScale);

        // 随机盒子的颜色
        stage.GetComponent<Renderer>().material.color =
            new Color(Random.Range(0f, 1), Random.Range(0f, 1), Random.Range(0f, 1));
    }

    void OnCollisionExit(Collision collision)
    {
        _enableInput = false;
    }

    // 小人刚体与其他物体发生碰撞时自动调用
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Ground")
        {
            OnGameOver();
        }
        else
        {
            if (_currentStage != collision.gameObject)
            {
                // 碰撞到的点的数组，即碰撞点
                var contacts = collision.contacts;

                // 碰撞到的第一个点法线向量
                if (contacts.Length == 1 && contacts[0].normal == Vector3.up)
                {
                    _currentStage = collision.gameObject;
                    AddScore(contacts);
                    RandomDirection();
                    SpawnStage();
                    MoveCamera();

                    _enableInput = true;
                }
                else
                {
                    OnGameOver();
                }
            }
            else
            {
                var contacts = collision.contacts;

                //check if player's feet on the stage
                if (contacts.Length == 1 && contacts[0].normal == Vector3.up)
                {
                    _enableInput = true;
                }
                else // body just collides with this box
                {
                    OnGameOver();
                }
            }
        }
    }

    /// 加分，准确度高的分数成倍增加
    /// <param name="contacts">小人与盒子的碰撞点</param>
    private void AddScore(ContactPoint[] contacts)
    {
        if (contacts.Length > 0)
        {
            var hitPoint = contacts[0].point;
            hitPoint.y = 0;

            var stagePos = _currentStage.transform.position;
            stagePos.y = 0;

            var precision = Vector3.Distance(hitPoint, stagePos);
            if (precision < 0.1)
                _lastReward *= 2;
            else
                _lastReward = 1;

            _score += _lastReward;
            TotalScoreText.text = _score.ToString();
            ShowScoreAnimation();
        }
    }

    private void OnGameOver()
    {
        ShowRankPanel();
    }

    /// 显示飘分动画
    private void ShowScoreAnimation()
    {
        _isUpdateScoreAnimation = true;
        _scoreAnimationStartTime = Time.time;
        SingleScoreText.text = "+" + _lastReward;
    }

    /// 更新飘分动画
    void UpdateScoreAnimation()
    {
        if (Time.time - _scoreAnimationStartTime > 1)
            _isUpdateScoreAnimation = false;

        // 设置初始位置为小人的位置；将小人的三维位置转化为文本框的二维位置
        var playerScreenPos =
            RectTransformUtility.WorldToScreenPoint(Camera.main, transform.position);
        // 设置显示的位置渐变
        SingleScoreText.transform.position = playerScreenPos +
                                             Vector2.Lerp(Vector2.zero, new Vector2(0, 200),
                                                 Time.time - _scoreAnimationStartTime);
        // 设置文本颜色的渐变
        SingleScoreText.color = Color.Lerp(Color.black, new Color(0, 0, 0, 0), Time.time - _scoreAnimationStartTime);
    }

    /// 随机方向
    void RandomDirection()
    {
        var seed = Random.Range(0, 2);
        _direction = seed == 0 ? new Vector3(1, 0, 0) : new Vector3(0, 0, 1);
        transform.right = _direction;
    }

    // 移动摄像机
    void MoveCamera()
    {
        Camera.main.transform.DOMove(transform.position + _cameraRelativePosition, 1);
    }

    // 显示排行榜面板
    void ShowRankPanel()
    {
        ShowScoreText.text = "你的分数：" + _score;
        RankPanel.SetActive(true);
    }
}