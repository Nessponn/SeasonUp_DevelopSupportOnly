using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Burst;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Jobs.LowLevel;
using Unity.Collections;
public class WaterPallarel : MonoBehaviour
{

    //可変数

    [SerializeField] public bool WaveSetting;
    public float Width;
    public float Bottom;
    public float WavePower = 1;
    public float WaveSpeed = 5;
    public float SplashPower = 2.5f;

    //拡張機能
    [SerializeField] private bool Waver;//常に波を打つか

    //物理演算の結果を書き込み、読み込みを行う
    private NativeArray<Vector3> Vertices;
    private NativeArray<float> xpositions;
    private NativeArray<float> ypositions;
    private NativeArray<float> velocities;
    private NativeArray<float> accelerations;

    //メッシュとコライダー
    GameObject[] meshobjects;
    GameObject[] colliders;
    Mesh meshes;

    [SerializeField] public bool OtherSetting;

    //水しぶきのパーティクル
    public GameObject splash;

    //表面上の波の描画をするためのテクスチャマテリアル
    public Material mat;

    //実際のメッシュ
    public GameObject watermesh;

    //物理演算に必要な定数
    const float springconstant = 0.02f;//バネ定数
    const float damping = 0.04f;//減衰係数。こいつのおかげで弾力のある波が生成される
    const float spread = 0.05f;
    const float z = 10f;

    //元の波の高さ
    float baseheight;

    private int edgecount = 0;
    private int nodecount = 0;

    JobHandle _jobHandle;

    void Start()
    {
        SpawnWater(Width, Bottom);
    }
    //スタート時に生成。設定をするだけなのでここではJobSystemは用いない
    public void SpawnWater(float Width, float Bottom)
    {
        //判定を取り付ける
        gameObject.AddComponent<BoxCollider2D>();
        gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(this.transform.position.x + Width / 2, (this.transform.position.y + Bottom) / 2);
        gameObject.GetComponent<BoxCollider2D>().size = new Vector2(Width, this.transform.position.y - Bottom);
        gameObject.GetComponent<BoxCollider2D>().isTrigger = true;

        edgecount = Mathf.RoundToInt(Width) * 5;//設定する頂点の数。
        nodecount = edgecount + 1;//波に適用される実際の頂点（節目、節合点）。きちんと設定しなければぐちゃぐちゃになる

        xpositions = new NativeArray<float>(nodecount, Allocator.Persistent);
        ypositions = new NativeArray<float>(nodecount, Allocator.Persistent);
        velocities = new NativeArray<float>(nodecount, Allocator.Persistent);
        accelerations = new NativeArray<float>(nodecount, Allocator.Persistent);

        //メッシュと波の他オブジェクトの感知用コライダー
        meshobjects = new GameObject[edgecount];
        colliders = new GameObject[edgecount];

        //もとの高さに戻るように基準を保存
        baseheight = this.transform.position.y;

        //For each node, set the line renderer and our physics arrays
        for (int i = 0; i < nodecount; i++)
        {
            ypositions[i] = this.transform.position.y;
            xpositions[i] = this.transform.position.x + (Width * i) / edgecount;//Leftに設定された値を基準にwidthで指定した広さまで頂点を展開する

            //上記のposition関連の処理はあくまで値を決めただけで、「座標ではない」
            //座標の取り決めはmeshを生成するときに用いる

            accelerations[i] = 0;
            velocities[i] = 0;
        }

        //メッシュつくるー
        meshes = new Mesh();

        //メッシュの要素いろいろ。
        Vertices = new NativeArray<Vector3>(nodecount * 2, Allocator.Persistent);
        Vector2[] UVs = new Vector2[nodecount * 2];
        for (int i = 0; i < nodecount; i++)
        {
            //頂点とUVを生成する。頂点はJobSystemで操作しやすいようにするためにこのような生成をしている。UVはその頂点の生成法に沿って一緒に作っている。
            float r = this.transform.position.x + (Width * i) / edgecount;
            Vertices[i * 2] = new Vector3(xpositions[i], ypositions[i], z);//上辺の頂点、左上
            Vertices[i * 2 + 1] = new Vector3(xpositions[i], Bottom, z);//下辺の頂点、左下
            UVs[i * 2] = new Vector2(r, 1f);
            UVs[i * 2 + 1] = new Vector2(r, 0f);
        }

        int[] tris = new int[3 * 2 * (nodecount - 1)];
        //頂点同士で三角形を作ってつなげる作業。
        for (int i = 0; i < nodecount - 1; i++)
        {
            tris[i * 6 + 0] = i * 2;
            tris[i * 6 + 1] = i * 2 + 1;
            tris[i * 6 + 2] = i * 2 + 2;
            tris[i * 6 + 3] = i * 2 + 2;
            tris[i * 6 + 4] = i * 2 + 3;
            tris[i * 6 + 5] = i * 2 + 1;
        }



        //上記までの生成結果をMeshに反映
            meshes.vertices = Vertices.ToArray();
            meshes.uv = UVs;
            meshes.triangles = tris;

        for (int i = 0; i < nodecount - 1; i++)
        {
            //メッシュを波の四角の数だけ張り付ける。
            meshobjects[i] = Instantiate(watermesh, Vector3.zero, Quaternion.identity);
            meshobjects[i].GetComponent<MeshFilter>().mesh = meshes;
            meshobjects[i].transform.parent = transform;

        }

        
        
        
        for (int i = 0; i < nodecount - 1 ; i++)
        {
            //子オブジェクトとしてトリガーコライダーをセット
            colliders[i] = new GameObject();
            colliders[i].name = "WaveCollider";
            colliders[i].AddComponent<BoxCollider2D>();
            colliders[i].transform.parent = transform;

            //適切なサイズにコライダーをリサイズ
            colliders[i].transform.position = new Vector3(this.transform.position.x + Width * (i + 0.5f) / nodecount * 2, this.transform.position.y - 0.5f, 0);
            colliders[i].transform.localScale = new Vector3(Width / nodecount * 2, 1, 1);

            //各コライダーの設定を変更、反映
            colliders[i].GetComponent<BoxCollider2D>().isTrigger = true;
            colliders[i].AddComponent<WaterDetectorPallarel>();
        }
        
    }


