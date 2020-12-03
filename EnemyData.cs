using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

[Serializable]
[CreateAssetMenu(menuName = "EnemyScriptable/Create EnemyData")]
public class EnemyData : ScriptableObject
{
	/*
	public string enemyName;
	public int maxHp;
	public int atk;
	public int def;
	public int exp;
	public int gold;
	*/

	//MonoBehaviourを継承したクラスのように、クラスをSerializableしてできるようなやつじゃあない


	//敵ステータスの設定(HP、リスポーン状態、）
	[SerializeField] public int HP = 1;
	[SerializeField] public bool Respawn;//やられてもリスポーン地点に再びカメラが入れば


	public Animation Animpatch;//Animatorでアニメーションさせる敵のアニメーションデータ

	public Sprite Image;//エディターに表示するスプライト
	public float Scale;//表示するときの倍率


	//敵がどう行動するかの選択

	//移動するか
	public bool move;

	//移動するとしたら、どんなときにどのくらい動くか
	public float Speed;
	public float Speed_InAir;
	public float Speed_OnIce;

	public float MaxSpeed;

	//ジャンプするか
	public bool Jump;

	public int JumpPower;


	//着地した時にちょっと加速するか
	//

	//がつがつ攻める
	//リスポーン地点でジャンプしまくる
	//
	//

	//特定の地点に来るまで待つか（眠り動作を入れ、イベントを受け入れるメソッドも作成しておく）



	//基本的な当たり判定
	public float Circleoffset_y;
	public float radius;

	//床の判定で着地を行う場合はこのboolをOnにする
	public bool BoxCol;//着地判定を有効にするか（接地できるようにする）

	public float Boxsize_x;
	public float Boxsize_y;
	public float Boxoffset_x;
	public float Boxoffset_y;


	public Vector2 Boxsize;
	public Vector2 Boxoffset;

	//敵リジッドボディーの設定（質量、重力、コンスタント状態、コンスタントの解除条件）
	public float Mass;
	public float Gravity;

	//コンスタント設定
	public bool constant_x;
	public bool constant_y;
	public bool constant_z;

	//プレイヤーのサーチ範囲に入ったら解除する

	public bool PlayerRangeWithconstant_x;
	public bool PlayerRangeWithconstant_y;
	public bool PlayerRangeWithconstant_z;


	//対プレイヤー設定
	public int PlayerDamage;//ダメージ量（最大４。４は即死、大抵１）
	public float PlayerSearchRange;//プレイヤーのサーチ範囲
	public bool PlayerSpinPenetration;//スピンを貫通する
	public bool PlayerCameraOut_thisObjectDelete;//この敵オブジェクトがプレイヤーカメラから外れた時、消えるようにするか
}

