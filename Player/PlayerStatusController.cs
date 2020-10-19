using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;

public class PlayerStatusController : MonoBehaviour,DamagableObject
{

    [SerializeField] public bool ButtonfoldOut;//ボタン折り畳み表示の可否

    [SerializeField] public GameObject BlueButton;
    [SerializeField] public GameObject GreenButton;
    [SerializeField] public GameObject YellowButton;
    [SerializeField] public GameObject RedButton;

    [SerializeField] public bool StatusfoldOut;//ライフの状態変更インスペクター折り畳み表示の可否

    [SerializeField] public bool Life;
    [SerializeField] public bool Blue = true;
    [SerializeField] public bool Green;
    [SerializeField] public bool Yellow;
    [SerializeField] public bool Red;
    [SerializeField] public bool NoLife;

    [SerializeField] public bool ItemStatusfoldOut;

    [SerializeField] public bool Nothing;
    [SerializeField] public bool Flola;
    [SerializeField] public bool IceCream;

    //詳しいインスペクター内の記述は「PlayerStateEditor」に記載してあります。

    private int LifeNum = 4;
    private int Item_number;

    //カメラの設定変数
    private Vector3 Camera_posX;//カメラのポジション。基本はｘ軸を動かす

    public float L_limit;//左のスクロール限度
    public float R_limit;//右のスクロール限度
    private float Lerping_Num;//スクロールするときの実際の値

    [SerializeField] public  float DamageWatingTime;
    private bool Damage = true;

    public bool CameraLerping;//カメラの位置が移行しているときはプレイヤーの追跡をやめる

    Animator anim;

    Animator Sp1;
    Animator Sp2;
    Animator Sp3;
    Animator Sp4;


    private void Start()
    {
        anim = GetComponent<Animator>();

        Sp1 = BlueButton.GetComponent<Animator>();
        Sp2 = GreenButton.GetComponent<Animator>();
        Sp3 = YellowButton.GetComponent<Animator>();
        Sp4 = RedButton.GetComponent<Animator>();
    }


    private float AlphaLevelSpeed;
    private void Update()
    {
        if (LifeNum < 0) LifeNum = 0;

        if (LifeNum == 4)
        {
            Sp1.SetBool("Damage", false);
            Sp2.SetBool("Damage", false);
            Sp3.SetBool("Damage", false);
            Sp4.SetBool("Damage", false);
            Sp4.SetBool("Dangerous", false);
        }
        if (LifeNum == 3)
        {
            Sp1.SetBool("Damage", true);
            Sp2.SetBool("Damage", false);
            Sp3.SetBool("Damage", false);
            Sp4.SetBool("Damage", false); 
            Sp4.SetBool("Dangerous", false);
        }
        if (LifeNum == 2)
        {
            Sp1.SetBool("Damage", true);
            Sp2.SetBool("Damage", true);
            Sp3.SetBool("Damage", false);
            Sp4.SetBool("Damage", false);
            Sp4.SetBool("Dangerous", false);
        }
        if (LifeNum == 1)
        {
            Sp1.SetBool("Damage", true);
            Sp2.SetBool("Damage", true);
            Sp3.SetBool("Damage", true);
            Sp4.SetBool("Damage", false);
            Sp4.SetBool("Dangerous", true);
        }
        if (LifeNum == 0)
        {
            Sp1.SetBool("Damage", true);
            Sp2.SetBool("Damage", true);
            Sp3.SetBool("Damage", true);
            Sp4.SetBool("Damage", true);
        }

        if (!Damage)
        {
            gameObject.GetComponent<SpriteRenderer>().color =
                new Color(1f, 1f, 1f, Mathf.Abs(Mathf.Sin(Time.time * AlphaLevelSpeed)));//無敵が発動すると点滅を開始);

        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().color =
                new Color(1f, 1f, 1f, 1f);//無敵が発動すると点滅を開始
        }

        Debug.Log("LerpingNum = " + Lerping_Num);

        //y軸は変えない。絶対に。設計が乱れる。
        //プレイヤーがステージチャンクを移動するときはプレイヤーの値を代入することをやめる

    }
    private void FixedUpdate()
    {

        if (!CameraLerping)
        {
            Vector3 pos = new Vector3(this.transform.position.x, Camera.main.transform.position.y, -110);

            pos.x = L_limit >= pos.x ? pos.x = L_limit : pos.x;
            pos.x = R_limit <= pos.x ? pos.x = R_limit : pos.x;

            Camera.main.transform.position = pos;
            //プレイヤーが画面外から出た時、一定速度でカメラが平行移動、次の場面へ

            //Camera.main.transform.position = ;
        }
        /*
        if (CameraLerping == true)
        {
            Debug.Log("現在カメラは止まっています");
            CameraLerping = true;

            Debug.Log("右から侵入");

            for(int num =0;num < 100;num++)
            {
                Debug.Log("→Num進行中");
                Camera.main.transform.position = new Vector3(Mathf.Lerp(R_Limit_L, L_Limit_R, Num / 100), Camera.main.transform.position.y, -110);
                Num++;
            }
        }
        */

        Debug.Log("CameraLerping = "+ CameraLerping);
    }


    



