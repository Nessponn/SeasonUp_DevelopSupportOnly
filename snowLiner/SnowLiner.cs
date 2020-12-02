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
using UnityEngine.Tilemaps;

public class SnowLiner : MonoBehaviour
{
    public float Width = 15;
    public float Bottom;
    public float minDepth;
    public float maxDepth;

    public bool waterlike;

    //物理演算の結果を書き込み、読み込みを行う
    private Vector3[] Vertices;
    private float[] xpositions;
    private float[] ypositions;

    //メッシュとコライダー
    private List<GameObject> meshobjects;
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

    const float z = 10f;


    private int edgecount = 0;
    private int nodecount = 0;

    JobHandle _jobHandle;

    void Awake()
    {
        SpawnWater(Width, Bottom);
        if(waterlike) StartCoroutine(WaterLike()); ;
    }
    //スタート時に生成。設定をするだけなのでここではJobSystemは用いない
    public void SpawnWater(float Width, float Bottom)
    {

        edgecount = Mathf.RoundToInt(Width) * 20;//設定する頂点の数。
        nodecount = edgecount + 1;//波に適用される実際の頂点（節目、節合点）。きちんと設定しなければぐちゃぐちゃになる

        xpositions = new float[nodecount];
        ypositions = new float[nodecount];

        //メッシュと波の他オブジェクトの感知用コライダー
        meshobjects = new List<GameObject>(edgecount);

        //もとの高さに戻るように基準を保存

        //ビルド時に波の高さの基準をここで設定
        for (int i = 0; i < nodecount; i++)
        {
            ypositions[i] = this.transform.position.y;
            xpositions[i] = this.transform.position.x + (Random.Range(20,40)* i) / edgecount;//Leftに設定された値を基準にwidthで指定した広さまで頂点を展開する
            //上記のposition関連の処理はあくまで値を決めただけで、「座標ではない」
            //座標の取り決めはmeshを生成するときに用いる
        }

        //メッシュつくるー
        meshes = new Mesh();

        //メッシュの要素いろいろ。
        Vertices = new Vector3[nodecount * 2];
        Vector2[] UVs = new Vector2[nodecount * 2];
        for (int i = 0; i < nodecount; i++)
        {
            //頂点とUVを生成する。頂点はJobSystemで操作しやすいようにするためにこのような生成をしている。UVはその頂点の生成法に沿って一緒に作っている。
            Vertices[i * 2] = new Vector3(xpositions[i], 1, z);//上辺の頂点、左上
            Vertices[i * 2 + 1] = new Vector3(xpositions[i], 1 -Bottom, z);//下辺の頂点、左下
            UVs[i * 2] = new Vector2(xpositions[i], 1f);
            UVs[i * 2 + 1] = new Vector2(xpositions[i], 0f);
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
        meshes.vertices = Vertices;
        meshes.uv = UVs;
        meshes.triangles = tris;

        for (int i = 0; i < nodecount - 1; i++)
        {
            //波に沿ってメッシュを張り付ける。

            float r = Width / edgecount;

            float rt = this.transform.position.x + (Width * i) / edgecount;

            if(waterlike) meshobjects[i] = Instantiate(watermesh, new Vector3(r, this.transform.position.y, 10), Quaternion.identity);
            else meshobjects[i] = Instantiate(watermesh, new Vector3(r,this.transform.position.y,-10), Quaternion.identity);
            meshobjects[i].GetComponent<MeshFilter>().mesh = meshes;

            meshobjects[i].layer = LayerMask.NameToLayer("Water");

            meshobjects[i].transform.localScale= new Vector3(r /15,Random.Range(minDepth ,maxDepth * Mathf.Abs(Mathf.Sin(rt)) + minDepth), 1);//ここで雪の高低差を
            if (waterlike) meshobjects[i].transform.localPosition = new Vector3(rt, meshobjects[i].transform.localPosition.y, 10);
            else     meshobjects[i].transform.localPosition = new Vector3(rt, meshobjects[i].transform.localPosition.y, -10);

            meshobjects[i].AddComponent<Rigidbody2D>();

            meshobjects[i].GetComponent<Rigidbody2D>().useAutoMass = true;
            meshobjects[i].GetComponent<Rigidbody2D>().gravityScale = 0.3f;
            meshobjects[i].GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            meshobjects[i].GetComponent<Rigidbody2D>().sharedMaterial = PM;

            meshobjects[i].AddComponent<BoxCollider2D>();


            meshobjects[i].GetComponent<BoxCollider2D>().offset = new Vector2(6 + transform.position.x * 1.1f, 1);
            meshobjects[i].GetComponent<BoxCollider2D>().size = new Vector2(50, 0.2f);

             meshobjects[i].transform.parent = transform;

            meshobjects[i].GetComponent<Rigidbody2D>().velocity = new Vector2(0,-8.5f);
        }
    }

    private IEnumerator WaterLike()
    {
        yield return new WaitForSeconds(0.7f);
        SpawnWater(Width, Bottom);
        StartCoroutine(WaterLike());
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
                    //Debug.Log("高さ補正");
                    meshobjects[i].transform.localPosition = new Vector2(meshobjects[i].transform.localPosition.x, ypositions[i]);
                }

                if (meshobjects[i].GetComponent<Rigidbody2D>().velocity.y <= -8.5f)
                {
                    meshobjects[i].GetComponent<Rigidbody2D>().velocity = new Vector2(0, -8.5f);
                }

                if(meshobjects[i].transform.localPosition.y <= this.transform.position.y - 25)
                {
                    Destroy(meshobjects[i]);
                }

            }
        }
    }
}