[CustomEditor(typeof(EnemyData))]
[CanEditMultipleObjects]
public class EnemyDataEdit : Editor
{
	private void OnEnable()
	{

	}
	public override void OnInspectorGUI()
	{
		EnemyData ED = target as EnemyData;

		/*
	    GUILayout.BeginHorizontal(GUI.skin.box);
		GUILayout.Label(" ");
		GUILayout.EndHorizontal();
		 */

		GUILayout.BeginHorizontal(GUI.skin.box);
		GUILayout.Label("//基本的な敵の情報//");
		GUILayout.EndHorizontal();

		ED.HP = EditorGUILayout.IntField("体力", ED.HP);
		ED.Respawn = EditorGUILayout.Toggle("復活の可否", ED.Respawn);

		GUILayout.Label("");
		GUILayout.Label("");
		GUILayout.BeginHorizontal(GUI.skin.box);
		GUILayout.Label("//エディター上の表示設定//");
		GUILayout.EndHorizontal();

		ED.Image = EditorGUILayout.ObjectField("エディター上で表示する画像", ED.Image, typeof(Sprite), true) as Sprite;
		ED.Scale = EditorGUILayout.FloatField("画像の大きさ", ED.Scale);


		GUILayout.Label("");
		GUILayout.Label("");
		GUILayout.BeginHorizontal(GUI.skin.box);
		GUILayout.Label("//対プレイヤー設定//");
		GUILayout.EndHorizontal();

		ED.PlayerDamage = EditorGUILayout.IntSlider("ダメージ量", ED.PlayerDamage, 1, 4);
		ED.PlayerSearchRange = EditorGUILayout.FloatField("プレイヤーサーチ範囲", ED.PlayerSearchRange);

		ED.PlayerSpinPenetration = EditorGUILayout.Toggle("スピン貫通の可否(プレイヤーのスピンにあたったらダメージを受けるか)", ED.PlayerSpinPenetration);
		ED.PlayerCameraOut_thisObjectDelete = EditorGUILayout.Toggle("プレイヤーカメラから見えなくなった時、消えるようにするか", ED.PlayerCameraOut_thisObjectDelete);



		GUILayout.Label("");
		GUILayout.Label("");
		GUILayout.BeginHorizontal(GUI.skin.box);
		GUILayout.Label("//敵のアクション設定//");
		GUILayout.EndHorizontal();

		ED.move = EditorGUILayout.Toggle("動く", ED.move);

		//動ける設定を追加したら、移動速度やスピードの設定をいじれるようになる
		ED.move = EditorGUILayout.Foldout(ED.move, "");
		if (ED.move)
		{
			ED.Speed = EditorGUILayout.FloatField("雪地での移動速度", ED.Speed);
			ED.Speed_InAir = EditorGUILayout.FloatField("空中での移動速度", ED.Speed_InAir);
			ED.Speed_OnIce = EditorGUILayout.FloatField("氷上での移動速度", ED.Speed_OnIce);
			ED.MaxSpeed = EditorGUILayout.FloatField("最大移動速度", ED.MaxSpeed);
		}

		ED.Jump = EditorGUILayout.Toggle("ジャンプする", ED.Jump);

		//ジャンプをできる設定を追加したら、ジャンプ力などを設定する項目を増やす
		ED.Jump = EditorGUILayout.Foldout(ED.Jump, "");
		if (ED.Jump)
		{
			ED.JumpPower = EditorGUILayout.IntField("ジャンプ力", ED.JumpPower);
		}
		GUILayout.Label("");
		GUILayout.Label("");
		GUILayout.BeginHorizontal(GUI.skin.box);
		GUILayout.Label("//当たり判定の設定//");
		GUILayout.EndHorizontal();

		ED.Circleoffset_y = EditorGUILayout.FloatField("当たり判定の中心の高さ", ED.Circleoffset_y);

		ED.BoxCol = EditorGUILayout.Toggle("着地判定", ED.BoxCol);
		ED.BoxCol = EditorGUILayout.Foldout(ED.BoxCol, "");
		if (ED.BoxCol)
		{
			ED.Boxsize = EditorGUILayout.Vector2Field("着地判定の大きさ", ED.Boxsize);
			ED.Boxoffset = EditorGUILayout.Vector2Field("着地判定の中心", ED.Boxoffset);
		}

		GUILayout.Label("");
		GUILayout.Label("");
		GUILayout.BeginHorizontal(GUI.skin.box);
		GUILayout.Label("//リジッドボディーの設定//");
		GUILayout.EndHorizontal();

		ED.Mass = EditorGUILayout.FloatField("重さ", ED.Mass);
		ED.Gravity = EditorGUILayout.FloatField("重力", ED.Gravity);

		ED.constant_x = EditorGUILayout.Toggle("constant_x", ED.constant_x);
		ED.constant_x = EditorGUILayout.Foldout(ED.constant_x, "");
		if (ED.constant_x)
		{
			ED.PlayerRangeWithconstant_x = EditorGUILayout.Toggle("プレイヤーがサーチ範囲に入ったらx_constantを解除", ED.PlayerRangeWithconstant_x);

		}

		ED.constant_y = EditorGUILayout.Toggle("constant_y", ED.constant_y);
		ED.constant_y = EditorGUILayout.Foldout(ED.constant_y, "");
		if (ED.constant_y)
		{
			ED.PlayerRangeWithconstant_y = EditorGUILayout.Toggle("プレイヤーがサーチ範囲に入ったらy_constantを解除", ED.PlayerRangeWithconstant_y);
		}

		ED.constant_z = EditorGUILayout.Toggle("constant_z", ED.constant_z);
		ED.constant_z = EditorGUILayout.Foldout(ED.constant_z, "");
		if (ED.constant_z)
		{
			ED.PlayerRangeWithconstant_z = EditorGUILayout.Toggle("プレイヤーがサーチ範囲に入ったらz_constantを解除", ED.PlayerRangeWithconstant_z);
		}


		EditorUtility.SetDirty(target);
	}
}