    public void ToggletoLifeNum()
    {
        if (Blue)   LifeNum = 4;
        if (Green)  LifeNum = 3;
        if (Yellow) LifeNum = 2;
        if (Red)    LifeNum = 1;
        if (NoLife) LifeNum = 0;

        Life_property = LifeNum;
    }

    public int Life_property
    {
        get { return LifeNum; }
        set
        {
            LifeNum = value;
        }
    }

    public int Item_property
    {
        get { return Item_number; }
        set { }
    }


    //プレイヤーの何かを規制したい場合、このメソッドを使用
    public void PlayerControll(bool Stop,bool Yukidama_Stop)
    {
        var PC = GetComponent<PlayerController>();
        if (Yukidama_Stop)
        {
            //プレイヤーのアタックを中止
            PC.Fuyuka_Attack_False();
        }
        if (Stop)
        {
            //プレイヤーの移動を中止
            PC.Fuyuka_Stop();
        }
    }
    /*
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Stage")
        {
            var camlim = col.gameObject.GetComponent<CameraLimit>();
            if (camlim != null)
            {
                //そのチャンクのcameralimitスクリプトから値を代入する
                
                Camera_Limit_Setter(camlim.Limit_L, camlim.Limit_R);
            }
            else
            {
                Debug.LogError("このチャンクには「CameraLimit」スクリプトがついていないので画面簡易を実行できません");
            }
        }
    }
*/

    private void OnTriggerExit2D(Collider2D col)
    {
        if(col.gameObject.GetComponent<CameraScroll>())
        {
            var Scrool = col.gameObject.GetComponent<CameraScroll>();
            if (GetComponent<Rigidbody2D>().velocity.x < 0)//左向きの時
            {
                StartCoroutine(ScroolCamera(Scrool.R_Limit_L,Scrool.L_Limit_R,Scrool.time));
                L_limit = Scrool.L_Limit_L;
                R_limit = Scrool.L_Limit_R;
            }
            else//右向きの時
            {
                StartCoroutine(ScroolCamera(Scrool.L_Limit_R, Scrool.R_Limit_L,Scrool.time));
                L_limit = Scrool.R_Limit_L;
                R_limit = Scrool.R_Limit_R;
            }
        }
    }

    private IEnumerator ScroolCamera(float a,float b,float time)
    {
        CameraLerping = true;

        float timeflame = 1 / (60 * time);//フレーム計算式
        
        for(float num = 0;num <= 1; num += timeflame)
        {
            Camera.main.transform.position = new Vector3(Mathf.Lerp(a, b, num), Camera.main.transform.position.y, Camera.main.transform.position.z);
            yield return new WaitForSeconds(1/60);//画面のレンダリングの更新速度
        }

        CameraLerping = false;//移動を終えたらカメラの追従に移行させる
    }


