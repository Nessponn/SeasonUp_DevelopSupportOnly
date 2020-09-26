using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerStatusController PSC;
    private PlayerYukidamaManager PYM;
    private Player_Audio PA;
    private Rigidbody2D rbody;

    //プレイヤーの操作にかかわるトリガー部分。逆にこちらを付け消しする

    private bool left = false;
    private bool right = false;//移動に使う。こいつだけはどうしてもイベントトリガーとかで付けるのはだめだった
    private bool Jump = false;//ボタンが押されている間は、連続でジャンプできるようにする

    void Start()
    {
        PSC = GetComponent<PlayerStatusController>();
        PYM = GetComponent<PlayerYukidamaManager>();
        //PA = GetComponent<Player_Audio>();
        rbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //ここにキーボード入力についての処理を書く
        if (Input.GetKey(KeyCode.LeftArrow))//移動
        {
            PSC.movedirection_Setter(2);
            PYM.movedirection_Setter(2);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            PSC.movedirection_Setter(1);
            PYM.movedirection_Setter(1);
        }

        if(Input.GetKeyUp(KeyCode.LeftArrow))
        {
            PSC.movedirection_Setter(0);
            PYM.movedirection_Setter(0);
            if (rbody.velocity.x < -5f) rbody.velocity = new Vector2(-5, rbody.velocity.y);
        }else if(Input.GetKeyUp(KeyCode.RightArrow))
        {
            PSC.movedirection_Setter(0);
            PYM.movedirection_Setter(0);
            if (rbody.velocity.x > 5f) rbody.velocity = new Vector2(5, rbody.velocity.y);
        }

        if (Input.GetKeyDown(KeyCode.E)|| Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.X))
        {
            Fuyuka_Attack_True();
        }
        if (Input.GetKeyUp(KeyCode.E)|| Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.X))
        {
            Fuyuka_Attack_False();
        }
        if (Input.GetKey(KeyCode.Space) || Jump)
        {
            Fuyuka_Jump();
            PYM.YukidamaJump(false);
        }
        else
        {
            PYM.YukidamaJump(true);
        }
        if (Input.GetKeyUp(KeyCode.Space) && rbody.velocity.y > 3)
        {
            rbody.velocity = new Vector2(rbody.velocity.x, 3);
        }
    }

    public void Fuyuka_Left()//左に移動する
    {
        left = true;
        right = false;
        PSC.movedirection_Setter(2);
        PYM.movedirection_Setter(2);
    }
    public void Fuyuka_Right()//右に移動する
    {
        left = false;
        right = true;
        PSC.movedirection_Setter(1);
        PYM.movedirection_Setter(1);
    }
    public void Fuyuka_Stop()//止まる
    {
        left = false;
        right = false;
        PSC.movedirection_Setter(0);
        PYM.movedirection_Setter(0);
        if (rbody.velocity.x < -5f) rbody.velocity = new Vector2(-5, rbody.velocity.y);
        if (rbody.velocity.x > 5f) rbody.velocity = new Vector2(5, rbody.velocity.y);
    }
    private void Fuyuka_Jump()
    {
        if (PSC.JumpGetter() || PYM.OnSnowballGetter())
        {
            //PA.JumpAudio();
            PSC.Jumping();
        }
    }

    public void FuyukaJump_Down()
    {
        Jump = true;
    }
    public void FuyukaJump_Up()
    {
        Jump = false;
        PYM.YukidamaJump(true);
        if(rbody.velocity.y > 3) rbody.velocity = new Vector2(rbody.velocity.x, 3);
    }

    public void Fuyuka_Attack_True()//ここからYukidama生成にアクセス
    {
        PYM.ChargeMode_Setter(true);
    }
    public void Fuyuka_Attack_False()
    {
        PYM.ChargeMode_Setter(false);
        PYM.Spin_Up();
    }
}