    private float ft = 0;//波を常時揺らしたいときにid
    void AlwaysWaver()//拡張機能。常に波を揺らす
    {
        NativeArray<float> nodeWaver = new NativeArray<float>(nodecount, Allocator.TempJob);

        AlwaysWaverParallel Awave = new AlwaysWaverParallel()
        {
            nodeWaver = nodeWaver,
            ft = ft,
        };
        _jobHandle = Awave.Schedule(nodecount, 2);

        JobHandle.ScheduleBatchedJobs();

        WavePallarel Wave = new WavePallarel()
        {
            xpositions = xpositions,
            ypositions = ypositions,
            velocities = velocities,
            WavePower = WavePower,
            accelerations = accelerations,

            baseheight = baseheight,
        };

        _jobHandle.Complete();

        //Debug.Log("ypositions = " + ypositions.Length);

        for (int i = 0; i < nodecount; i++)
        {
            ypositions[Random.Range(i, (i + 20 < nodecount ? i + 20 : i + 10 < nodecount ? i + 10 : i + 5 < nodecount ? i + 5 : i))] += (nodeWaver[i] / 500);
        }
        nodeWaver.Dispose();

        _jobHandle = Wave.Schedule(xpositions.Length, 5);
        JobHandle.ScheduleBatchedJobs();
        //上記のjobの実行中は他のjobが実行されるとエラるのでそれを防ぐためのComplete()

        _jobHandle.Complete();
    }

    void WavingBuffer()
    {
        
    }//オブジェクトから引き渡された速度をもとに波をゆらし、伝番させる

    void nodeUpdate()
    {

        
    }//ノードを更新させ、波の粗い高低差を自然に直す
    
    // Update is called once per frame
    void Update()
    {
        if(Waver) AlwaysWaver();//波を揺らす処理を追加したい場合は、様々な処理の事前処理としてここで行う
       
        
        //WavingBuffer();//波のバッファごとにくわえられた速度をもとに波の減衰、伝番処理を行う
        
        //jobの実行（繰り返す回数,処理分割回数）
        
        //LineRendererの座標更新（LineRendererはIJobPallarelでは使えない）
        //for (int i = 0; i < xpositions.Length; i++)
        //{
            //Body.SetPosition(i, new Vector3(xpositions[i], ypositions[i], z));
       // }


        // nodeUpdate();//次はnodeを更新する

        NativeArray<float> wlb1 = new NativeArray<float>(xpositions.Length, Allocator.TempJob);
        NativeArray<float> wrb1 = new NativeArray<float>(xpositions.Length, Allocator.TempJob);

        NodesUpdates NodeUpd = new NodesUpdates()
        {
            xpositions = xpositions,
            ypositions = ypositions,
            wlb1 = wlb1,
            wrb1 = wrb1,
        };

        _jobHandle = NodeUpd.Schedule(xpositions.Length, 8);

        JobHandle.ScheduleBatchedJobs();

        _jobHandle.Complete();

        for (int i = 0; i < xpositions.Length; i++)
        {
            if (i > 0)
            {
                velocities[i - 1] += wlb1[i];
                ypositions[i - 1] += wlb1[i];
            }
            if (i < xpositions.Length - 1)
            {
                velocities[i + 1] += wrb1[i];
                ypositions[i + 1] += wrb1[i];
            }

        }
        wlb1.Dispose();
        wrb1.Dispose();

        //バッファ更新地点
        UpdateMeshes();

    }
    [BurstCompile]
    struct WavePallarel : IJobParallelFor
    {
        public NativeArray<float> xpositions;
        public NativeArray<float> ypositions;
        public NativeArray<float> velocities;
        public NativeArray<float> accelerations;
        public float WavePower;

