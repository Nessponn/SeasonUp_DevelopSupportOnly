using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class PlayerYukidamaManager : MonoBehaviour
{
    
    //整数型

    private int charge = 0;
    private int charge_MAX = 25;//チャージに掛かるフレーム数。フローラがいれば7フレーム早くなるようにする

    //スピンアタック関連変数
    private int spin_counter = 0;//スピンアタックで浮くことができる回数。

    //真偽型

    protected bool YUKIDAMA_cancreate = true;
    bool[] yukidama_floatSafety = new bool[3];
    private bool Destroyflag;

    private bool ChargeMode = false;
    

    private Rigidbody2D rbody;
    private Animator anim;

    public GameObject SnowBall;//攻撃用雪玉
    private GameObject[] ball = new GameObject[6];//雪玉溜めるときに生成される奴
    public GameObject shadowsnow;//雪玉を集めているときのパーティクル
    public GameObject Instantshadowsnow;//雪玉を生成した時のパーティクル

    private bool onSnowBall = false;//こいつがtrueのとき、雪玉に乗り続ける
    private bool SpinAttack = false;

    //向きの判定
    private bool left;
    private bool right = true;//最初に未操作だった場合は右向きをデフォルトにする

    //UpdateでSpinとかの読み取りをしていく

    public LayerMask SnowBallLayer;//雪玉のこと


    public AudioClip SpinSE;
    public AudioClip ChargeSE;
    private void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public void ChargeMode_Setter(bool Setter)
    {
        Input_AttackButton = Setter;
    }

    public void ChengeVector_Left()//左ボタンが押されたとき、雪玉の飛ばす方向を左に変更する
    {
        left = true;
        right = false;
    }

    public void ChengeVector_Right()//右ボタンが押されたとき、雪玉の飛ばす方向を右に変更する
    {
        left = false;
        right = true;
    }

    public void Throwing()
    {
        throwingBall = false;
    }

    //雪玉のフラグ全般のbool
    private bool Input_AttackButton = false;//攻撃ボタンが押されたかの入力
    //private bool AddPower = true;//falseのときは雪玉のため動作を受け付けず、同じ雪玉を再びふゆかの頭の上には乗らない
    private bool throwingBall;//雪玉を放ってから消えるまではtrue

    private void Update()
    {   
        //後で状態分け
        //雪玉ボタンが押されているか Input_AttackButton
        //雪は生成されているか clonesnow
        //雪玉を生成してはなった後の状況か throwingBall 雪が消えたらtrueに戻す
        var cloneSnow = GameObject.FindWithTag("SnowBall");
        if (!cloneSnow)//雪玉が存在しているか.
        {//存在しない場合は雪玉を貯める処理を実行できる

            //Debug.Log("まだ生成されていません");
            if (Input_AttackButton)
            {
                AttackCharge();
            }
            else//アタックボタンが離されたら、貯めの過程の処理の変数を初期状態に戻す
            {
                //Spin_Up();
            }

            
        }
        else
        {
            //Debug.Log("生成されています");
            if (!throwingBall)//まだボールを投げていないなら攻撃ボタンの入力を受け付ける
            {
                if (Input_AttackButton)
                {
                    Hold_SnowBall();
                }
                else
                {
                    //攻撃を受けた時も以下の二行のコードを別の場所で実行する
                    throwingBall = true;
                    //Spin_Up();
                    Yukidama_Shot();
                }
            }

            if (throwingBall)
            {
                if (cloneSnow.transform.position.y <= -25 || cloneSnow.gameObject.GetComponent<Rigidbody2D>().velocity == Vector2.zero)
                {
                    cloneSnow.GetComponent<Yukidama_PrefabManager>().Destroy();
                    onSnowBall = false;
                }
            }
            if (onSnowBall)//足元に雪玉がきたら
            {

                //ここにレイの指揮を立てる
                this.transform.position = Ride_Fuyuka(this.transform.position, cloneSnow.transform.position);
               
            }
        }
        //Debug.Log("throwingBall = "+throwingBall);
        //Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Snow"), !throwingBall);
    }

    private Vector3 Ride_Fuyuka(Vector3 PlayerPos,Vector3 SnowPos)
    {
        PlayerPos.x = SnowPos.x;
        PlayerPos.y = SnowPos.y + 0.3f;
        rbody.velocity = new Vector2(0, 0);
        return PlayerPos;
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
                    
                    ball[0] = Instantiate(shadowsnow, new Vector3(this.transform.position.x , this.transform.position.y + 1.5f, this.transform.position.z),Quaternion.identity);
                    ball[1] = Instantiate(shadowsnow, new Vector3(this.transform.position.x , this.transform.position.y + 1.5f, this.transform.position.z), Quaternion.identity);
                    ball[2] = Instantiate(shadowsnow, new Vector3(this.transform.position.x , this.transform.position.y + 1.5f, this.transform.position.z), Quaternion.identity);
                    ball[3] = Instantiate(shadowsnow, new Vector3(this.transform.position.x , this.transform.position.y + 1.5f, this.transform.position.z), Quaternion.identity);
                    ball[4] = Instantiate(shadowsnow, new Vector3(this.transform.position.x , this.transform.position.y + 1.5f, this.transform.position.z), Quaternion.identity);
                    ball[5] = Instantiate(shadowsnow, new Vector3(this.transform.position.x , this.transform.position.y + 1.5f, this.transform.position.z), Quaternion.identity);
                    /*
                    ball[0].SetActive(true);
                    ball[1].SetActive(true);
                    ball[2].SetActive(true);
                    ball[3].SetActive(true);
                    ball[4].SetActive(true);
                    ball[5].SetActive(true);
                    */
                    YUKIDAMA_cancreate = false;

                    PlayAudio(ChargeSE);
                }

            }
            else if (charge < charge_MAX)
            {
                
                ball[0].transform.position = Vector3.Lerp(new Vector3(this.transform.position.x + 1, this.transform.position.y + 3f, this.transform.position.z), new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), (float)charge / charge_MAX);
                ball[0].transform.RotateAround(new Vector3(this.transform.position.x, this.transform.position.y + 1.5f, this.transform.position.z), Vector3.forward, -45f * Mathf.Log(charge_MAX, charge*6) * charge);

                ball[1].transform.position = Vector3.Lerp(new Vector3(this.transform.position.x + 2, this.transform.position.y + 1.5f, this.transform.position.z), new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), (float)charge / charge_MAX);
                ball[1].transform.RotateAround(new Vector3(this.transform.position.x, this.transform.position.y + 1.5f, this.transform.position.z), Vector3.forward, -45f * Mathf.Log(charge_MAX, charge * 6) * charge);

                ball[2].transform.position = Vector3.Lerp(new Vector3(this.transform.position.x + 1, this.transform.position.y - 0.5f, this.transform.position.z), new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), (float)charge / charge_MAX);
                ball[2].transform.RotateAround(new Vector3(this.transform.position.x, this.transform.position.y + 1.5f, this.transform.position.z), Vector3.forward, -45f * Mathf.Log(charge_MAX, charge * 6) * charge);

                ball[3].transform.position = Vector3.Lerp(new Vector3(this.transform.position.x - 1, this.transform.position.y - 0.5f, this.transform.position.z), new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), (float)charge / charge_MAX);
                ball[3].transform.RotateAround(new Vector3(this.transform.position.x, this.transform.position.y + 1.5f, this.transform.position.z), Vector3.forward, -45f * Mathf.Log(charge_MAX, charge * 6) * charge);

                ball[4].transform.position = Vector3.Lerp(new Vector3(this.transform.position.x - 2, this.transform.position.y + 1.5f, this.transform.position.z), new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), (float)charge / charge_MAX);
                ball[4].transform.RotateAround(new Vector3(this.transform.position.x, this.transform.position.y + 1.5f, this.transform.position.z), Vector3.forward, -45f * Mathf.Log(charge_MAX, charge *6) * charge);

                ball[5].transform.position = Vector3.Lerp(new Vector3(this.transform.position.x - 1, this.transform.position.y + 3f, this.transform.position.z), new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), (float)charge / charge_MAX);
                ball[5].transform.RotateAround(new Vector3(this.transform.position.x, this.transform.position.y + 1.5f, this.transform.position.z), Vector3.forward, -45f * Mathf.Log(charge_MAX, charge * 6) * charge);
            }

            else if (charge == charge_MAX)
            {
                //Debug.Log("雪玉生成");
                Destroy(Instantiate(Instantshadowsnow, new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z),Quaternion.identity),0.5f);
                for (int i = 0; i < ball.Length; i++)
                {
                    if (ball[i] != null)
                    {
                        ball[i].GetComponent<ParticleSystem>().loop = false;
                        Destroy(ball[i], 0.5f);
                    }
                }
                /*
                ball[0].SetActive(false);
                ball[1].SetActive(false);
                ball[2].SetActive(false);
                ball[3].SetActive(false);
                ball[4].SetActive(false);
                ball[5].SetActive(false);
                */
                Instantiate(SnowBall, new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z), Quaternion.identity);
            }
            else
            {
                if (charge >= 20) Hold_SnowBall();
            }
        }

    }

    public void Spin_Up()
    {
        for (int i = 0; i < ball.Length; i++)
        {
            if (ball[i] != null)
            {
                ball[i].GetComponent<ParticleSystem>().loop = false;
                Destroy(ball[i], 0.5f);
            }
        }

        if (charge < charge_MAX)//スピンジャンプ処理に移る場合
        {
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

        }
    }

    private void Yukidama_Shot()
    {
        var cloneSnow = GameObject.FindWithTag("SnowBall");
        if (right)//最後に入力された向きが右であれば
        {
            cloneSnow.GetComponent<Rigidbody2D>().velocity = (Vector3.right * 4 * 3f - Vector3.up * 6f);
        }
        if (left)//最後に入力された向きが左であれば
        {
            cloneSnow.GetComponent<Rigidbody2D>().velocity = (Vector3.left * 4 * 3f - Vector3.up * 6f);
        }
    }

    //ジャンプの機能を付けるまでいったんコメントアウト
    private void SpinJump()//空中にいるとき、チャージを途中で終了するとスピンに移行
    {
        var PlayerController = GetComponent<PlayerController>();

        if(PlayerController != null)
        {

            if (!PlayerController.FloorTakenGetter())
            {
                PlayOneShotAudio(SpinSE);
                StartCoroutine(SpinAnim());//ジャンプ中、スピンのアニメーション＆スピン中は雑魚敵なら倒せるようにする   
            }
            if (!PlayerController.FloorTakenGetter())//地面から浮いていた場合に発生
            {
                if (spin_counter <= 2 && spin_counter >= 0 && rbody.velocity.y < 0)//降下開始から一度もスピンしていない場合
                {
                    rbody.velocity = new Vector2(rbody.velocity.x, 0);
                    
                    rbody.AddForce(Vector3.up * 250 * spin_counter);
                    spin_counter++;
                }
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
        var cloneSnow = GameObject.FindWithTag("SnowBall"); 
        if(cloneSnow != null)
        {
            cloneSnow.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
            cloneSnow.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 1.4f, this.transform.position.z);
        }
    }

    //ここからはセッターゲッター類
    
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
        var cloneSnow = GameObject.FindWithTag("SnowBall");
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

    public void OnSnowballSetter()
    {
        onSnowBall = false;
    }
    public bool SpinAttackGetter()
    {
        return SpinAttack;
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

    private void OnTriggerEnter2D(Collider2D col)//ジャンプ可能かどうかの判定
    {
        
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        if (LayerName == "floor") 
        {
            //Debug.Log("着地判定に反応しています");
            spin_counter = 0;
        }



        if (LayerName == "Snow")
        {
            //Debug.Log("雪玉に反応しています");
            if(col.gameObject.tag == "SnowRide")
            {
                spin_counter = 0;
                if (throwingBall)
                {
                    onSnowBall = true;
                }
            }
        }

    }

    private void OnTriggerExit2D(Collider2D col)//ジャンプ可能な状態を解除する判定
    {
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        //雪玉の乗り解除判定はジャンプ入力、移動入力、雪玉消滅時に行う
        //if (LayerName == "floor" || LayerName == "SnowBallOnride") 
    }
}
