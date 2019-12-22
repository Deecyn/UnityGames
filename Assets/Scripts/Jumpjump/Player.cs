using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{

    // 距离的影响因素，如按压 1 秒中时能跳多远
    public float DistanceFactor;

    // 新生成盒子的最大的随机距离
    public float StageMaxDistance = 5;

    // 最开始的盒子对象
    public GameObject InitStage;

    // 跟随小人当前位置的相机
    public Transform CurrentCamera;

    // 跟随小人的粒子效果对象
    public GameObject Particle;

    // 小人头部
    public Transform Head;
    // 小人身体部分
    public Transform Body;

    // 总分显示框
    public Text TotalScoreText;

    // 每次得分时显示的 +1 文本
    public Text SingleScoreText;

    //public GameObject SaveScorePannel;
    //public InputField NameField;
    //public Button SaveButton;
    //public GameObject RankPanel;
    //public GameObject RankItem;

    //　是否更新了加分显示的动画
    private bool _isUpdateScoreAnimation = false;

    // 加分显示动画的更新时间
    private float _scoreAnimationTime;

    // 当前分数
    private int _score;
    // 相机的相对位置
    private Vector3 _cameraRelativePostion;

    // 小人当前所在的盒子对象
    private GameObject _currentStage;

    // 记录小人最近一次碰撞的物体
    private Collider _lastCollisionCollider;

    // 小人--刚体
    private Rigidbody _rigibody;

    // 用户按键以开始游戏的时间
    private float _startTime;

    // 设置新盒子的生成的方向，初始为 x 轴的正方向
    private Vector3 newStageDirection = new Vector3(1, 0, 0);

    /// 游戏开始时进行的操作
    void Start()
    {
       
        // 获取刚体组件
        _rigibody = GetComponent<Rigidbody>();
        // 设置重心为底部，防止倾倒
        _rigibody.centerOfMass = new Vector3(0,0,0);

        _currentStage = InitStage;
        _lastCollisionCollider = _currentStage.GetComponent<Collider>();
        NewStage();

        // 初始化为相机的位置减去当前小人所在的位置
        _cameraRelativePostion = CurrentCamera.position - transform.position;

        // 为保存分数按钮绑定事件
        //SaveButton.onClick.AddListener(OnClickSaveButton);

    }

    /// 游戏中更新状态的操作
    void Update()
    {
       
        // 按下空格键的时候
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 赋值为当前时间
            _startTime = Time.time;
            Particle.SetActive(true);
        }

        // 松开空格键的时候
        if (Input.GetKeyUp(KeyCode.Space))
        {
            var pressTime = Time.time - _startTime;
            OnJump(pressTime);
            Particle.SetActive(false);

            // 将小人恢复原状。这里的 0.1f 表示 new Vector3(0.1f,0.1f,0.1f)，0.2f 表示动画时长
            Body.transform.DOScale(0.1f, 0.2f);
            Head.transform.DOLocalMoveY(0.25f, 0.2f);

            // 将盒子恢复原状
            _currentStage.transform.DOLocalMoveY(0.25f,0.2f);
            _currentStage.transform.DOScale(new Vector3(1,0.5f,1),0.2f);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            // 身体部分的蓄力动画，四周向内缩。deltaTime 表示每一帧渲染的时间，0.05f 表示缩放的速率
            Body.transform.localScale += new Vector3(1, -1, 1) * 0.05f * Time.deltaTime;
            // 头部的蓄力动画，位置下降，0.1f 表示下降的速率
            Head.transform.localPosition += new Vector3(0, -1, 0) * 0.1f * Time.deltaTime;

            // 盒子的蓄力动画
            _currentStage.transform.localScale += new Vector3(0, -1, 0) * 0.15f * Time.deltaTime;
            _currentStage.transform.localPosition += new Vector3(0, -1, 0) * 0.15f * Time.deltaTime;
        }

        if (_isUpdateScoreAnimation)
        {
            UpdateScoreAnimation();
        }
    }

    /// <summary>
    /// 根据按键的时间，定义跳的距离，即下次的落点
    /// </summary>
    /// <param name="pressTime"></param>
    void OnJump(float pressTime)
    {
        // 定义刚体跳跃的方向、距离
        _rigibody.AddForce((new Vector3(0, 1, 0) + newStageDirection )* pressTime * DistanceFactor,ForceMode.Impulse);
    }

    /// <summary>
    /// 生成新的盒子
    /// </summary>
    void NewStage()
    {
        var stage = Instantiate(InitStage);
        // 设置新盒子的位置：延原盒子的 x 轴方向扩展，
        // 生成的最小距离为 1.1f，最大距离为 StageMaxDistance；生成的方向由随机的矢量值 newStageDirection 决定
        stage.transform.position = _currentStage.transform.position + newStageDirection * Random.Range(1.1f, StageMaxDistance);

        // 设置盒子随机大小
        var randomScale = Random.Range(0.5f,1);
        stage.transform.localScale = new Vector3(randomScale, 0.5f, randomScale);

        // 设置盒子的随机颜色
        stage.GetComponent<Renderer>().material.color = new Color(Random.Range(0f, 1), Random.Range(0f, 1), Random.Range(0f, 1));
    }

    /// <summary>
    /// 当刚体小人与其他物体碰撞时:
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
       
        // 当小人此次碰撞的物体为盒子，且这个盒子不是小球上一次碰撞后所在的盒子时
        if (collision.gameObject.name.Contains("Stage") && collision.collider != _lastCollisionCollider)
        {
            // 重新指定当前盒子，并生成新的盒子
            _lastCollisionCollider = collision.collider;
            _currentStage = collision.gameObject;
            RandomStageDirection();
            NewStage();
            MoveCamera();
            ShowScoreAnimation();

            _score++;
            TotalScoreText.text = _score.ToString();
        }

        if (collision.gameObject.name == "Ground")
        {
            // "0" 代表 build settings 里面配置的序号为 0 的场景
           SceneManager.LoadScene(0);

            // 本局游戏结束，显示上传分数 pannel
            //SaveScorePannel.SetActive(true);
        }
    }

    // 显示分数的动画
    void ShowScoreAnimation()
    {
        _isUpdateScoreAnimation = true;
        _scoreAnimationTime = Time.time;
       
    }

    // 随着小人位置的变化，更新加分显示框的动画位置
    void UpdateScoreAnimation()
    {
        // 每隔一段时间，设置需要更新动画
        if (Time.time - _scoreAnimationTime > 1)
        {
            _isUpdateScoreAnimation = false;
        }

        // 设置初始位置为小人的位置；将小人的三维位置转化为文本框的二维位置
        var playerScreenPos = RectTransformUtility.WorldToScreenPoint(CurrentCamera.GetComponent<Camera>(), transform.position);
        // 设置显示的位置渐变
        SingleScoreText.transform.position = playerScreenPos + 
            Vector2.Lerp(Vector2.zero, new Vector2(0, 200),Time.time - _scoreAnimationTime);
        // 设置文本颜色的渐变
        SingleScoreText.color = Color.Lerp(Color.black, new Color(0, 0, 0, 0), Time.time - _scoreAnimationTime);
    }


    void RandomStageDirection()
    {
        var seed = Random.Range(0, 2);
        if (seed == 0)
        {
            newStageDirection = new Vector3(1, 0, 0);
        }
        else
        {
            newStageDirection = new Vector3(0, 0, 1);
        }
    }

    // 移动相机
    void MoveCamera()
    {
        // 移动相机，1 代表移动时，动画的时长
       CurrentCamera.DOMove(transform.position + _cameraRelativePostion,1);
    }

    // 保存分数的按钮
    void OnClickSaveButton()
    {
        //var nickname = NameField.text;
        var score =  _score;

        // 上传数据到服务器
        Post();

    }

    IEnumerator Post()
    {
        Debug.Log("提交分数");
        WWWForm from = new WWWForm();

        from.AddField("name", "testzhang");
        from.AddField("score", 17);

        UnityWebRequest webRequest = UnityWebRequest.Post("http://dawsson.qicp.vip/api/updateScore",from);
        yield return webRequest.SendWebRequest();

        if (webRequest.isHttpError || webRequest.isNetworkError)
        {
            Debug.Log(webRequest.error);
        }
        else
        {
            Debug.Log(webRequest.downloadHandler.text);
        }
    }

    // 显示分数排行的 panel
    void ShowRankPanel()
    {
        // 从服务器获取数据,然后到列表
       
    }


}
