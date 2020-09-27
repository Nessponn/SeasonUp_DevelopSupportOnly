using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Test_GameManager : MonoBehaviour
{
    //ゲームマネージャーは、ステージのそれぞれの中間地点が違うということもあるので、それぞれ一個のものとしてシーンに一個存在する（シーンで共有するなどはしない。）

    [SerializeField]//ゲームオブジェクト
    private  GameObject Player;
    public GameObject StartFlola;
    public GameObject FlontGround;
    public GameObject MiddleGround;
    public GameObject StartPosition;
    public GameObject CheckPoint;
    public GameObject GameOver_Rogo;//ゲームオーバー時に表示

    public GameObject Water_Object;
    public GameObject SnowShadow;
    public GameObject SnowShadow_Small;
    public GameObject PlayerDEATH_Particle;

    [SerializeField]//カメラの位置
    public float Start_LeftMax = 0f;
    public float Start_RightMax = 0f;
    public float Cameraposition_y_1 = 0f;
    private float Now_Cameraposition_y;

    //プレイヤーの座標
    private float Player_x;
    private float Player_y;
    private float Player_z;

    //真偽変数
    private bool CameraNormal = true;
    private bool DEATH_Fuyuka = false;
    public bool DEATH_Fuyuka_DEBUG = false;
    public bool From_StartPosition;//デバッグ専用。trueにしておくと、中間とってもスタート地点からの再開となる（フローラ演出あり）

    //クラス変数
    public Stage1_Camera SC = new Stage1_Camera();
    public LifeManager LM = new LifeManager();
    public RestManager RM = new RestManager();
    public CheckPointManager CM = new CheckPointManager();
    public AudioManager AM = new AudioManager();

    //その他の変数
    private AudioSource AS;

    public AudioClip DEATH_SE;

    public AudioSource Main;
    public AudioSource Instrument;
    public AudioSource Boss;
    // Start is called before the first frame update


    private void Awake()
    {
        if (From_StartPosition) PlayerPrefs.SetInt("checkpoint", StartFlola.GetComponent<StartFlola>().StartPointNumber); ;
    }


    void Start()
    {
        AS = Camera.main.GetComponent<AudioSource>();

        AS.clip = AM.MainTheme;
        //AS.Play();

        Player = GameObject.FindWithTag("Player");
        LM.GetSpriteRenderer();

        if(PlayerPrefs.GetInt("checkpoint") % 2 == 0)
        {
            //Debug.Log("中間未到達");
            GetComponent<Test_GameManager>().setter_Cameraposition_y(Cameraposition_y_1);//インスペクターで設定したｙ座標がセット

            SC.LEFTMAX_set(Start_LeftMax);
            SC.RIGHTMAX_set(Start_RightMax);

            Player.SetActive(false);//登場演出を入れるため、ふゆかをfalseにしておく
            StartFlola.SetActive(true);//フローラを出現させる（フローラがふゆかのトリガーの為）
            Vector3 pos = StartPosition.transform.position;
            pos.y = getter_Cameraposition_y();
            pos.z = -10;
            Camera.main.transform.position = pos;

            
        }
        else
        {
            //Debug.Log("中間到達済");
            GetComponent<Test_GameManager>().setter_Cameraposition_y(CheckPoint.GetComponent<CheckPointAboutNumberSetter>().y_position);//インスペクターで設定したｙ座標がセット
            SC.LEFTMAX_set(CheckPoint.GetComponent<CheckPointAboutNumberSetter>().x_min);
            SC.RIGHTMAX_set(CheckPoint.GetComponent<CheckPointAboutNumberSetter>().x_max);



            Player.SetActive(true);//ここは単純に登場してほしいだけなので、ふゆかはtrue

            Player.transform.position = new Vector3(CheckPoint.transform.position.x, CheckPoint.transform.position.y, 0);

            Vector3 pos = CheckPoint.transform.position;
            pos.y = CheckPoint.GetComponent<CheckPointAboutNumberSetter>().y_position;
            pos.z = -10;
            Camera.main.transform.position = pos;

            
        }

        

    }

    // Update is called once per frame
    void Update()
    {
        Player_x = Player.transform.position.x;
        Player_y = Player.transform.position.y;
        Player_z = Player.transform.position.z;

        if (CameraNormal)
        {
            Vector3 pos = Player.transform.position;
            pos.y = GetComponent<Test_GameManager>().getter_Cameraposition_y();
            pos.z = -10;
            Camera.main.transform.position = pos;
        }

        SC.CameraMode_positionx();//スクロール限界をここで決める

        if (DEATH_Fuyuka_DEBUG)
        {
            DEATH_Fuyuka_DEBUG = false;
            FUYUKA_DEATH();
        }
        if (DEATH_Fuyuka)
        {
            DEATH_Fuyuka = false;
            FUYUKA_DEATH();
            Main.Stop();
            Instrument.Stop();
            Boss.Stop();
        }
    }
    
    private void FixedUpdate()
    {
        FlontGround.transform.localPosition = new Vector3((FlontGround.transform.localPosition.x - (Camera.main.transform.localPosition.x - StartPosition.transform.localPosition.x))/ 15, (FlontGround.transform.localPosition.y /100),10);
        MiddleGround.transform.localPosition = new Vector3((MiddleGround.transform.localPosition.x - (Camera.main.transform.localPosition.x - StartPosition.transform.localPosition.x)) / 30, (MiddleGround.transform.localPosition.y / 100)-1.5f, 10);
    }


    public float Player_x_Getter()
    {
        return Player_x;
    }
    public float Player_y_Getter()
    {
        return Player_y;
    }
    public float Player_z_Getter()
    {
        return Player_z;
    }


     private void FUYUKA_DEATH()
    {
        LM.LifeSetter(0);
        StartCoroutine(Player_Respawn());
    }


    public IEnumerator Player_Respawn()
    {
        //Debug.Log("シーンを読み込むよ");
        GetComponent<SCORE_MANAGER>().Rest_Decreaser();
        yield return new WaitForSeconds(3f);
        if (GetComponent<SCORE_MANAGER>().Rest_Getter() != 0)
        {
            Reload_Scene();
        }
        else
        {
            //ゲームオーバー
            GameOver_Rogo.SetActive(true);
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene("Title");
        }
    }

    public void Reload_Scene()
    {
        //Debug.Log("SceneManager.GetActiveScene().name=" + SceneManager.GetActiveScene().name);
        string scenename = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scenename);
    }

    public IEnumerator To_StageMap()//ステージのボスを倒したらこのコルーチンがスタートする
    {
        Debug.Log("ステージクリア！移動するよ");
        yield return new WaitForSeconds(3f);
    }

    public IEnumerator Clear_Road()
    {
        yield return new WaitForSeconds(3f);
    }

    public void setter_Cameraposition_y(float num)
    {
        Now_Cameraposition_y = num;
    }
    public float getter_Cameraposition_y()
    {
        return Now_Cameraposition_y;
    }
    public void CameraNormal_Set(bool Camerastatus)
    {
        CameraNormal = Camerastatus;
    }
    public void DEATH_Setter(bool Setter)
    {
        DEATH_Fuyuka = Setter;
    }

    
}

