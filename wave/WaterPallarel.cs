using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;
using Unity.Burst;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Jobs.LowLevel;
using Unity.Collections;
using UnityEditor.UIElements;
using System.Linq;

public class WaterPallarel : MonoBehaviour
{
    //可変数
    [SerializeField] public bool WaveSetting;
    public float Width = 15;
    public float Bottom;
    public float WavePower;
    public int WaveSpeed;
    public float SplashPower;

    //拡張機能
    [SerializeField] public bool ExpandSetting;

    [SerializeField] public bool Waver;//常に波を打つか
    public bool NotWaver;
    public bool NamerakaWaver;
    public bool GizaGizaWaver;
    [SerializeField] public float WaverPower = 0.001f;//波打つパワー

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
        edgecount = Mathf.RoundToInt(Width) * WaveSpeed;//設定する頂点の数。
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

        //ビルド時に波の高さの基準をここで設定
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
            Vertices[i * 2 + 1] = new Vector3(xpositions[i], ypositions[i] + Bottom, z);//下辺の頂点、左下
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
            //波に沿ってメッシュを張り付ける。
            meshobjects[i] = Instantiate(watermesh, Vector3.zero, Quaternion.identity);
            meshobjects[i].GetComponent<MeshFilter>().mesh = meshes;
            meshobjects[i].transform.parent = transform;

        }

        for (int i = 0; i < nodecount - 1; i++)
        {
            float r = this.transform.position.x + Width * (i + 0.5f) / nodecount * 2;

            if (r > xpositions.Last()) return;
            //子オブジェクトとしてトリガーコライダーをセット
            colliders[i] = new GameObject();
            colliders[i].name = "WaveCollider";
            colliders[i].AddComponent<BoxCollider2D>();
            colliders[i].transform.parent = transform;

            //適切なサイズにコライダーをリサイズ
            colliders[i].transform.position = new Vector3(r, this.transform.position.y - 0.5f, 0);
            colliders[i].transform.localScale = new Vector3(Width / nodecount * 2, 1, 1);

            //各コライダーの設定を変更、反映
            colliders[i].GetComponent<BoxCollider2D>().isTrigger = true;
            colliders[i].AddComponent<WaterDetectorPallarel>();

        }

    }
    void Update()
    {
        AlwaysWaver();//波を揺らす処理を追加したい場合は、様々な処理の事前処理としてここで行う

        nodeUpdate();//次はnodeを更新する

        //バッファ更新地点
        UpdateMeshes();

        if (NamerakaWaver)
        {
            StartCoroutine(WaverAlways());
        }
    }

    private int Num = 0;
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

        //JobHandle.ScheduleBatchedJobs();とComplete()の間に処理を挟むと
        //その分の処理がJobの管理下のスレッドで処理が進むことで並列処理の恩恵を多く受けられる「らしい」。
        //素人だからか、Profiler上で見てもその実感が分からない…
        //が、とりあえず効果はあると信じてこういうわけわからん視認性の悪い書き方してる

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



        if (GizaGizaWaver)
        {
            for (int i = 0; i < nodecount; i++)
            {
                ypositions[Random.Range(0, nodecount)] += Random.Range(nodeWaver[i], WaverPower) / 200;
            }
        }
        nodeWaver.Dispose();

        _jobHandle = Wave.Schedule(xpositions.Length, 5);
        JobHandle.ScheduleBatchedJobs();
        //上記のjobの実行中は他のjobが実行されるとエラるのでそれを防ぐためのComplete()

        _jobHandle.Complete();
    }

    private IEnumerator WaverAlways()
    {
        for (int i = 0; i < nodecount; i++)
        {
            if (ypositions[i] <= WaverPower) ypositions[i] += WaverPower;

            yield return new WaitForSeconds(Random.Range(0f, 0.015f));
            if (ypositions[i] <= WaverPower) ypositions[i] -= WaverPower;
        }
        if (NamerakaWaver) StartCoroutine(WaverAlways());
    }


    void nodeUpdate()
    {
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

    }//ノードを更新させ、波の粗い高低差を自然に直す


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
            for (int j = 0; j < 10; j++)
            {
                //Parallelの中だと！Unity乱数が！使えましぇ――ン！！！！！！
                //擬似乱数
                ft += (i % 11 == 0 ? 0.0013f : i % 9 == 0 ? 0.015776f : i % 7 == 0 ? 0.00672f : i % 5 == 0 ? 0.00379f : i % 3 == 0 ? 0.0091324f : i % 2 == 0 ? 0.0131f : 0.01f);
                if (i % 21 == 0) nodeWaver[i] -= (Mathf.Sin(Mathf.Atan(ft) * 5 + i / 2) + Mathf.Sin(Mathf.Sin(ft / 2) * 7 + i / 2) / 7);
                else if (i % 17 == 0) nodeWaver[i] -= (Mathf.Sin(Mathf.Atan(ft) * 5 + i / 2) + Mathf.Sin(Mathf.Sin(ft / 2) * 7 + i / 2) / 7);
                else if (i % 13 == 0) nodeWaver[i] -= Mathf.Sin(ft) / 10;
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
                wlb1[index] = spread * (ypositions[index] - ypositions[index - 1]);
            }
            if (index < xpositions.Length - 1)
            {
                wrb1[index] = spread * (ypositions[index] - ypositions[index + 1]);
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
            //普通のコードで行うと　ここが滅茶苦茶重い…ホントにマルチスレッド、いやJobSystemさまさまやで
            if (i % 2 == 0) Vertices[i] = new Vector3(xpositions[i / 2], ypositions[i / 2], z);
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
    }

    public void Splash(float xpos, float velocity_x, float velocity_y)
    {

        if (xpos >= xpositions[0] && xpos <= xpositions[xpositions.Length - 1])
        {
            //波を揺らす座標を決める
            xpos -= xpositions[0];
            //与えられた情報から計算されたバッファ情報からどの頂点に触れられたかを計算する
            int index = Mathf.RoundToInt((xpositions.Length - 1) * (xpos / (xpositions[xpositions.Length - 1] - xpositions[0])));

            //最終的な波の緩急処理はここ
            //触れたバッファーの高度をオブジェクトのvelocityの分だけ下げる
            velocities[index] += velocity_y * WavePower;

            //物体が波の表面で横移動をしても波が立つようにする
            if (velocity_x > 0)
            {
                velocities[index + 1 > xpositions.Length ? index + 1 : index] += velocity_x * WavePower;
                velocities[index - 1 < 0 ? index : index - 1] -= velocity_x * 0.2f * WavePower;
            }
            else
            {
                velocities[index + 1 > xpositions.Length ? index + 1 : index] += velocity_x * 0.2f * WavePower;
                velocities[index - 1 < 0 ? index : index - 1] -= velocity_x * WavePower;
            }


            //以下、パーティクルの処理
            float lifetime = 0.93f + Mathf.Abs(velocity_y) * 0.07f * SplashPower;

            //速度によって生成する水しぶきの密度を変更
            int splashCount = (int)Mathf.Abs(velocity_x) + (int)Mathf.Abs(velocity_y);

            splash.GetComponent<ParticleSystem>().startSpeed = Mathf.Abs(velocity_y) * SplashPower + 1;
            splash.GetComponent<ParticleSystem>().startLifetime = lifetime;

            //if (splash.GetComponent<ParticleSystem>().startSpeed > 10) splash.GetComponent<ParticleSystem>().startSpeed = 10;

            Vector3 position = new Vector3(xpositions[index], ypositions[index] - 0.35f, this.transform.position.z + 15);

            Quaternion rotation = Quaternion.LookRotation(new Vector3(xpositions[Mathf.FloorToInt(xpositions.Length / 2)], baseheight + 8, 5) - position);

            for (int i = 0; i < splashCount; i++) Destroy(Instantiate(splash, position, rotation), lifetime + 0.3f);
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


[CustomEditor(typeof(WaterPallarel))]

public class WavePallarelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WaterPallarel WP = target as WaterPallarel;
        WP.WaveSetting = EditorGUILayout.Foldout(WP.WaveSetting, "波の設定 (Wave Setting)");
        if (WP.WaveSetting)
        {
            WP.Width = EditorGUILayout.FloatField("横幅(Width)", WP.Width);
            WP.Bottom = EditorGUILayout.FloatField("深さ (Depth)", WP.Bottom);
            WP.WavePower = EditorGUILayout.FloatField("波のパワー(Reaction)", WP.WavePower);
            WP.WaveSpeed = EditorGUILayout.IntField("波のスピード（Speed）", WP.WaveSpeed);
            WP.SplashPower = EditorGUILayout.FloatField("水しぶきの飛散力(Splash Power)", WP.SplashPower);

            WP.ExpandSetting = EditorGUILayout.Foldout(WP.ExpandSetting, "拡張機能(Expanding Setting)");
            if (WP.ExpandSetting)
            {
                //WP.Waver = EditorGUILayout.Toggle("常時波を揺らす", WP.Waver);
                WP.Waver = EditorGUILayout.Foldout(WP.Waver, "波を揺らす(wave)");
                if (WP.Waver)
                {
                    WP.NotWaver = EditorGUILayout.Toggle("揺らさない", !WP.NamerakaWaver && !WP.GizaGizaWaver);
                    WP.NamerakaWaver = EditorGUILayout.Toggle("滑らかに揺らす", !WP.NotWaver && !WP.GizaGizaWaver);
                    WP.GizaGizaWaver = EditorGUILayout.Toggle("ギザギザに揺らす", !WP.NamerakaWaver && !WP.NotWaver);


                    WP.WaverPower = EditorGUILayout.FloatField("揺らすパワー（Waving Power）", WP.WaverPower);
                }


                //揺らす強さも設定する
            }
        }

        WP.OtherSetting = EditorGUILayout.Foldout(WP.OtherSetting, "マテリアルセット（Materials Setting）");
        if (WP.OtherSetting)
        {
            WP.splash = EditorGUILayout.ObjectField("水しぶきのパーティクル", WP.splash, typeof(GameObject), true) as GameObject;
            WP.mat = EditorGUILayout.ObjectField("テクスチャマテリアル", WP.mat, typeof(Material), true) as Material;
            WP.watermesh = EditorGUILayout.ObjectField("テクスチャメッシュ", WP.watermesh, typeof(GameObject), true) as GameObject;
        }
        EditorUtility.SetDirty(target);//この処理を忘れると変数の変更が反映されない呪いに掛かる
    }
}
