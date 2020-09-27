using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerYukidamaManager : MonoBehaviour
{
    private PlayerStatusController PSC;
    private PlayerController PC;
    //private Player_Audio PA;


    private enum Move_dir
    {
        Left, Right, Stop
    }
    private Move_dir movedirection = Move_dir.Stop;

    //整数型

    private int charge = 0;
    private int charge_MAX = 20;

    //スピンアタック関連変数
    private int spin_counter = 0;//スピンアタックで浮くことができる回数。

    //真偽型

    protected bool YUKIDAMA_cancreate = true;
    bool[] yukidama_floatSafety = new bool[3];
    private bool Destroyflag;

    private bool ChargeMode = false;
    private bool Input_AttackButton = false;

    private Rigidbody2D rbody;
    private Animator anim;

    public GameObject SnowBall;//攻撃用雪玉
    private GameObject cloneSnow;//生み出した雪玉
    public GameObject[] ball = new GameObject[6];//雪玉溜めるときに生成される奴

    private bool AddPower = true;//falseのときは雪玉のため動作を受け付けず、同じ雪玉を再びふゆかの頭の上には乗らない


    private bool onSnowBall = false;//こいつがtrueのとき、雪玉に乗り続ける
    private bool JumpOnYukidama = false;//雪玉の上でジャンプしたらfalseになる。雪玉以外の地面に着地するとtrueに戻る
    private bool OnceOnJump = false;//雪玉からジャンプで降りるとき一回だけ発動。どこかに着地するとtrueに戻る

    private bool SpinAttack = false;
    //UpdateでSpinとかの読み取りをしていく

    public LayerMask SnowBallLayer;//雪玉のこと


    public AudioClip SpinSE;
    public AudioClip ChargeSE;
    private void Start()
    {
        PC = GetComponent<PlayerController>();
        PSC = GetComponent<PlayerStatusController>();
        //PA = GetComponent<Player_Audio>();
        rbody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        yukidama_floatSafety[0] = true;
        yukidama_floatSafety[1] = true;
        yukidama_floatSafety[2] = true;
    }
    private void Update()
    {
        Debug.Log(rbody.velocity);
        GameObject snowrbody = cloneSnow;

        //玉乗りギミック
        //雪玉発射中かつ、ふゆかは止まっているかつ、ジャンプボタンが押されないかつ、
        if (!AddPower)//雪玉に乗っているときの処理
        {
            if (movedirection == Move_dir.Stop && JumpOnYukidama)
            {
                Debug.Log("１");
                if (onSnowBall)
                {
                    Debug.Log("2");
                    
                    Vector3 yukipos = this.transform.position;
                    yukipos.x = cloneSnow.transform.position.x;
                    yukipos.y = cloneSnow.transform.position.y + 0.3f;
                    this.transform.position = yukipos;
                    

                    


                    rbody.velocity = new Vector2(snowrbody.GetComponent<Rigidbody2D>().velocity.x, snowrbody.GetComponent<Rigidbody2D>().velocity.y);
                }
            }
        }
        else
        {
            //if (!JumpOnYukidama) PSC.JumpSetter(false);
        }

        //すぐに消す
        Debug.Log("clonesnow = " + cloneSnow);
        Debug.Log("onSnowBall = " + onSnowBall);

        if (cloneSnow)//そもそも雪玉が無い時に探知する必要はない
        {
            onSnowBall = Physics2D.Linecast(transform.position - (transform.up * 0.1f), transform.position - (transform.up * 0.15f) + (transform.right * 0.1f), SnowBallLayer)
                   || Physics2D.Linecast(transform.position - (transform.up * 0.1f), transform.position - (transform.up * 0.15f) - (transform.right * 0.1f), SnowBallLayer);
        }
        else
        {
            onSnowBall = false;
        }
        /*
        if (onSnowBall)
        {
            onSnowBall = false;
            AlwaysOnSnowBall = true;
        }
        */
        if (Input_AttackButton && AddPower)
        {
            AttackCharge();
        }

        //ここでデストロイ判定管理も行うと、アンドロイドでも雪玉が消えてくれる
        if (!AddPower)
        {
            if (Destroyflag && (cloneSnow.transform.position.y <= -25 || cloneSnow.GetComponent<Rigidbody2D>().velocity == Vector2.zero))
            {
                cloneSnow.GetComponent<Yukidama_PrefabManager>().Destroyed();
                Destroyflag = false;
                AddPower = true;
            }
        }

        if (PSC.JumpGetter())
        {
            spin_counter = 0;
        }

    }

    private void AttackCharge()//スピン、雪玉の処理
    {
        ChargeMode = !ChargeMode;
        if (ChargeMode)
        {
            charge++;

            if (charge >= 1 && yukidama_floatSafety[0])
            {
                charge = 1;
                yukidama_floatSafety[0] = false;
            }
            if (charge >= charge_MAX && yukidama_floatSafety[1])
            {
                charge = charge_MAX;
                yukidama_floatSafety[1] = false;
            }

            if (charge == 1f)
            {
                if (YUKIDAMA_cancreate)
                {
                    ball[0].SetActive(true);
                    ball[1].SetActive(true);
                    ball[2].SetActive(true);
                    ball[3].SetActive(true);
                    ball[4].SetActive(true);
                    ball[5].SetActive(true);
                    
                    YUKIDAMA_cancreate = false;

                    //PA.ChargeAudio();
                    PlayAudio(ChargeSE);
                }

            }
            else if (charge < charge_MAX)
            {
                ball[0].transform.position = Vector3.Lerp(new Vector3(this.transform.position.x + 1, this.transform.position.y + 2.8f, this.transform.position.z), new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), (float)charge / charge_MAX);
                ball[0].transform.RotateAround(new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), Vector3.forward, -15f * charge);

                ball[1].transform.position = Vector3.Lerp(new Vector3(this.transform.position.x + 2, this.transform.position.y + 1.4f, this.transform.position.z), new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), (float)charge / charge_MAX);
                ball[1].transform.RotateAround(new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), Vector3.forward, -15f * charge);

                ball[2].transform.position = Vector3.Lerp(new Vector3(this.transform.position.x + 1, this.transform.position.y - 0.4f, this.transform.position.z), new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), (float)charge / charge_MAX);
                ball[2].transform.RotateAround(new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), Vector3.forward, -15f * charge);

                ball[3].transform.position = Vector3.Lerp(new Vector3(this.transform.position.x - 1, this.transform.position.y - 0.4f, this.transform.position.z), new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), (float)charge / charge_MAX);
                ball[3].transform.RotateAround(new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), Vector3.forward, -15f * charge);

                ball[4].transform.position = Vector3.Lerp(new Vector3(this.transform.position.x - 2, this.transform.position.y + 1.4f, this.transform.position.z), new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), (float)charge / charge_MAX);
                ball[4].transform.RotateAround(new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), Vector3.forward, -15f * charge);

                ball[5].transform.position = Vector3.Lerp(new Vector3(this.transform.position.x - 1, this.transform.position.y + 2.8f, this.transform.position.z), new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), (float)charge / charge_MAX);
                ball[5].transform.RotateAround(new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), Vector3.forward, -15f * charge);

                /*//これと同じも内容のものをダメージ受けた時に外部から受け付ける
                if (!Move_Ok)
                {
                    charge = 0;
                    ChargeMode = false;
                }
                */
            }

            else if (charge == charge_MAX)
            {
                ball[0].SetActive(false);
                ball[1].SetActive(false);
                ball[2].SetActive(false);
                ball[3].SetActive(false);
                ball[4].SetActive(false);
                ball[5].SetActive(false);
                cloneSnow = Instantiate(SnowBall, new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), Quaternion.identity);
            }
            else
            {
                cloneSnow.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
                if (charge >= 20) Hold_SnowBall();
            }
        }

    }

    public void Spin_Up()
    {
        ball[0].SetActive(false);
        ball[1].SetActive(false);
        ball[2].SetActive(false);
        ball[3].SetActive(false);
        ball[4].SetActive(false);
        ball[5].SetActive(false);

        if (charge < charge_MAX)//スピンジャンプ処理に移る場合
        {
            //PA.StopAudio();
            PlayAudio(null);
            charge = 0;
            YUKIDAMA_cancreate = true;

            SpinJump();
        }
        if (charge >= charge_MAX)//雪玉処理に移る場合
        {
            charge = 0;
            YUKIDAMA_cancreate = true;
            yukidama_floatSafety[0] = true;
            yukidama_floatSafety[1] = true;

            if (AddPower)
            {
                Yukidama_Shot();
                Destroyflag = true;
                AddPower = false;//AddForceを一度この瞬間にしか発動させないため
            }

            //cloneSnow.GetComponent<SnowBallManager_Mk3>().shotflag_Set(true);
            //AddPower = true;
            //Yukidama_Shot();
        }
    }

    private void Yukidama_Shot()
    {
        if (PSC.RightGetter() == true)
        {
            cloneSnow.GetComponent<Rigidbody2D>().velocity = (Vector3.right * 4 * 3f - Vector3.up * 6f);
        }
        if (PSC.LeftGetter() == true)
        {
            cloneSnow.GetComponent<Rigidbody2D>().velocity = (Vector3.left * 4 * 3f - Vector3.up * 6f);
        }
    }

    //ジャンプの機能を付けるまでいったんコメントアウト
    public void SpinJump()
    {
        if (!PSC.JumpGetter())
        {
            //PA.SpinAudio();
            PlayOneShotAudio(SpinSE);
            StartCoroutine(SpinAnim());//ジャンプ中、スピンのアニメーション＆スピン中は雑魚敵なら倒せるようにする   
        }
        if (!PSC.JumpGetter())//地面から浮いていた場合に発生
        {
            if (spin_counter <= 2 && spin_counter >= 0 && rbody.velocity.y < 0)//降下開始から一度もスピンしていない場合
            {
                switch (movedirection)
                {
                    case Move_dir.Left:

                        rbody.velocity = new Vector2(rbody.velocity.x, 0);
                        break;
                    case Move_dir.Right:

                        rbody.velocity = new Vector2(rbody.velocity.x, 0);
                        break;
                    case Move_dir.Stop:

                        rbody.velocity = new Vector2(0, 0);
                        break;
                }
                rbody.AddForce(Vector3.up * 250 * spin_counter);
                spin_counter++;
            }
        }
    }

    private IEnumerator SpinAnim()
    {
        anim.SetBool("CanSpin", true);
        SpinAttack = true;
        yield return new WaitForSeconds(0.25f);
        anim.SetBool("CanSpin", false);
        SpinAttack = false;
    }

    private void Hold_SnowBall()
    {
        cloneSnow.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z);
    }

    //ここからはセッターゲッター類
    public void ChargeMode_Setter(bool Setter)
    {
        Input_AttackButton = Setter;
    }

    public void movedirection_Setter(int Setter)
    {
        //０…停止
        //１…右
        //２…左
        if (Setter == 0)
        {
            movedirection = Move_dir.Stop;
        }
        if (Setter == 1)
        {
            movedirection = Move_dir.Right;
        }
        if (Setter == 2)
        {
            movedirection = Move_dir.Left;
        }
    }

    public void PlayerDamageProcess(bool Setter)
    {
        //雪玉精製を中断させる
        Input_AttackButton = Setter;

        ball[0].SetActive(false);
        ball[1].SetActive(false);
        ball[2].SetActive(false);
        ball[3].SetActive(false);
        ball[4].SetActive(false);
        ball[5].SetActive(false);

        charge = 0;

        //もしも既に雪玉が生成されていたら、強制で雪玉を発射させる
        if (cloneSnow)
        {
            //雪玉が存在していたら実行
            Yukidama_Shot();
        }
    }

    public bool OnSnowballGetter()
    {
        return onSnowBall;
    }
    public bool SpinAttackGetter()
    {
        return SpinAttack;
    }

    public void YukidamaJump(bool Setter)
    {
        JumpOnYukidama = Setter;
    }

    //このスクリプトが持つAudioClipを鳴らす
    private void PlayAudio(AudioClip AC)
    {
        var PAM = GetComponent<Player_AudioManager>();
        if(AC != null)
        {
            if (PAM != null) PAM.AudioPlayer(AC);
        }
        else
        {
            PAM.AudioStop();
        }
    }
    private void PlayOneShotAudio(AudioClip AC)
    {
        var PAM = GetComponent<Player_AudioManager>();
        if(AC != null)
        {
            PAM.AudioPlayerOnplayOneShot(AC);
        }
    }
}
