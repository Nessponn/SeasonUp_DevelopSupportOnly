using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[ExecuteInEditMode]…実行したものが、エディターの再生を止めても残り続ける設定
public class EnemySpawner : MonoBehaviour
{
    //アクティブバランサーを考慮した設計にする（）

    //EnemyObjを発生させるためのもの。

    //スポナーにつけるもの
    //こいつ（EnemySpawner）、SpriteRenderer、エディター上にスプライトを表示させるための更新ボタン「スプライト更新」

    public EnemyData EnemyData;

    public GameObject EnemyBase;

    private bool Respawn = true;

    private GameObject enemy;

    public bool DebugMode;

    // Update is called once per frame
    void Update()
    {
        
        var Player = GameObject.FindWithTag("Player");
        //Debug.Log("this.gameObject.transform.localPosition.x - Player.transform.localPosition.x = " + (this.gameObject.transform.localPosition.x - Player.transform.localPosition.x));
        if (Mathf.Abs(this.gameObject.transform.localPosition.x - Camera.main.transform.localPosition.x) < 10)
        {//範囲内にプレイヤーがいるかどうか
            
            //敵が生成されていない、かつリスポーン設定がされていた場合
            if (!enemy && Respawn)
            {
                Respawn = false;
                EnemySpawn();
            }
            //生成後にRespawnの状態を上書きする（つまり一度はかならず生成されるようにする）
        }
        else
        {
            if (!enemy) Respawn = EnemyData.EnemyObjectSetting.Respawn;
        }

    }

    void EnemySpawn()//敵を生成する
    {
        //あーガバガバ…
        //子オブジェクトに子オブジェクトはつけらんないので、仕方がないので敵の型のプレハブ作ります
        //プレハブの敵ベースとなるオブジェクトに様々なデータ情報を送り込むという形にします

        enemy = Instantiate(EnemyBase, this.transform.position, Quaternion.identity);

        enemy.transform.localPosition = new Vector3(this.transform.position.x, this.transform.position.y, 0);
        enemy.GetComponent<CircleCollider2D>().isTrigger = true;
        //enemy.gameObject.layer = LayerMask.NameToLayer("Enemy");

        //敵を形成する判定として必要なものを渡す
        //enemy.GetComponent<CircleCollider2D>().size = new Vector2(1, enemy.transform.localScale.y);
        enemy.GetComponent<CircleCollider2D>().offset = new Vector2(0, EnemyData.ColliderSetting.Circleoffset_y);
        enemy.GetComponent<CircleCollider2D>().radius = EnemyData.ColliderSetting.radius;
        


        if (EnemyData.ColliderSetting.BoxCol)
        {//着地を有効にする
            
            //敵の判定とは別に、着地を行う足の判定をつけるための判定
            enemy.GetComponentInChildren<BoxCollider2D>().size = new Vector2(EnemyData.ColliderSetting.Boxsize_x, EnemyData.ColliderSetting.Boxsize_y);
            enemy.GetComponentInChildren<BoxCollider2D>().offset = new Vector2(EnemyData.ColliderSetting.Boxoffset_x, EnemyData.ColliderSetting.Boxoffset_y);
            enemy.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
        else
        {
            //着地を無効にし、床をすり抜けるようにする
            enemy.GetComponentInChildren<BoxCollider2D>().size = new Vector2(EnemyData.ColliderSetting.Boxsize_x, EnemyData.ColliderSetting.Boxsize_y);
            enemy.GetComponentInChildren<BoxCollider2D>().offset = new Vector2(EnemyData.ColliderSetting.Boxoffset_x, EnemyData.ColliderSetting.Boxoffset_y);
            enemy.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("EnemythroughFloor");
        }

        //リジッドボディーの設定
        enemy.GetComponent<Rigidbody2D>().mass = EnemyData.RigidBodySetting.Mass;
        enemy.GetComponent<Rigidbody2D>().gravityScale = EnemyData.RigidBodySetting.Gravity;

        if (EnemyData.RigidBodySetting.constant_z)
        {
            if (EnemyData.RigidBodySetting.constant_x && EnemyData.RigidBodySetting.constant_y)
            {
                //ｘ、ｙ、ｚの全てが押されている
                enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            }
            else if (EnemyData.RigidBodySetting.constant_y)
            {
                enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
            }//この時点で、zは押されているが、ｙは押されていないということがわかる
            else if (EnemyData.RigidBodySetting.constant_x)
            {
                enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            }
            //この時点で、zのみが押されているということが分かった
            else
            {
                enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
        //この時点でzが押されているということはなくなった
        else if (EnemyData.RigidBodySetting.constant_y)
        {
            if (EnemyData.RigidBodySetting.constant_x)
            {
                enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
            }
            //この時点で、ｙのみが押されているということが分かった
            else
            {
                enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY;
            }
        }
        //この時点でｙが押されているということはなくなった
        else if (EnemyData.RigidBodySetting.constant_x)
        {
            //ｘのみが押されている
            enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX;
        }
        //この時点で、zのみが押されているということが分かった
        else
        {
            enemy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        }

        //SpriteRendererのアタッチ
        enemy.GetComponent<SpriteRenderer>().sprite = EnemyData.EnemySpriteSetting.Image;
        enemy.gameObject.tag = "Enemy";
        //敵の行動パターンやデータを入れる.
        //データの送信は、メソッド開始時にStartメソッドで行われる
        enemy.AddComponent<EnemyObj>();
        enemy.GetComponent<EnemyObj>().Enemydata = EnemyData;
        //エディターで表示しているスポナーのスプライトは消す
        this.GetComponent<SpriteRenderer>().enabled = false;
    }


    public void SetSprite()//エディター上でもスプライトの状態を確認できるもの
    {
        if(EnemyData != null)
        {
            GetComponent<SpriteRenderer>().sprite = EnemyData.EnemySpriteSetting.Image;

            //スケールはスポナーのスケールと同じものにしても大丈夫
            //敵オブジェクトの方には、いいスケールデータの渡し方が今のところ思いつかないのでこのままで…
            //this.transform.localScale = new Vector3(EnemyData.EnemySpriteSetting.Scale, EnemyData.EnemySpriteSetting.Scale, 1);
        }
        else
        {
            Debug.LogError("そのスポナーには「EnemyData」がついてません！");
        }
    }

    public EnemyData GetEnemyData()
    {
        return EnemyData;
    }
}
[CustomEditor(typeof(EnemySpawner))]
public class SpriteSetter : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();//もともとのインスペクターの内容を表示

        //追加でボタンの表示を行う
        EnemySpawner spr = target as EnemySpawner;

        if (GUILayout.Button("エディターで表示するスプライトの更新"))
        {
            spr.SetSprite();
        }
        EditorUtility.SetDirty(target);
    }
}
