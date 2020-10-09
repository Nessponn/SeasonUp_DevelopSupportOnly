using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Yukidaruman : MonoBehaviour
{
    //敵からプレイヤーへの当たり判定は、別のスクリプトで行う
    //ジャンプの処理はここで行う
    //敵がやられた場合、このスクリプトと敵のコライダーの機能を停止させる
    //ここは接地判定と敵の仕組みを書く

    //全部の敵で共通
    private GameObject Player;
    private Animator anim;
    private Rigidbody2D rbody;

    //接地系の敵に共通

    private bool jump_ok = false;
    private bool Run = false;
    private bool canSlip = false;

    private bool DamageOnce = true;//死んだときの吹っ飛びは一回だけにする

    private float t = 0;
    private float speed = 0;

    private enum Move_dir
    {
        Left, Right, Stop
    }
    private Move_dir movedirection = Move_dir.Stop;//ゲッターセッターで内容を変える

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        anim = GetComponentInParent<Animator>();
        rbody = GetComponentInParent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (movedirection)
        {
            case Move_dir.Left:
                t += Time.deltaTime * 2;
                speed = 4 - (4 - t);
                if (speed > 4f)
                {
                    speed = 4;
                }

                //left = true;
                //right = false;
                canSlip = true;
                rbody.velocity = new Vector2(-speed, rbody.velocity.y);
                GetComponentInParent<SpriteRenderer>().flipX = false;
                anim.SetBool("Standing", true);
                break;
            case Move_dir.Right:
                t += Time.deltaTime * 2;
                speed = 4- (4 - t);
                if (speed > 4f)
                {
                    speed = 4;
                }

                //right = true;
                //left = false;
                canSlip = true;
                rbody.velocity = new Vector2(speed, rbody.velocity.y);
                GetComponentInParent<SpriteRenderer>().flipX = true;
                anim.SetBool("Standing", true);
                break;
            case Move_dir.Stop:
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
                anim.SetBool("Standing", false);
                break;
        }



        if (jump_ok)//ジャンプと歩くモーションのアニメーション
        {
            //未ジャンプ
            
            anim.SetBool("Jumping", false);

            if (Player.transform.position.y - transform.position.y >= 1f)//ジャンプ可能であれば、プレイヤーが一定距離の間隔内にいる場合にジャンプをする
            {
                //Debug.Log("Enemy_Jump");
                if(DamageOnce) rbody.velocity = new Vector2(rbody.velocity.x, 8f);
            }
        }
        else
        {
            //ジャンプ中
            anim.SetBool("Jumping", true);
        }


        if (Run)//プレイヤーがこいつのトリガーコライダー内にいる場合、プレイヤーに向かって走る
        {
            anim.SetFloat("Running", 5);
            if (Player.transform.position.x - transform.position.x >= 0)
            {
                movedirection = Move_dir.Right;
            }
            if (Player.transform.position.x - transform.position.x < 0)
            {
                movedirection = Move_dir.Left;
            }
        }
        else
        {
            anim.SetFloat("Running",0);
            movedirection = Move_dir.Stop;
        }

    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        if (LayerName == "PlayerSearcher")
        {
            Run = true;
        }
        
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        if (LayerName == "PlayerSearcher")
        {
            Run = false;
            movedirection = Move_dir.Stop;
        }
        if (col.gameObject.tag == "YukidamaEraser")
        {
            Destroy(this.gameObject);
        }
    }
    /*
    void OnTriggerStay2D(Collider2D col)
    {
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        if (LayerName == "PlayerSearcher")
        {
            Debug.Log("プレイヤーは感知できています！");
            Run = true;
        }
        else
        {
            Debug.Log("止まってるよ");
            Run = false;
            movedirection = Move_dir.Stop;
        }
    }
    */
    
    void OnCollisionStay2D(Collision2D col)//ジャンプ可能かどうかの判定(精密性は求めていないので、Collisionで設置判定)
    {
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        if (LayerName == "floor(jumpOk)") jump_ok = true;
    }

    void OnCollisionExit2D(Collision2D col)//ジャンプ可能な状態を解除する判定
    {
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        if (LayerName == "floor(jumpOk)") jump_ok = false;
    }
    
    public void DamageOnce_Setter(bool Setter)
    {
        DamageOnce = Setter;
    }

    public void Run_Setter(bool Setter)
    {
        Run = Setter;
    }
}
