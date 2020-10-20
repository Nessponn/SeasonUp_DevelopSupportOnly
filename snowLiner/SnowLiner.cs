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
public class SnowLiner : MonoBehaviour
{
    public float Width = 15;
    public float Bottom;
    public float minDepth;
    public float maxDepth;

    //物理演算の結果を書き込み、読み込みを行う
    private NativeArray<Vector3> Vertices;
    private NativeArray<float> xpositions;
    private float[] ypositions;
    private NativeArray<float> velocities;
    private NativeArray<float> accelerations;

    //メッシュとコライダー
    GameObject[] meshobjects;
    Mesh meshes;

    [SerializeField] public bool OtherSetting;

    //水しぶきのパーティクル
    public GameObject splash;

    //表面上の波の描画をするためのテクスチャマテリアル
    public Material mat;

    //実際のメッシュ
    public GameObject watermesh;

    public PhysicsMaterial2D PM;

    //PhysicsMaterialが３Dと２Dで書き方ちげぇ…わかりづら。
    //3D... Physic Material (スペースは開けない)
    //2D... PhysicsMaterial2D

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

    void Awake()
    {
        SpawnWater(Width, Bottom);
    }
    //スタート時に生成。設定をするだけなのでここではJobSystemは用いない
    public void SpawnWater(float Width, float Bottom)
    {
        //判定を取り付ける
        //gameObject.AddComponent<BoxCollider2D>();
        //gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(this.transform.position.x + Width / 2, (this.transform.position.y + Bottom) / 2);
        //gameObject.GetComponent<BoxCollider2D>().size = new Vector2(Width, this.transform.position.y - Bottom);
        //gameObject.GetComponent<BoxCollider2D>().isTrigger = true;

        edgecount = Mathf.RoundToInt(Width) * 15;//設定する頂点の数。
        nodecount = edgecount + 1;//波に適用される実際の頂点（節目、節合点）。きちんと設定しなければぐちゃぐちゃになる

        xpositions = new NativeArray<float>(nodecount, Allocator.Persistent);
        ypositions = new float[nodecount];
        velocities = new NativeArray<float>(nodecount, Allocator.Persistent);
        accelerations = new NativeArray<float>(nodecount, Allocator.Persistent);

        //メッシュと波の他オブジェクトの感知用コライダー
        meshobjects = new GameObject[edgecount];
        //colliders = new GameObject[edgecount];

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
            Vertices[i * 2] = new Vector3(xpositions[i], 1, z);//上辺の頂点、左上
            Vertices[i * 2 + 1] = new Vector3(xpositions[i], 1 -Bottom, z);//下辺の頂点、左下
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


            float r = Width / edgecount;

            float rt = this.transform.position.x + (Width * i) / edgecount;

            meshobjects[i] = Instantiate(watermesh, new Vector3(r,0,-10), Quaternion.identity);
            meshobjects[i].GetComponent<MeshFilter>().mesh = meshes;

            meshobjects[i].layer = LayerMask.NameToLayer("Water");

            meshobjects[i].transform.localScale= new Vector3(r /15,Random.Range(minDepth ,maxDepth * Mathf.Abs(Mathf.Sin(rt)) + minDepth), 1);//ここで雪の高低差を

            meshobjects[i].transform.localPosition = new Vector3(rt, 1, -10);

            /*
            Ray ray = new Ray(transform.position, transform.forward);

            RaycastHit2D hit = Physics2D.Raycast(new Vector2(meshobjects[i].transform.position.x, meshobjects[i].transform.position.y-3), -Vector3.up,10,LayerMask.NameToLayer("floor")) ; //レイが衝突したオブジェクト
            Debug.Log(hit);
            if (hit.collider != null)
            {
                Debug.Log("反応したよ");
                meshobjects[i].transform.localPosition = new Vector3(rt, hit.point.y, -10);
            }
            */
            meshobjects[i].AddComponent<Rigidbody2D>();
            meshobjects[i].GetComponent<Rigidbody2D>().useAutoMass = true;
            meshobjects[i].GetComponent<Rigidbody2D>().gravityScale = 0.3f;
            meshobjects[i].GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            meshobjects[i].GetComponent<Rigidbody2D>().sharedMaterial = PM;

            meshobjects[i].AddComponent<BoxCollider2D>();


            //meshobjects[i].GetComponent<BoxCollider2D>().offset = new Vector2(xpositions[i], ypositions[i]);
            meshobjects[i].GetComponent<BoxCollider2D>().offset = new Vector2(0, 1);
            meshobjects[i].GetComponent<BoxCollider2D>().size = new Vector2(1 * 15, 0.2f);

             meshobjects[i].transform.parent = transform;

            meshobjects[i].GetComponent<Rigidbody2D>().velocity = new Vector2(0,-7);
        }
        /*
        for (int i = 0; i < nodecount - 1; i++)
        {
            //子オブジェクトとしてトリガーコライダーをセット
            colliders[i] = new GameObject();
            colliders[i].name = "WaveCollider";
            colliders[i].AddComponent<BoxCollider2D>();
            //colliders[i].AddComponent<Rigidbody2D>();
            colliders[i].transform.parent = transform;

            //適切なサイズにコライダーをリサイズ
            colliders[i].transform.position = new Vector3(this.transform.position.x + Width * (i + 0.5f) / nodecount * 2, this.transform.position.y - 0.5f, 0);
            colliders[i].transform.localScale = new Vector3(Width / nodecount * 2, 1, 1);

            //各コライダーの設定を変更、反映
            //colliders[i].GetComponent<BoxCollider2D>().isTrigger = true;
            //colliders[i].AddComponent<WaterDetectorPallarel>();
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0;i<nodecount - 1; i++)
        {
            if(meshobjects[i] != null)
            {

                if (meshobjects[i].transform.localPosition.y <= ypositions[i])
                {
                    ypositions[i] = meshobjects[i].transform.localPosition.y;
                }
                //Debug.Log("ypositions[i] = "+ ypositions[i]);
                if (meshobjects[i].transform.localPosition.y >= ypositions[i] + 0.5f)//記録した高さよりも
                {
                    Debug.Log("高さ補正");
                    meshobjects[i].transform.localPosition = new Vector2(meshobjects[i].transform.localPosition.x, ypositions[i]);
                }

                if (meshobjects[i].GetComponent<Rigidbody2D>().velocity.y <= -7f)
                {
                    meshobjects[i].GetComponent<Rigidbody2D>().velocity = new Vector2(0, -7f);
                }

                if(meshobjects[i].transform.localPosition.y <= transform.position.y - 25)
                {
                    meshobjects[i] = null;
                }
            }
        }
    }
    public void OnDestroy()
    {
        xpositions.Dispose();
        velocities.Dispose();
        accelerations.Dispose();
        Vertices.Dispose();
    }
}