        public float baseheight;
        public void Execute(int i)//ここのindexにはxposition.Lengthの値が入る
        {
            float force = springconstant * (ypositions[i] - baseheight) + velocities[i] * damping;//velocitys * αが加速度として使われる
            accelerations[i] = -force;//a = -(k * x + vd)
            ypositions[i] += velocities[i];

            //if (Mathf.Abs(ypositions[i]) <= 0.01f) ypositions[i] = 0;//数値の処理をミリ単位で続行するのは重くなるので０に収束させる
            velocities[i] += accelerations[i];
        }
    }
    [BurstCompile]
    struct AlwaysWaverParallel : IJobParallelFor
    {
        public NativeArray<float> nodeWaver;
        public float ft;
        public void Execute(int i)
        {
            for (int j = 0; j < 10; j++){
                //Parallelの中だと！Unity乱数が！使えましぇ――ン！！！！！！
                //擬似乱数
                ft += (i % 11 == 0 ? 0.0013f : i % 9 == 0 ? 0.015776f : i % 7 == 0 ? 0.00672f : i % 5 == 0 ? 0.00379f : i % 3 == 0 ? 0.0091324f : i % 2 == 0 ? 0.0131f : 0.01f);
                //nodeWaver[i] = ypositions[i] + (Mathf.Sin(Mathf.Cos(ft) * 7 + i / 2) / 7 + Mathf.Sin(Mathf.Cos(ft / 2) * 7 + i / 2) / 7) / 2;
                if (i % 7 == 0) nodeWaver[i] += (Mathf.Atan(Mathf.Atan(ft) * Mathf.Sin(ft / 3.4f) * 3 + i / 2) / 9 + Mathf.Atan(Mathf.Sin(ft / 2.7f) * 3 + i / 2) / 7) / 15;
                else if (i % 2 == 0) nodeWaver[i] -= (Mathf.Sin(Mathf.Atan(ft) * 5 + i / 2) + Mathf.Sin(Mathf.Sin(ft / 2) * 7 + i / 2) / 7);
                else if (i % 9 == 0) nodeWaver[i] -= Mathf.Sin(ft) / 10;
            }
        }
    }
    [BurstCompile]
    struct NodesUpdates : IJobParallelFor
    {

        [ReadOnly] public NativeArray<float> xpositions;
        [ReadOnly] public NativeArray<float> ypositions;
        

        //ypositionsとvelocitiesはそれぞれindexと違う値を参照するためReadOnlyにする必要がある。
        //よって最終的な書き込みは明示されたWriteOnlyでの変数からIJobPallarel脱出後に
        //ypositionsとvelocitiesにそれぞれ書き込みなおす必要がある

        //と、思ったが…
        //PallarelForにて配列を参照するときはindexと同じ番号の配列を参照しないとダメです、エラります（りふじんじん）
        
        //最初の配列の値はすべて０。初期化はFixedUpdateに突入したすぐに行う

        public NativeArray<float> wlb1;
        public NativeArray<float> wrb1;
        public void Execute(int index)//8を入れる(なんかindexの値と配列で参照する番号が一致していないとエラーが起きるっぽい？)
        {
            if (index > 0)
            {
                wlb1[index] = spread * (ypositions[index] - ypositions[index-1]);
            }
            if (index < xpositions.Length-1)
            {
                wrb1[index] = spread * (ypositions[index] - ypositions[index+1]);
            }

        }
    }

    [BurstCompile]
    struct UpdateMeshesParallel : IJobParallelFor
    {
        public NativeArray<Vector3> Vertices;
        [ReadOnly] public NativeArray<float> xpositions;
        [ReadOnly] public NativeArray<float> ypositions;

        public void Execute(int i)//ここでメッシュの更新を行う
        {
            if (i % 2 == 0) Vertices[i] = new Vector3(xpositions[i/2], ypositions[i/2], z);
        }
    }

