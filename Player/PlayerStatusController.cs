using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerStatusController : MonoBehaviour
{
    //こちらのスクリプトは付け消しせず、PlayerControllerからの入力によって動作をする
    //PlayerControllerの参照
    private PlayerController PC;
    private PlayerYukidamaManager PYM;
    private Player_Audio PA;
    private SCORE_MANAGER SM;

    private enum Move_dir
    {
        Left,Right,Stop
    }
    private Move_dir movedirection = Move_dir.Stop;//ゲッターセッターで内容を変える

    private Animator anim;
    private Rigidbody2D rbody;
    private SpriteRenderer Sp;
    //整数型
    private int levelspeed = 0;

    //浮動小数点型
    private float speed = 0;
    private float t = 0;
    float level = 0;
    //真偽型
    private bool left = false;
    private bool right = true;
    private bool jump_ok = false;
    private bool Damage_Standby = true;//これがfalseの時、ふゆかはダメージを受けない
    private bool Damage_Transform = true;//境界線で移動している間(falseの時)は無敵になる。既存のDamage_Standbyを利用すると透明化が働いてしまう

    //GameObject型

    public LayerMask SnowBallLayer;//雪玉のこと

    public AudioClip StepSE;//足音
    public AudioClip JumpSE;
    void Start()
    {
        PC = GetComponent<PlayerController>();
        PYM = GetComponent<PlayerYukidamaManager>();
        PA = GetComponent<Player_Audio>();
        anim = GetComponent<Animator>();
        rbody = GetComponent<Rigidbody2D>();
        Sp = GetComponent<SpriteRenderer>();

    }

    void Update()//ここは主にキーボード入力を受け付ける
    {
        //やっぱりキーボード入力に関してはこんがらがるから
        //別のスクリプトからの入力でオナシャス

        switch (movedirection)
        {
            case Move_dir.Left:
                anim.SetFloat("Run", Mathf.Abs(-3));
                /*
                t += Time.deltaTime * 10;
                speed = 7 - (3 - t);
                if (speed > 7f)
                {
                    speed = 7;
                }

                left = true;
                right = false;
                canSlip = true;
                rbody.velocity = new Vector2(-speed, rbody.velocity.y);
                */
                left = true;
                right = false;
                if (jump_ok) rbody.AddForce(new Vector2(-100, 0));
                else rbody.AddForce(new Vector2(-60, 0));
                if (rbody.velocity.x < -7f) rbody.velocity = new Vector2(-7, rbody.velocity.y);
                //if (rbody.velocity.x < -4f && !jump_ok) rbody.velocity = new Vector2(-5, rbody.velocity.y);
                GetComponent<SpriteRenderer>().flipX = true;
                break;
            case Move_dir.Right:
                anim.SetFloat("Run", Mathf.Abs(3));
                /*
                t += Time.deltaTime * 10;
                speed = 7 - (3 - t);
                if (speed > 7f)
                {
                    speed = 7;
                }

                right = true;
                left = false;
                canSlip = true;
                rbody.velocity = new Vector2(speed, rbody.velocity.y);
                */
                right = true;
                left = false;
                if(jump_ok)rbody.AddForce(new Vector2(100, 0));
                else rbody.AddForce(new Vector2(60, 0));
                if (rbody.velocity.x > 7f) rbody.velocity = new Vector2(7, rbody.velocity.y);
                //if (rbody.velocity.x > 4f && !jump_ok) rbody.velocity = new Vector2(5, rbody.velocity.y);
                GetComponent<SpriteRenderer>().flipX = false;
                break;
            case Move_dir.Stop:

                anim.SetFloat("Run", Mathf.Abs(0));
                /*
                t = 0;
                speed = 0;
                if (canSlip)
                {
                    rbody.velocity = new Vector2(0, rbody.velocity.y);
                    canSlip = false;
                }
                else
                {
                    rbody.velocity = new Vector2(rbody.velocity.x, rbody.velocity.y);
                }
                */
                break;
        }//移動に関するプログラム

       

        //ここからアニメーション関連
        anim.SetBool("Standing", jump_ok ||PYM.OnSnowballGetter() );

        if (rbody.velocity.y >= 0f && !jump_ok)//上昇中の処理
        {
            anim.SetBool("Jumping", true);
        }
        if (rbody.velocity.y <= -1f && !jump_ok)//ジャンプの最大地点に到達したとき
        {
            anim.SetBool("Jumping", false);
            jump_ok = false;
        }

        if (!Damage_Standby)
        {
            level = Mathf.Abs(Mathf.Sin(Time.time * levelspeed));//無敵が発動すると点滅を開始
            Sp.color = new Color(1f, 1f, 1f, level);
        }

        if(rbody.velocity.y <= -12)
        {
            rbody.velocity = new Vector2(rbody.velocity.x,-12);
        }
    }

    public void Jumping()//入力を受け付けるとジャンプする
    {
        //if (PYM.OnSnowballGetter()||PYM.AlwaysSnowGetter()) PYM.YukidamaJump(false);
        rbody.velocity = new Vector2(rbody.velocity.x, 12);
        jump_ok = false;
        PlayOneShotAudio(JumpSE);
    }

    public void movedirection_Setter(int Setter)
    {
        //０…停止
        //１…右
        //２…左
        if(Setter == 0)
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

    private void OnCollisionEnter2D(Collision2D col)//即死トラップとアイテム取得の判定はここで行う
    {
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        if (LayerName == "DEATH")//針とかのスローアニメーションした方がいいやつ
        {
            anim.SetBool("Damage", true);
            StartCoroutine(DEATH_Fuyuka_Immedia());
        }
        if(LayerName == "Sea")//水ポチャ
        {
            DEATH_Fuyuka_Sea();
        }

    }

    private void OnTriggerEnter2D(Collider2D col)//敵に当たったかどうかの判定
    {
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);


        



        //スピンアタック中でもない時に敵に突っ込んだ場合
        if (LayerName == "Enemy_Collider(PlayercantTouch)" && Damage_Standby && !PYM.SpinAttackGetter() && Damage_Transform)
        {
            //プレイヤーの操作権を奪う
            StartCoroutine(PlayerDamage());//プレイヤーの操作時間を一定時間奪い、ダメージ中に雪玉を溜めていた場合、溜めを中断させるプログラム
            LifeDecrese();//ライフを一つ減らして
            /*
            if(TG.LM.LifeGetter() != 0)//喰らった後のライフが０でなければ
            {
                StartCoroutine(SpecialTime());//無敵時間を付与する
            }
            else//０だったら
            {
                PC.enabled = false; //操作権限を奪われて

                StartCoroutine(DEATH_Fuyuka());//死ぬ
            }
            */

            //プレイヤーをぶっ飛ばす
            if (right == true)
            {
                rbody.velocity = (Vector3.left * 4 + Vector3.up * 7);
            }
            if (left == true)
            {
                rbody.velocity = (Vector3.right * 4 + Vector3.up * 7);
            }

        }
        //スピンアタック中に敵に突っ込んだ場合
        if(LayerName == "Enemy_Collider(PlayercantTouch)" && PYM.SpinAttackGetter())
        {
            //Debug.Log("スピンアターック！");
        }

        if (LayerName == "Boss" && Damage_Standby && !Damage_Transform)//スピンアタックで突っ込んでもダメージを受けてしまう
        {

        }


        if (col.gameObject.tag == "Cookie")
        {
            //TG.LM.LifeHeal();
            PA.ItemGet_Audio();
            Destroy(col.gameObject);
        }
        if (col.gameObject.tag == "Key")
        {
            SM.KEY_Interface(true);
            PA.ItemGet_Audio();
            Destroy(col.gameObject);
        }
        if (col.gameObject.tag == "Jewel")
        {
            SM.Jewel.SetActive(true);
            PA.ItemGet_Audio();
            Destroy(col.gameObject);
        }
    }
    void OnTriggerStay2D(Collider2D col)//ジャンプ可能かどうかの判定
    {
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        if (LayerName == "floor" || LayerName == "SnowBallOnride") jump_ok = true;
        //if (LayerName == "SnowBallOnride") PYM.YukidamaJump(true);
    }

    void OnTriggerExit2D(Collider2D col)//ジャンプ可能な状態を解除する判定
    {
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        
        if (LayerName == "floor" || LayerName == "SnowBallOnride") jump_ok = false;
        //if (LayerName == "SnowBallOnride") PYM.YukidamaJump(false);
    }
    public void JumpSetter(bool Setter)
    {
        jump_ok = Setter;
    }
    public bool JumpGetter()
    {
        return jump_ok;
    }
    public bool LeftGetter()
    {
        return left;
    }
    public bool RightGetter()
    {
        return right;
    }

    public void Damage_StandbySetter(bool Setter)
    {
        Damage_Transform = Setter;
    }


    private IEnumerator PlayerDamage()//プレイヤーの操作時間を奪う＆雪玉精製中断
    {
        PC.enabled = false;
        PYM.PlayerDamageProcess(false);
        anim.SetBool("Damage", true);

        yield return new WaitForSeconds(0.5f);

        anim.SetBool("Damage", false);
        PC.enabled = true;
    }
    public IEnumerator PlayerControll(float SetTime)//こちらは画面移行などで一時的に操作だけを奪いたいときに
    {
        PC.enabled = false;
        yield return new WaitForSeconds(SetTime);
        PC.enabled = true;
    }
    private void LifeDecrese()//ライフを一つ減らす
    {
        PA.Damage_Audio();
        //TG.LM.LifeDecreser();
    }

    private IEnumerator SpecialTime()//無敵時間を一定時間付与
    {
        Damage_Standby = false;
        levelspeed = 10; //無敵が発動すると点滅を開始
        yield return new WaitForSeconds(1f);
        levelspeed = 20;//無敵が切れる直後は点滅が早くなる
        yield return new WaitForSeconds(1f);
        Damage_Standby = true;
        Sp.color = new Color(1f, 1f, 1f, 1f);
    }

    

    //敵との接触で死んだ場合、少し画面を止めてから演出を開始する
    public IEnumerator DEATH_Fuyuka()
    {
        Time.timeScale = 0.1f;
        yield return new WaitForSeconds(0.05f);
        Time.timeScale = 1;
        //Instantiate(TG.PlayerDEATH_Particle, this.transform.position,Quaternion.identity);
        //StartCoroutine(TG.Player_Respawn());
        //AS.Stop();
        //AS.PlayOneShot(TG.DEATH_SE);
        //TG.DEATH_Setter(true);
        this.gameObject.SetActive(false);
        //PC.enabled = false;
        //Damage_Standby = false;
        //Sp.color = new Color(1f, 1f, 1f, 0f);
        //yield return new WaitForSeconds(3f);
        //string scenename = SceneManager.GetActiveScene().name;
        //SceneManager.LoadScene(scenename);
    }

    public IEnumerator DEATH_Fuyuka_Immedia()//トゲなどでの即死
    {
        //TG.LM.LifeDead();
        PA.Damage_Audio();
        Time.timeScale = 0.1f;
        yield return new WaitForSeconds(0.05f);
        Time.timeScale = 1;
//Instantiate(TG.PlayerDEATH_Particle, this.transform.position, Quaternion.identity);
        //StartCoroutine(TG.Player_Respawn());
        //AS.Stop();
        //AS.PlayOneShot(TG.DEATH_SE);
        //TG.DEATH_Setter(true);
        this.gameObject.SetActive(false);
    }

    //水にポチャるなどの即死トラップで死んだ場合、すぐに演出を開始する
    public void DEATH_Fuyuka_Sea()
    {
        //TG.LM.LifeDead();
        //Destroy(Instantiate(TG.Water_Object, this.transform.position, Quaternion.identity),1.0f);
        Camera.main.GetComponent<AudioSource>().Stop();
        //Camera.main.GetComponent<AudioSource>().PlayOneShot(TG.AM.WaterPotya);
        //StartCoroutine(TG.Player_Respawn());
        //TG.DEATH_Setter(true);
        this.gameObject.SetActive(false);
    }

    public void StepAudioClip()//Unityのアニメーションイベントで鳴らす
    {
        PlayOneShotAudio(StepSE);
    }

    //このスクリプトが持つAudioClipを鳴らす
    private void PlayAudio(AudioClip AC)
    {
        var PAM = GetComponent<Player_AudioManager>();
        if (PAM != null) PAM.AudioPlayer(AC);
    }

    private void PlayOneShotAudio(AudioClip AC)
    {
        var PAM = GetComponent<Player_AudioManager>();
        if (AC != null)
        {
            PAM.AudioPlayerOnplayOneShot(AC);
        }
    }
}