[System.Serializable]
public class BackGround
{
    private float Player_xPosition_InStart;//こいつを基準に背景を動かしていく
    private float Now_x;
    public GameObject FlontGround;

    public void SetFloat(float x)//スタート時にこいつを開始させる
    {
        Player_xPosition_InStart = x;
    }
    public float GetFloat()
    {
        return Player_xPosition_InStart;
    }
    public void SettingX_position(float x)//ここに現在のプレイヤーのx座標を上書きし続ける
    {
        Now_x = x;
    }

    public float FlontGround_position_x()//ここで計算して背景の位置を上書きし続ける
    {
        return (Player_xPosition_InStart - Now_x) / 10;
    }
}

public class Stage1_Camera
{
    private float LEFTMAX;
    private float RIGHTMAX;

    public void CameraMode_positionx()//0～9
    {
       if (Camera.main.transform.position.x >= RIGHTMAX)
       {
         　//Debug.Log("右のスクロール値が限界値に達しました");
         　Camera.main.transform.position = new Vector3(RIGHTMAX, Camera.main.transform.position.y, Camera.main.transform.position.z);
       }
       if (Camera.main.transform.position.x <= LEFTMAX)
       {
        　 //Debug.Log("左のスクロール値が限界値に達しました");
        　 Camera.main.transform.position = new Vector3(LEFTMAX, Camera.main.transform.position.y, Camera.main.transform.position.z);
       }
    }