    public void Camera_Limit_Setter(float L_Limit, float R_Limit)
    {
        //まず移行アニメーションを実行してから値の実行を行う
        //いきなり代入すると、移行するのではなくワープしてしまっているように見えるため

        /*
        CameraLerping = true;

        //左から侵入した場合
        if (!this.gameObject.GetComponent<SpriteRenderer>().flipX)
        {
            for(int Num = 0;Num <= 100; Num++)
            {
                Debug.Log("繰り返し処理");
                Debug.Log("R_limit =" + R_limit);
                Debug.Log("L_Limit = " + L_Limit);
                Lerping_Num = Mathf.Lerp(R_limit,L_Limit,Num/100);
            }
        }
        //右から侵入した場合
        if (this.gameObject.GetComponent<SpriteRenderer>().flipX)
        {
            for (int Num = 0; Num <= 100; Num++)
            {
                Debug.Log("繰り返し処理");
                //Debug.Log("R_Limit =" + R_Limit);
                //Debug.Log("L_limit = " + L_limit);
                Lerping_Num = Mathf.Lerp(L_limit, R_Limit, Num / 100);
                
            }
        }
        //移行後に初めて値の代入がなされる

        L_limit = L_Limit;
        R_limit = R_Limit;

        CameraLerping = false;
        */


    }
    /*
    public Vector3 Camera_Property
    {
        get
        {
            //カメラは常にプレイヤーの中心を追い続ける
            //しかし、カメラが移動制限を超えようとしたら、最大値もしくは最小値に値をセットする
            //画面移動する場合、このメソッドの処理を中断し、別のメソッドでカメラに値を代入する
            

            //Debug.Log("カメラの値　get＝ "+Camera_posX.x);


            return new Vector3(Camera_posX.x, Camera_posX.y, -100);
        }
        //プレイヤーが８以上Limitの値から差が出た場合、カメラを次の場面に高速移動

        set
        {
            Camera_posX = value;
            if (L_limit >= Camera_posX.x && !CameraLerping) Camera_posX.x = L_limit;
            if (R_limit <= Camera_posX.x && !CameraLerping) Camera_posX.x = R_limit;
            //Debug.Log("カメラの値　set＝ " + Camera_posX.x);
        }
    }
    */
    public void _TakeDamage()
    {
        Debug.Log("プレイヤーはダメージを受けました！");

        //ダメージが通る状態だったら、ダメージを通す
        if (Damage)
        {
            //プレイヤーがダメージを食らった時の挙動をここに記す
            PlayerControll(true, true);//移動と攻撃を中断させる


            StartCoroutine(DamageSpan());
            Life_property--;

            GetComponent<Rigidbody2D>().velocity = new Vector2(-GetComponent<Rigidbody2D>().velocity.x, 6);

        }
    }

    public void _YukidamaDamage()
    {
        return;
    }
    public void _SpinDamage()
    {
        return;
    }
    public void _WaterDEATH()
    {

    }

    private IEnumerator DamageSpan()
    {
        Damage = false;
        AlphaLevelSpeed = 10;//点滅の速さ

        GetComponent<PlayerController>().enabled = false;
        anim.SetBool("Damage", true);

        yield return new WaitForSeconds(DamageWatingTime * 2 / 10);
        GetComponent<PlayerController>().enabled = true;
        anim.SetBool("Damage", false);

        yield return new WaitForSeconds(DamageWatingTime * 5 / 10);
        AlphaLevelSpeed = 20;//点滅の速さ

        yield return new WaitForSeconds(DamageWatingTime * 3 / 10);
        Damage = true;
    }

    public bool SpinStateGetter()//スピン中であるかを検査する
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
    }
}
