using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour,CameraChase
{
    private PlayerStatusController PSC;
    private PlayerYukidamaManager PYM;
    private Player_Audio PA;
    private Rigidbody2D rbody;

    private Vector3 Camera_posX;//カメラのポジション。基本はｘ軸を動かす

    private float L_limit;//左のスクロール限度
    private float R_limit;//右のスクロール限度

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

        //カメラの位置の調節
        Camera_Property = new Vector3(this.transform.position.x, Camera.main.transform.position.y, -10);

        Camera.main.transform.position = Camera_Property;
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

    public Vector3 Camera_Property
    {
        get { return new Vector3(Camera_posX.x, Camera_posX.y, -10); }

        set { Camera_posX = value; }
    }

    public void Camera_Limit_Setter(float X_Limit, float Y_Limit)
    {

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