    public void LEFTMAX_set(float num)
    {
        LEFTMAX = num;
    }
    public void RIGHTMAX_set(float num)
    {
        RIGHTMAX = num;
    }
}

[System.Serializable]
public class LifeManager
{
    public GameObject BlueButton;
    public GameObject GreenButton;
    public GameObject YellowButton;
    public GameObject RedButton;



    Animator Sp1;
    Animator Sp2;
    Animator Sp3;
    Animator Sp4;



    private int Life = 4;

    private Test_GameManager TG;

    

    public void GetSpriteRenderer()
    {
        Sp1 = BlueButton.GetComponent<Animator>();
        Sp2 = GreenButton.GetComponent<Animator>();
        Sp3 = YellowButton.GetComponent<Animator>();
        Sp4 = RedButton.GetComponent<Animator>();
    }

    public void LifeDecreser()//一回実行されるたびライフが１減り、その演出が施される。０になると残機が１減る
    {
        Life--;
        if(Life == 3)
        {
            Sp1.SetBool("Damage", true);
        }
        if (Life == 2)
        {
            Sp2.SetBool("Damage", true);
        }
        if (Life == 1)
        {
            Sp3.SetBool("Damage", true);
            Sp4.SetBool("Dangerous", true);
        }
        if (Life == 0)
        {
            Sp4.SetBool("Damage", true);
            //ここにゲームオーバーの処理を入れる
        }
    }

    public void LifeDead()//即死トラップに引っかかった時に行う処理
    {
        Life = 0;
        Sp1.SetBool("Damage", true);
        Sp2.SetBool("Damage", true);
        Sp3.SetBool("Damage", true);
        Sp4.SetBool("Damage", true);
        //Debug.Log("即死トラップに引っかかった");
    }

    public void LifeHeal()
    {
        //ライフの値をもとに戻し、ボタンの色も元に戻す
        Life = 4;
        Sp1.SetBool("Damage", false);
        Sp2.SetBool("Damage", false);
        Sp3.SetBool("Damage", false);
        Sp4.SetBool("Damage", false);

        Sp4.SetBool("Dangerous", false);
        //Debug.Log("ボタンの色をもとに戻した！");
    }

    public int LifeGetter()
    {
        return Life;
    }
    public void LifeSetter(int Setter)
    {
        Life = Setter;
    }
}

public class RestManager
{
    private int Rest;

    public void Rest_Setter(int Setter)
    {

    }
    public int Rest_Getter()
    {
        return Rest;
    }
}

public class CheckPointManager
{
    public void Set_Data(int Setter)//中間地点に到達した時、ステージをクリアした時、書き込む
    {
        PlayerPrefs.SetInt("checkpoint", Setter);
    }

    public void Load_Data()//死んだとき、再びゲームに戻るときに読み込む
    {
        
    }
}

[System.Serializable]
public class AudioManager
{
    public AudioClip MainTheme;
    public AudioClip SecretTheme;
    public AudioClip BossTheme;
    public AudioClip WaterPotya;
}
