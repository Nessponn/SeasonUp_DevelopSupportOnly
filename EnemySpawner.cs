using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour,DamagableObject,AI_Logic_Enemy
{
    //あとでEnemyObjectっていうクラス名に変えようかな…

    enum _MoveDir
    {
        Left,Right,Wait
    }
    private _MoveDir movedirection = _MoveDir.Wait;

    private Rigidbody2D Rbody;
    [SerializeField] private EnemyData Enemydata;

    private bool _Jump;


    private void Start()
    {
        //
        Rbody = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        //プレイヤーのサーチ
        var Player = GameObject.FindWithTag("Player");
        //一定距離までプレイヤーが敵に近づいたら行動開始
        if (Mathf.Abs(this.transform.localPosition.x - Player.transform.localPosition.x) < Enemydata.AgainstPlayerSetting.PlayerSearchRange)
        {
            _move();

            //移動中、プレイヤーが一定の高さ以上に位置していたらジャンプ行動を開始する
            if(Player.transform.localPosition.y - this.transform.localPosition.y > 2)
            {
                _jump();
            }

        }
        else
        {
            movedirection = _MoveDir.Wait;
        }

        switch (movedirection)
        {
            case _MoveDir.Left:

                if (_Jump) Rbody.AddForce(new Vector2(-Enemydata.EnemyObjectSetting.Speed, 0));
                else Rbody.AddForce(new Vector2(-Enemydata.EnemyObjectSetting.Speed_InAir, 0));

                if (Rbody.velocity.x < -Enemydata.EnemyObjectSetting.MaxSpeed) Rbody.velocity = new Vector2(-Enemydata.EnemyObjectSetting.MaxSpeed, Rbody.velocity.y);
                GetComponent<SpriteRenderer>().flipX = true;



                break;
            case _MoveDir.Right:
                if (_Jump) Rbody.AddForce(new Vector2(Enemydata.EnemyObjectSetting.Speed, 0));
                else Rbody.AddForce(new Vector2(Enemydata.EnemyObjectSetting.Speed_InAir, 0));

                if (Rbody.velocity.x > Enemydata.EnemyObjectSetting.MaxSpeed) Rbody.velocity = new Vector2(Enemydata.EnemyObjectSetting.MaxSpeed, Rbody.velocity.y);
                break;
            case _MoveDir.Wait:
                break;
        }
    }

    public void _move()
    {
        var Player = GameObject.FindWithTag("Player");
        if(this.transform.localPosition.x - Player.transform.localPosition.x > 0)//右方向にプレイヤーがいた場合
        {
            movedirection = _MoveDir.Left;
        }
        else
        {
            movedirection = _MoveDir.Right;
        }
    }

    public void _jump()
    {
        if (_Jump) Rbody.velocity = new Vector2(Rbody.velocity.x,Enemydata.EnemyObjectSetting.JumpPower);
        
    }
    //ダメージ処理
    
    public void _TakeDamage() 
    {
        Debug.Log("敵がやられました！");
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

    private void OnCollisionStay2D(Collision2D col)
    {
        string layername = LayerMask.LayerToName(col.gameObject.layer);
        if (layername == "floor") _Jump = true;
    }
    private void OnCollisionExit2D(Collision2D col)
    {
        string layername = LayerMask.LayerToName(col.gameObject.layer);
        if (layername == "floor") _Jump = false;
    }
}
