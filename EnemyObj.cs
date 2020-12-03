using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class EnemyObj : MonoBehaviour,DamagableObject,AI_Logic_Enemy
{
    public EnemyData Enemydata;
    //あとでEnemyObjectっていうクラス名に変えようかな…

    //このスクリプトは、ジャンプなどの行動を行うとともに

    enum _MoveDir
    {
        Left,Right,Wait
    }
    private _MoveDir movedirection = _MoveDir.Wait;

    private Rigidbody2D rbody;

    private bool FloorTaken = false;//接地しているかどうか。
    private bool iceFloorTaken = false;//氷の床に接地しているかどうか。

    private bool _DEATH;//やられたかどうか

    //ステータス
    private int HP;


    //このオブジェクトの設定
    private bool OutRangeDelete;//画面外に出たら消えるようになる


    //private GameObject Player;
    private void Start()
    {
        //ゲームオブジェクトを定義し、敵の情報を格納した後そのオブジェクトを子オブジェクトに登録する
        //サークルコライダーをつける
        //Enemydata = transform.parent.GetComponent<EnemySpawner>().GetEnemyData();

        //データの定義
        this.transform.localScale = new Vector3(Enemydata.EnemySpriteSetting.Scale, Enemydata.EnemySpriteSetting.Scale, 1);
        HP = Enemydata.EnemyObjectSetting.HP;
        rbody = GetComponent<Rigidbody2D>();
        
    }
    private void Update()
    {
        /*
        if (DebugMode)
        {
            enemy.GetComponent<Rigidbody2D>().mass = EnemyData.RigidBodySetting.Mass;
            enemy.GetComponent<Rigidbody2D>().gravityScale = EnemyData.RigidBodySetting.Gravity;
            if (EnemyData.ColliderSetting.BoxCol)
            {
                //敵の判定とは別に、着地を行う足の判定をつけるための判定
                enemy.GetComponentInChildren<BoxCollider2D>().size = new Vector2(EnemyData.ColliderSetting.Boxsize_x, EnemyData.ColliderSetting.Boxsize_y);
                enemy.GetComponentInChildren<BoxCollider2D>().offset = new Vector2(enemy.GetComponentInChildren<BoxCollider2D>().offset.x, EnemyData.ColliderSetting.Boxoffset_y);
            }
        }

        */
        if(this.transform.position.y <= -10)
        {
            Destroy(this.gameObject);
        }

        var Player = GameObject.FindWithTag("Player");

        if (!_DEATH)//やられているのに行動されちゃあまずいからさ…
        {
            //一定距離までプレイヤーが敵に近づいたら行動開始
            if (Mathf.Abs(this.gameObject.transform.localPosition.x - Player.transform.localPosition.x) < Enemydata.AgainstPlayerSetting.PlayerSearchRange)
            {
                _ConstraintsCheck();
                _move();
                    //移動中、プレイヤーが一定の高さ以上に位置していたらジャンプ行動を開始する
                    
                if (Player.transform.localPosition.y - this.transform.localPosition.y > 2f)
                {
                   
                    _jump();
                }
            }
            else
            {
                movedirection = _MoveDir.Wait;
            }
        }

        switch (movedirection)
        {
            case _MoveDir.Left:

                //anim.SetFloat("Run", Mathf.Abs(-3));

                //空中と地上(雪地)と地上（氷面）で加速度が違う
                if (iceFloorTaken && FloorTaken) rbody.AddForce(new Vector2(-Enemydata.EnemyActionSetting.Speed_OnIce, 0));
                else if (FloorTaken) rbody.AddForce(new Vector2(-Enemydata.EnemyActionSetting.Speed, 0));
                else rbody.AddForce(new Vector2(-Enemydata.EnemyActionSetting.Speed_InAir, 0));

                //または、サーチ範囲内にいるにもかかわらず、何らかの原因で速度が激減した場合もジャンプ行動を行う
                if (rbody.velocity.x >= 0) _jump();

                if (rbody.velocity.x < -Enemydata.EnemyActionSetting.MaxSpeed) rbody.velocity = new Vector2(-Enemydata.EnemyActionSetting.MaxSpeed, rbody.velocity.y);
                GetComponent<SpriteRenderer>().flipX = false;
                break;
            case _MoveDir.Right:
                //anim.SetFloat("Run", Mathf.Abs(3));

                //空中と地上(雪地)と地上（氷面）で加速度が違う
                if (iceFloorTaken && FloorTaken) rbody.AddForce(new Vector2(Enemydata.EnemyActionSetting.Speed_OnIce, 0));
                else if (FloorTaken) rbody.AddForce(new Vector2(Enemydata.EnemyActionSetting.Speed, 0));
                else rbody.AddForce(new Vector2(Enemydata.EnemyActionSetting.Speed_InAir, 0));

                if (rbody.velocity.x <= 0) _jump();

                if (rbody.velocity.x > Enemydata.EnemyActionSetting.MaxSpeed) rbody.velocity = new Vector2(Enemydata.EnemyActionSetting.MaxSpeed, rbody.velocity.y);
                GetComponent<SpriteRenderer>().flipX = true;
                break;
            case _MoveDir.Wait:

                //anim.SetFloat("Run", Mathf.Abs(0));
                break;
        }

        if (_Damage)
        {
            gameObject.GetComponent<SpriteRenderer>().color =
                new Color(1f, 1f, 1f, Mathf.Abs(Mathf.Sin(Time.time * 10)));//無敵が発動すると点滅を開始);

        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().color =
                new Color(1f, 1f, 1f, 1f);//無敵が発動すると点滅を開始
        }
    }

    public void _move()
    {
        var Player = GameObject.FindWithTag("Player");
        //Debug.Log("this.transform.position.x  - Player.transform.localPosition.x = " + (this.transform.position.x - Player.transform.localPosition.x));
       // Debug.Log("Player.transform.localPosition.x = " + Player.transform.localPosition.x);
        if (this.transform.position.x - Player.transform.localPosition.x > 0)//右方向にプレイヤーがいた場合
        {
            Debug.Log("左にいます！");
            movedirection = _MoveDir.Left;
        }
        else
        {
            Debug.Log("右にいます！");
            movedirection = _MoveDir.Right;
        }
    }

    public void _jump()
    {
        //そもそも接地判定があるのか
        //無ければそもそもジャンプする必要はない
        if (Enemydata.ColliderSetting.BoxCol)
        {

            if (FloorTaken || iceFloorTaken) rbody.velocity = new Vector2(rbody.velocity.x, Enemydata.EnemyActionSetting.JumpPower);

        }
    }
    //ダメージ処理
    
    public void _TakeDamage() 
    {
        Debug.Log("敵がやられました！");
        rbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        GetComponent<CircleCollider2D>().enabled = false;
        gameObject.GetComponentInChildren<BoxCollider2D>().enabled = false;
        gameObject.GetComponentInChildren<SpriteRenderer>().flipY = true;
        gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(gameObject.GetComponent<Rigidbody2D>().velocity.x, 9);

        

        _DEATH = true;
    }//プレイヤーまたは敵にあたったときのダメージ

    public void _YukidamaDamage()
    {

    }//雪玉にあたったときの処理

    public void _SpinDamage() 
    {
    
    }//プレイヤーのスピンにあたったとき

    public void _WaterDEATH()
    {
    
    }//水没した時


    private void _ConstraintsCheck()
    {
        if (Enemydata.RigidBodySetting.constant_z && !Enemydata.RigidBodySetting.PlayerRangeWithconstant_z)
        {
            if (Enemydata.RigidBodySetting.constant_x && Enemydata.RigidBodySetting.constant_y&& !Enemydata.RigidBodySetting.PlayerRangeWithconstant_x && !Enemydata.RigidBodySetting.PlayerRangeWithconstant_y)
            {
                //ｘ、ｙ、ｚの全てが押されている
                GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            }
            else if (Enemydata.RigidBodySetting.constant_y && !Enemydata.RigidBodySetting.PlayerRangeWithconstant_y)
            {
                GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            }//この時点で、zは押されているが、ｙは押されていないということがわかる
            else if (Enemydata.RigidBodySetting.constant_x && !Enemydata.RigidBodySetting.PlayerRangeWithconstant_x)
            {
                GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            }
            //この時点で、zのみが押されているということが分かった
            else
            {
                GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
        //この時点でzが押されているということはなくなった
        else if (Enemydata.RigidBodySetting.constant_y && !Enemydata.RigidBodySetting.PlayerRangeWithconstant_y)
        {
            if (Enemydata.RigidBodySetting.constant_x && !Enemydata.RigidBodySetting.PlayerRangeWithconstant_x)
            {
                GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
            }
            //この時点で、ｙのみが押されているということが分かった
            else
            {
                GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY;
            }
        }
        //この時点でｙが押されているということはなくなった
        else if (Enemydata.RigidBodySetting.constant_x && !Enemydata.RigidBodySetting.PlayerRangeWithconstant_x)
        {
            //ｘのみが押されている
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX;
        }
        //この時点で、zのみが押されているということが分かった
        else
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        }
    }


    private bool _Damage;
    private IEnumerator Damage()
    {
        _Damage = true;
        yield return new WaitForSeconds(0.5f);
        _Damage = false;
    }




    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "Player")
        {
            var obj = col.gameObject.GetComponent<PlayerStatusController>();
            //大抵falseが帰ってくるので、それで判別
            //また、スピンペネトレーション（スピン貫通）が働いていたらそれもダメージ通す
            if (obj.SpinStateGetter() && !Enemydata.AgainstPlayerSetting.PlayerSpinPenetration && !_Damage)
            {
                //プレイヤーがスピンしながら突っ込んできた場合、敵は死ぬ
                HP--;
                //Debug.Log("HP =" + HP);
                if(HP == 0)
                {
                    //HPが０になったらご退場
                    _TakeDamage();
                }
                else
                {
                    //HPがまだ１以上であれば、敵に無敵時間を付与し、行動を続行させる
                    StartCoroutine(Damage());
                }
            }
            else
            {
                //プレイヤーがそのまま突っ込んできたら、プレイヤーにダメージを与える
                obj._TakeDamage();
            }
        }
        if(col.gameObject.tag == "SHEILD")//ブリザードシールドの判定に触れた場合
        {
            HP--;
            if (HP == 0)
            {
                //HPが０になったらご退場
                _TakeDamage();
            }
            else
            {
                //HPがまだ１以上であれば、敵に無敵時間を付与し、行動を続行させる
                StartCoroutine(Damage());
            }
        }

        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        if ((LayerName == "floor" ))
        {
            FloorTaken = true;
        }
        if (col.gameObject.tag == "icefloor")
        {
            iceFloorTaken = true;
        }
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        //string layername = LayerMask.LayerToName(col.gameObject.layer);
        //if (layername == "floor") _Jump = true;
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        if ((LayerName == "floor"))
        {
            FloorTaken = true;
        }
        if (col.gameObject.tag == "icefloor")
        {
            iceFloorTaken = true;
        }
    }
    private void OnCollisionExit2D(Collision2D col)
    {
        //string layername = LayerMask.LayerToName(col.gameObject.layer);
        //if (layername == "floor") _Jump = false;
        string LayerName = LayerMask.LayerToName(col.gameObject.layer);
        if ((LayerName == "floor"))
        {
            FloorTaken = false;
        }
        if (col.gameObject.tag == "icefloor")
        {
            iceFloorTaken = false;
        }
    }
}