    void UpdateMeshes()
    {

        meshes.vertices = Vertices.ToArray();//更新の反映

        UpdateMeshesParallel UMP = new UpdateMeshesParallel
        {
            Vertices = Vertices,
            xpositions = xpositions,
            ypositions = ypositions,
        };
        _jobHandle = UMP.Schedule(Vertices.Length, 0);

        JobHandle.ScheduleBatchedJobs();

        _jobHandle.Complete();

        

        
        

        
        /*
        for (int i = 0; i < nodecount; i++)
        {
            Vertices[i*2] = new Vector3(xpositions[i], ypositions[i], z);
        }
        
        meshes.vertices = Vertices;
        */
    }

    public void Splash(float xpos, float velocity_x, float velocity)
    {
        //波から法線を取り、その角度に向けて飛ばせば行ける（JobSystem使用）


        //If the position is within the bounds of the water:
        if (xpos >= xpositions[0] && xpos <= xpositions[xpositions.Length - 1])
        {
            //Offset the x position to be the distance from the left side
            //波を揺らす座標を決める
            //Debug.Log("xpos = " + xpos);
            //Debug.Log("xpositions[0] = " + xpositions[0]);
            xpos -= xpositions[0];
            //Debug.Log("xpos after= " + xpos);

            //Find which spring we're touching
            //与えられた情報から計算されたバッファ情報からどの頂点に触れられたかを計算する
            int index = Mathf.RoundToInt((xpositions.Length - 1) * (xpos / (xpositions[xpositions.Length - 1] - xpositions[0])));
            //Debug.Log("index= " + index);

            //Add the velocity of the falling object to the spring
            //最終的な波の緩急処理はここ
            //触れたバッファーの高度をオブジェクトのvelocityの分だけ下げる
            velocities[index] += velocity * WavePower;

            //物体が波の表面で横移動をしても波が立つようにする
            if (velocity_x > 0)
            {
                velocities[index + 1 > xpositions.Length ? index + 1 : index] += velocity_x * WavePower;
                velocities[index - 1 < 0 ? index : index - 1] -= velocity_x * 0.2f * WavePower;
                //Debug.Log("右向き");
            }
            else
            {
                velocities[index + 1 > xpositions.Length ? index + 1 : index] += velocity_x * 0.2f * WavePower;
                velocities[index - 1 < 0 ? index : index - 1] -= velocity_x * WavePower;
                //Debug.Log("左向き");
            }


            //以下、パーティクルの処理
            //Set the lifetime of the particle system.
            float lifetime = 0.93f + Mathf.Abs(velocity) * 0.07f;

            //Set the splash to be between two values in Shuriken by setting it twice.
            //速度によって生成する水しぶきの密度を変更
            int splashCount = (int)Mathf.Abs(velocity_x) + (int)Mathf.Abs(velocity);

            if (velocity <= -7) velocity = -7;
            //if (velocity <= -20) velocity = -20;
            
            splash.GetComponent<ParticleSystem>().startSpeed = ((Mathf.Abs(velocity) * SplashPower ) * 2 )  * Mathf.Pow(Mathf.Abs(velocity) + 1, Mathf.Abs(velocity)  + 1) + 1;
            //splash.GetComponent<ParticleSystem>().startSpeed = (Mathf.Abs(velocity) * 2) + 2 * Mathf.Pow(Mathf.Abs(velocity) + 1, Mathf.Abs(velocity) + 1);
            splash.GetComponent<ParticleSystem>().startLifetime = lifetime;

            if (splash.GetComponent<ParticleSystem>().startSpeed > 10) splash.GetComponent<ParticleSystem>().startSpeed = 10;

            //Debug.Log("velocity = "+velocity);
            //Set the correct position of the particle system.
            Vector3 position = new Vector3(xpositions[index], ypositions[index] - 0.35f, 5);

            //This line aims the splash towards the middle. Only use for small bodies of water:
            Quaternion rotation = Quaternion.LookRotation(new Vector3(xpositions[Mathf.FloorToInt(xpositions.Length / 2)], baseheight + 8, 5) - position);

            //Create the splash and tell it to destroy itself.
            //GameObject splish = Instantiate(splash, position, rotation) as GameObject;
            //Destroy(splish, lifetime + 0.3f);
            for(int i = 0; i < splashCount; i++) Destroy(Instantiate(splash, position, rotation), lifetime + 0.3f);
        }
    }

    public void OnDestroy()
    {
        xpositions.Dispose();
        ypositions.Dispose();
        velocities.Dispose();
        accelerations.Dispose();
        Vertices.Dispose();
    }






}
