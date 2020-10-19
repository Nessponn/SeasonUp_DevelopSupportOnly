using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour, PlayerController_InterFace
{
    private Rigidbody2D rbody;
    //プレイヤーの操作にかかわるトリガー部分。逆にこちらを付け消しする

    private bool JumpPressed = false;//ボタンが押されているかどうか。離陸している状態とはまた別。ボタンが押されている間は、連続でジャンプできるようにする。
    private bool FloorTaken = false;//接地しているかどうか。

    public enum Move_Dir
    {
        Left,Right,Stop
    }
    private Move_Dir movedir = Move_Dir.Stop;

    private Animator anim;

    [SerializeField]private float PlayerSpeed = 5;

    public AudioClip StepSE;//足音
    public AudioClip JumpSE;//ジャンプ音
    void Start()
    {
        rbody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        Debug.DrawRay(new Vector2(this.transform.position.x, this.transform.position .y + 0.05f), new Vector2(0,-0.15f), Color.red);

        //ここにキーボード入力についての処理を書く
        if (Input.GetKey(KeyCode.LeftArrow))//移動
        {
            Fuyuka_Left();
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            Fuyuka_Right();
        }

        if(Input.GetKeyUp(KeyCode.LeftArrow))
        {
            Fuyuka_Stop();
        }
        else if(Input.GetKeyUp(KeyCode.RightArrow))
        {
            Fuyuka_Stop();
        }

        if (Input.GetKeyDown(KeyCode.E)|| Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.X))
        {
            Fuyuka_Attack_True();
        }
        if (Input.GetKeyUp(KeyCode.E)|| Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.X))
        {
            Fuyuka_Attack_False();
        }

        //ジャンプ
        if (Input.GetKey(KeyCode.Space))
        {
            FuyukaJumpButton_Down();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            FuyukaJumpButton_Up();
        }
        //カメラの位置の調節
       

        //移動の実行処理
        switch (movedir)
        {
            case Move_Dir.Left:

                anim.SetFloat("Run", Mathf.Abs(-3));

                //空中と地上で加速度が違う
                if (FloorTaken) rbody.AddForce(new Vector2(-100, 0));
                else rbody.AddForce(new Vector2(-60, 0));

                if (rbody.velocity.x < -PlayerSpeed) rbody.velocity = new Vector2(-PlayerSpeed, rbody.velocity.y);
                GetComponent<SpriteRenderer>().flipX = true;
                break;
            case Move_Dir.Right:
                anim.SetFloat("Run", Mathf.Abs(3));

                //空中と地上で加速度が違う
                if (FloorTaken) rbody.AddForce(new Vector2(100, 0));
                else rbody.AddForce(new Vector2(60, 0));

                if (rbody.velocity.x > PlayerSpeed) rbody.velocity = new Vector2(PlayerSpeed, rbody.velocity.y);
                GetComponent<SpriteRenderer>().flipX = false;
                break;
            case Move_Dir.Stop:

                anim.SetFloat("Run", Mathf.Abs(0));
                break;
        }//移動に関するプログラム

        if (rbody.velocity.y >= 0f && !FloorTaken)//上昇中の処理
        {
            anim.SetBool("Jumping", true);
        }
        if (rbody.velocity.y <= -1f && !FloorTaken)//ジャンプの最大地点に到達したとき
        {
            anim.SetBool("Jumping", false);
        }

        //後で雪玉に乗る処理も追加するので
        //anim.SetBool("Standing", FloorTaken || PYM.OnSnowballGetter());
        anim.SetBool("Standing", FloorTaken);
        if (rbody.velocity.y <= -12)
        {
            rbody.velocity = new Vector2(rbody.velocity.x, -12);
        }
    }

    public void Fuyuka_Left()//左に移動する「状態にする」
    {
        movedir = Move_Dir.Left;

        var PYM = GetComponent<PlayerYukidamaManager>();
        if (PYM != null)
        {
            PYM.ChengeVector_Left();
            PYM.OnSnowballSetter();
        }
    }
    public void Fuyuka_Right()//右に移動する「状態にする」
    {
        movedir = Move_Dir.Right;

        var PYM = GetComponent<PlayerYukidamaManager>();
        if (PYM != null) 
        {
            PYM.ChengeVector_Right();
            PYM.OnSnowballSetter();
        }
        
    }
    public void Fuyuka_Stop()//止まっている「状態にする」
    {
        movedir = Move_Dir.Stop;
    }
    private void Fuyuka_Jump()//実際にジャンプする処理はここ。
    {
        //接地しているかどうか
        //もしくは雪玉に乗っかっているかどうか
        if (FloorTaken)
        {
            //ジャンプボタンが押されたかどうか
            if (JumpPressed)
            {
                PlayOneShotAudio(JumpSE);
                rbody.velocity = new Vector2(rbody.velocity.x, 12);
            }
        }
    }

    public void FuyukaJumpButton_Down()
    {
        Debug.Log("ジャンプ！");
        JumpPressed = true;
        Fuyuka_Jump();

        var PYM = GetComponent<PlayerYukidamaManager>();
        if (PYM != null) PYM.OnSnowballSetter();
    }
    public void FuyukaJumpButton_Up()
    {
        JumpPressed = false;
        
        if(rbody.velocity.y > 3) rbody.velocity = new Vector2(rbody.velocity.x, 3);
    }

    public void Fuyuka_Attack_True()//ここからYukidama生成にアクセス
    {
        var PYM = GetComponent<PlayerYukidamaManager>();
        if (PYM != null) PYM.ChargeMode_Setter(true);
    }

    //ダメージを食らった時にもこのメソッドは発動する
    public void Fuyuka_Attack_False()
    {
        var PYM = GetComponent<PlayerYukidamaManager>();
        if (PYM != null)
        {
            PYM.ChargeMode_Setter(false);
            PYM.Spin_Up();
        }
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

    /*
    private void OnCollisionEnter2D(Collision2D col)
    {
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        if (col.GetType() == typeof(TilemapCollider2D))
        {

            if ((LayerName == "floor" || LayerName == "Snow")) FloorTaken = true;
        }

    }

    private void OnCollisionStay2D(Collision2D col)
    {
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        if (col.GetType() == typeof(TilemapCollider2D))
        {

            if ((LayerName == "floor" || LayerName == "Snow")) FloorTaken = true;
        }
    }


    private void OnCollisionExit2D(Collision2D col)
    {
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        if (col.GetType() == typeof(BoxCollider2D))
        {
            var PYM = GetComponent<PlayerYukidamaManager>();
            if ((LayerName == "floor" || LayerName == "Snow") && !PYM.OnSnowballGetter()) FloorTaken = true;
        }
    }
    */
    
    private void OnTriggerEnter2D(Collider2D col)//ジャンプ可能かどうかの判定
    {
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);

        //床コライダーにはBoxColliderとTilemapColliderがある
        //床コライダーのうち、タイルマップコライダーに触れたら着地判定にする

        if ((LayerName == "floor" || LayerName == "Snow")) FloorTaken = true;
        
    }

    

    private void OnTriggerStay2D(Collider2D col)
    {
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);

        if ((LayerName == "floor" || LayerName == "Snow")) FloorTaken = true;
        
    }

    private void OnTriggerExit2D(Collider2D col)//ジャンプ可能な状態を解除する判定
    {
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        var PYM = GetComponent<PlayerYukidamaManager>();
        if ((LayerName == "floor" || LayerName == "Snow")&& !PYM.OnSnowballGetter()) FloorTaken = false;
    }

    
    public bool FloorTakenGetter()
    {
        return FloorTaken;
    }

    public void FloorTakenSetter(bool setter)
    {
        FloorTaken = setter;
    }

    public void stepAudio()
    {
        PlayOneShotAudio(StepSE);
    }
}
