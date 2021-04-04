using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class TitleManager : SingletonMonoBehaviourFast<TitleManager>
{
    //本マップに挿入するマップの大きさは横幅　37+17*n　のサイズでヨロ～



    //ゲームオブジェクト（private組）
    public GameObject StageGrid;
    public GameObject[] StageMaps;

    public GameObject Image_Tilemap;

    public GameObject Outline_map;//アウトライン専用のマップ


    public GameObject MiddleGraund;

    public string GameSceneName;


    //変数
    private float t = 0;//時間。

    private int distance = 4;
    private int size_bottom = 25;
    //private float outline_total_distance = 0;//アウトラインマップの移動総合距離


    bool _isPausingIntro;
    public AudioSource _loopAudioSource;
    public AudioSource _introAudioSource;


    // Start is called before the first frame update
    void Start()
    {
       
    }

    public void AudioStart()
    {
        //イントロ部分の再生開始
        _introAudioSource.PlayScheduled(AudioSettings.dspTime);

        //イントロ終了後にループ部分の再生を開始
        _loopAudioSource.PlayScheduled(AudioSettings.dspTime + 0.05f + ((float)_introAudioSource.clip.samples / (float)_introAudioSource.clip.frequency));
    }

    /// <summary>
    /// BGMを一時停止
    /// </summary>
    public void Pause()
    {
        //イントロ中に一時停止したかどうか
        _isPausingIntro = _introAudioSource.isPlaying;

        _loopAudioSource.Pause();
        _introAudioSource.Pause();
    }

    /// <summary>
    /// BGMを一時停止を解除
    /// </summary>
    public void UnPause()
    {
        _introAudioSource.UnPause();

        if (_isPausingIntro)
        {
            //イントロ中に一時停止した場合はループ部分の遅延再生を設定し直す
            _loopAudioSource.Stop();
            _loopAudioSource.PlayScheduled(AudioSettings.dspTime - _introAudioSource.time + ((float)_introAudioSource.clip.samples / (float)_introAudioSource.clip.frequency));
        }
        else
        {
            _loopAudioSource.UnPause();
        }
    }


    // Update is called once per frame
    void Update()
    {
        MiddleGraund.transform.Translate(new Vector2(-0.005f, 0));
        if(MiddleGraund.transform.position.x < -18f)
        {
            MiddleGraund.transform.position = new Vector3(0,0,30);
        }

        //タイルマップの処理
        {
            t -= Time.deltaTime * distance;
            //outline_total_distance -= Time.deltaTime * distance;

            if (t <= -distance)
            {
                //Debug.Log("value = " + (Camera.main.transform.localPosition.x - Player.transform.localPosition.x));
                var bound = StageGrid.GetComponent<Tilemap>().cellBounds;
                //Debug.Log("bound.x = "+ bound.x);
                //Debug.Log("bound.max.x = " + bound.max.x);

                int num = UnityEngine.Random.Range(0, StageMaps.Length);
                if (bound.max.x > size_bottom)
                {
                    PassTile();
                }
                else
                {
                    //outline_total_distance = 0;
                    AddPass_Tile(num);
                    //Create_Outline_Maps();
                }

                //total_distance += distance;

                //distance = 1;

                t = 0;
            }

            StageGrid.transform.position = new Vector3(t, StageGrid.transform.position.y, StageGrid.transform.position.z);
            Image_Tilemap.transform.position = new Vector3(t, StageGrid.transform.position.y, StageGrid.transform.position.z);

            //Outline_map.transform.position = new Vector3(outline_total_distance, StageGrid.transform.position.y, StageGrid.transform.position.z);

            /*
            for(int i = 0;i< StageGrid.Length; i++)
            {
                if (StageGrid[i].GetComponent<Tilemap>())
                {
                    StageGrid[i].transform.position = new Vector3(t, StageGrid[i].transform.position.y, StageGrid[i].transform.position.z);
                }
                else
                {
                    StageGrid[i].transform.position -= new Vector3((float)distance/40 ,0, 0);
                }
            }
            */
        }

        //タイトル画面の入力

    }

    public void ToGameScene()
    {
        SceneManager.LoadScene(GameSceneName);
    }

    //もともとのタイルを保存するクラス
    private class TileInfo
    {
        public readonly Vector3Int m_position;
        public readonly Vector3 m_rotation;
        public readonly TileBase m_tile;

        public TileInfo(Vector3Int position,int addposition_x, Vector3 rotation, TileBase tile,int distance)
        {
            position.x -= distance;
            position.x += addposition_x;
            m_position = position;
            m_rotation = rotation;
            m_tile = tile;
        }
    }

    /*
    //追加分のタイルを保存するクラス
    private class TileInfo_add
    {
        public readonly Vector3Int m_position;
        public readonly Vector3 m_rotation;
        public readonly TileBase m_tile;

        public TileInfo_add(Vector3Int position, Vector3 rotation, TileBase tile, int distance)
        {
            position.x -= distance;
            m_position = position;
            m_rotation = rotation;
            m_tile = tile;
        }
    }
    */
    public void PassTile()
    {
        //タイルの情報をずらす
        //0から-1担った瞬間、座標を0基準でタイル情報を左にずらす


        //タイルから何列目にいるか（y軸の値）の情報を引き出す
        int num = 0;
        //Vector3Int vec;
        var tilemap = StageGrid.GetComponent<Tilemap>();
        var bound = StageGrid.GetComponent<Tilemap>().cellBounds;

        int gridnum = bound.max.x;

        //bound.max = new Vector3Int(bound.max.x - 1, bound.max.y, bound.max.z);

        var list = new List<TileInfo>();

        
        for (int y = bound.max.y - 1; y >= bound.min.y; --y)//左上から右下にかけてタイルを代入していく
        {
            for (int x = bound.min.x; x < gridnum; ++x)
            {
                //Debug.Log("x = " + x);
                //タイルの情報を下記の１行のコードに格納する
                //タイルの座標、メソッドだと思っていた部分で取れるのビビったんやけど…
                //var tile = StageGrid.GetComponent<Tilemap>().GetTile<Tile>(vec = new Vector3Int(x, y, 0));

                //vec.x -= 1;

                var tile = StageGrid.GetComponent<Tilemap>().GetTile<Tile>(new Vector3Int(x, y, 0));

                var position = new Vector3Int(x, y, 0);
                Vector3 rotation = tilemap.GetTransformMatrix(position).rotation.eulerAngles;//回転を取る

                //タイルのxの位置がタイルの最小の値より大きい（左側に位置している）なら
                if (position.x >= bound.min.x)
                {
                    //
                    var info = new TileInfo(position, 0,rotation, tile, distance);
                    list.Add(info);

                    //本マップの幅が17以下になったらマップを補充する
                }


                if (list.Count <= 0) return;

                num++;
            }

            foreach (var data in list)
            {
                var position = data.m_position;
                var rotation = data.m_rotation;

                if (position.x >= bound.min.x)
                {
                    tilemap.SetTile(position, data.m_tile);
                    Matrix4x4 matrix = Matrix4x4.TRS(Vector3Int.zero, Quaternion.Euler(rotation), Vector3.one);
                    tilemap.SetTransformMatrix(position, matrix);
                }
                else
                {
                    //ここで、本マップ以降のマスはすべて削除している
                    tilemap.SetTile(position, null);
                }


                position.x += 1;
                tilemap.SetTile(position, null);
            }
        }

        //
        
        if (distance > 1)
        {
            tilemap.size = new Vector3Int(tilemap.size.x - distance, 10, tilemap.size.z);
            tilemap.ResizeBounds();
        }
        StageGrid.GetComponent<Tilemap>().size = tilemap.size;
        StageGrid.GetComponent<Tilemap>().CompressBounds();

        Create_Outline_Maps(list);
    }

    void AddPass_Tile(int num)
    {

        var tilemap = StageGrid.GetComponent<Tilemap>();
        var addtilemap = StageMaps[num].GetComponent<Tilemap>();

        var bound = StageGrid.GetComponent<Tilemap>().cellBounds;
        var add_bound = StageMaps[num].GetComponent<Tilemap>().cellBounds;

        //int gridnum = bound.max.x;
        //追加するマップの横幅をもともとあるサイズと足す
        tilemap.size = new Vector3Int(tilemap.size.x + addtilemap.size.x, tilemap.size.y, tilemap.size.z);
        //足した分のサイズにリサイズする（このメソッドを使わないとリサイズしてくれない）
        tilemap.ResizeBounds();

        var list = new List<TileInfo>();

        for (int y = bound.max.y - 1; y >= bound.min.y; --y)//左上から右下にかけてタイルを代入していく
        {
            //もともと現存していたタイルマップの描画
            //上から順に、一列にタイルマップを描画しなおす
            
            for (int x = bound.min.x; x < bound.max.x; ++x)
            {
                var tile = StageGrid.GetComponent<Tilemap>().GetTile<Tile>(new Vector3Int(x, y, 0));
                //var tile2 = StageMaps[num].GetComponent<Tilemap>().GetTile<Tile>(new Vector3Int(x, y, 0));

                var position = new Vector3Int(x, y, 0);
                Vector3 rotation = tilemap.GetTransformMatrix(position).rotation.eulerAngles;//回転を取る

                //var position2 = new Vector3Int(x, y, 0);
                //Vector3 rotation2 = tilemap.GetTransformMatrix(position2).rotation.eulerAngles;//回転を取る

                //タイルのxの位置がタイルの最小の値より大きい（右側に位置している）なら

                var info  = new TileInfo(position, 0, rotation, tile, distance);
                //var info2 = new TileInfo(position2, gridnum + x, rotation, tile2, distance);
                list.Add(info);
                //list.Add(info2);

                //本マップの幅が17以下になったらマップを補充する

                //続いて下記より、追加マップ分のタイルの追加を行う

            }
            
            //Debug.Log("bound.max.x ="+ bound.max.x);
            //Debug.Log("add_bound.min.x =" + add_bound.min.x);
            //Debug.Log("add_bound.max.x =" + add_bound.max.x);
            //Debug.Log("");

            for (int x = add_bound.min.x; x < add_bound.max.x; ++x)
            {
                //var tile = StageGrid.GetComponent<Tilemap>().GetTile<Tile>(new Vector3Int(x, y, 0));
                var tile2 = addtilemap.GetTile<Tile>(new Vector3Int(x, y, 0));

                //var position = new Vector3Int(x, y, 0);
                //Vector3 rotation = tilemap.GetTransformMatrix(position).rotation.eulerAngles;//回転を取る

                var position2 = new Vector3Int(x, y, 0);
                Vector3 rotation2 = addtilemap.GetTransformMatrix(position2).rotation.eulerAngles;//回転を取る

                //Debug.Log("rotation = " + rotation2);

                //タイルのxの位置がタイルの最小の値より大きい（右側に位置している）なら

                //var info = new TileInfo(position, 0, rotation, tile, distance);

                //このコードで間違いない。なんか地形に断裂っぽいのがあったらそういうもんだと思え
                var info = new TileInfo(position2, bound.max.x + Mathf.Abs(add_bound.min.x), rotation2, tile2, distance);
                //list.Add(info);
                list.Add(info);

                //本マップの幅が17以下になったらマップを補充する

                //続いて下記より、追加マップ分のタイルの追加を行う
            }

            //TileInfoクラスを２つ作って、1→2の順で入れていった方が速い

            //y軸の1ごとにx一列の情報を丸ごと代入しているといった感じ

            foreach (var data in list)
            {
                var position = data.m_position;
                var rotation = data.m_rotation;

                if (position.x >= bound.min.x)
                {
                    tilemap.SetTile(position, data.m_tile);
                    Matrix4x4 matrix = Matrix4x4.TRS(Vector3Int.zero, Quaternion.Euler(rotation), Vector3.one);
                    tilemap.SetTransformMatrix(position, matrix);
                }
                else
                {
                    //ここで、本マップ以降のマスはすべて削除している
                    tilemap.SetTile(position, null);
                }

                position.x += 1;
                tilemap.SetTile(position, null);
            }
        }

        Create_Outline_Maps(list);
    }

    void Create_Outline_Maps(List<TileInfo> list)
    {
        var tilemap = Outline_map.GetComponent<Tilemap>();

        var bound = StageGrid.GetComponent<Tilemap>().cellBounds;

            var addtilemap = StageGrid.GetComponent<Tilemap>();
            //int gridnum = bound.max.x;
            //追加するマップの横幅をもともとあるサイズと足す
            tilemap.size = new Vector3Int(addtilemap.size.x, addtilemap.size.y, addtilemap.size.z);
            //足した分のサイズにリサイズする（このメソッドを使わないとリサイズしてくれない）
            tilemap.ResizeBounds();
        

        for (int y = bound.max.y - 1; y >= bound.min.y; --y)//左上から右下にかけてタイルを代入していく
        {
            foreach (var data in list)
            {
                var position = data.m_position;
                var rotation = data.m_rotation;

                if (position.x >= bound.min.x)
                {
                    tilemap.SetTile(position, data.m_tile);
                    Matrix4x4 matrix = Matrix4x4.TRS(Vector3Int.zero, Quaternion.Euler(rotation), Vector3.one);
                    tilemap.SetTransformMatrix(position, matrix);
                }
                else
                {
                    //ここで、本マップ以降のマスはすべて削除している
                    tilemap.SetTile(position, null);
                }

                position.x += 1;
                tilemap.SetTile(position, null);
            }
        }
    }
}
