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
	[SerializeField] public EnemyObjectSetting EnemyObjectSetting;
	[SerializeField] public EnemySpriteSetting EnemySpriteSetting;
	[SerializeField] public EnemyActionSetting EnemyActionSetting;
	[SerializeField] public ColliderSetting ColliderSetting;
	[SerializeField] public RigidBodySetting RigidBodySetting;
	[SerializeField] public AgainstPlayerSetting AgainstPlayerSetting;
}
//敵ステータスの設定(HP、リスポーン状態、）
//敵リジッドボディーの設定（質量、重力、コンスタント状態、コンスタントの解除条件）
//敵がダメージを受ける条件の設定



[Serializable]
public class EnemyObjectSetting : ScriptableObject
{
	public int HP = 1;
	public bool Respawn;//やられてもリスポーン地点に再びカメラが入れば


	public Animation Animpatch;//Animatorでアニメーションさせる敵のアニメーションデータ
}

[CustomEditor(typeof(EnemyObjectSetting))]
[CanEditMultipleObjects]
public class EnemyObjectSettingEdit : Editor
{
	private void OnEnable()
	{

	}
	public override void OnInspectorGUI()
	{
		EnemyObjectSetting EOS = target as EnemyObjectSetting;
		EOS.HP = EditorGUILayout.IntField("体力", EOS.HP);
		EOS.Respawn = EditorGUILayout.Toggle("復活の可否", EOS.Respawn);

		EditorUtility.SetDirty(target);
	}
}




[Serializable]
public class EnemySpriteSetting : ScriptableObject
{
	public Sprite Image;//エディターに表示するスプライト
	public float Scale;//表示するときの倍率
}

[CustomEditor(typeof(EnemySpriteSetting))]
public class EnemySpriteSettingEdit : Editor
{
	private void OnEnable()
	{

	}
	public override void OnInspectorGUI()
	{
		EnemySpriteSetting ESS = target as EnemySpriteSetting;
		ESS.Image = EditorGUILayout.ObjectField("エディター上で表示する画像", ESS.Image, typeof(Sprite),true) as Sprite;
		ESS.Scale = EditorGUILayout.FloatField("大きさ", ESS.Scale);

		EditorUtility.SetDirty(target);
	}
}


[Serializable]
public class EnemyActionSetting : ScriptableObject
{
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
}

[CustomEditor(typeof(EnemyActionSetting))]
public class EnemyActionSettingEdit: Editor
{
	private void OnEnable()
	{

	}
	public override void OnInspectorGUI()
	{
		EnemyActionSetting EAS = target as EnemyActionSetting;
		EAS.move = EditorGUILayout.Toggle("動く", EAS.move);

		//動ける設定を追加したら、移動速度やスピードの設定をいじれるようになる
		EAS.move = EditorGUILayout.Foldout(EAS.move, "");
		if (EAS.move)
        {
			EAS.Speed = EditorGUILayout.FloatField("雪地での移動速度", EAS.Speed);
			EAS.Speed_InAir = EditorGUILayout.FloatField("空中での移動速度", EAS.Speed_InAir);
			EAS.Speed_OnIce = EditorGUILayout.FloatField("氷上での移動速度", EAS.Speed_OnIce);
			EAS.MaxSpeed = EditorGUILayout.FloatField("最大移動速度", EAS.MaxSpeed);
		}

		EAS.Jump = EditorGUILayout.Toggle("ジャンプする", EAS.Jump);

		//ジャンプをできる設定を追加したら、ジャンプ力などを設定する項目を増やす
		EAS.Jump = EditorGUILayout.Foldout(EAS.Jump, "");
        if (EAS.Jump)
        {
			EAS.JumpPower = EditorGUILayout.IntField("氷上での移動速度", EAS.JumpPower);
		}
		EditorUtility.SetDirty(target);
	}
}

/*
 *private void OnEnable()
	{

	}
	public override void OnInspectorGUI()
	{
		= target as
		EditorUtility.SetDirty(target);
	}
 */


[Serializable]
public class ColliderSetting : ScriptableObject
{
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
}

[CustomEditor(typeof(ColliderSetting))]
public class ColliderSettingEdit: Editor
{
	private void OnEnable()
	{

	}
	public override void OnInspectorGUI()
	{
		ColliderSetting CS = target as ColliderSetting;

		//CS = EditorGUILayout.FloatField("", CS);

		CS.Circleoffset_y = EditorGUILayout.FloatField("当たり判定の中心の高さ", CS.Circleoffset_y);

		CS.BoxCol = EditorGUILayout.Toggle("着地判定", CS.BoxCol);
		CS.BoxCol = EditorGUILayout.Foldout(CS.BoxCol, "着地はするのか");
        if (CS.BoxCol)
        {
			CS.Boxsize = EditorGUILayout.Vector2Field("着地判定の大きさ", CS.Boxsize);
			CS.Boxoffset = EditorGUILayout.Vector2Field("着地判定の中心", CS.Boxoffset);
		}

		EditorUtility.SetDirty(target);
	}
}

[Serializable]
public class RigidBodySetting : ScriptableObject
{
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

}

[CustomEditor(typeof(RigidBodySetting))]
public class RigidBodySettingEdit : Editor
{
	private void OnEnable()
	{

	}
	public override void OnInspectorGUI()
	{
		RigidBodySetting RBS = target as RigidBodySetting;
		RBS.Mass = EditorGUILayout.FloatField("重さ", RBS.Mass);
		RBS.Gravity = EditorGUILayout.FloatField("重力", RBS.Gravity);

		RBS.constant_x = EditorGUILayout.Toggle("constant_x", RBS.constant_x);
		RBS.constant_x = EditorGUILayout.Foldout(RBS.constant_x, "");
        if (RBS.constant_x)
        {
			RBS.PlayerRangeWithconstant_x = EditorGUILayout.Toggle("プレイヤーがサーチ範囲に入ったらx_constantを解除", RBS.PlayerRangeWithconstant_x);

		}

		RBS.constant_y = EditorGUILayout.Toggle("constant_y", RBS.constant_y);
		RBS.constant_y = EditorGUILayout.Foldout(RBS.constant_y,"");
        if (RBS.constant_y)
        {
			RBS.PlayerRangeWithconstant_y = EditorGUILayout.Toggle("プレイヤーがサーチ範囲に入ったらy_constantを解除", RBS.PlayerRangeWithconstant_y);
		}

		RBS.constant_z = EditorGUILayout.Toggle("constant_z", RBS.constant_z);
		RBS.constant_z = EditorGUILayout.Foldout(RBS.constant_z,"");
        if (RBS.constant_z)
        {
			RBS.PlayerRangeWithconstant_z = EditorGUILayout.Toggle("プレイヤーがサーチ範囲に入ったらz_constantを解除", RBS.PlayerRangeWithconstant_z);
		}

		EditorUtility.SetDirty(target);
	}
}

[Serializable]
public class AgainstPlayerSetting : ScriptableObject
{
	public int PlayerDamage;//ダメージ量（最大４。４は即死、大抵１）
	public float PlayerSearchRange;//プレイヤーのサーチ範囲
	public bool PlayerSpinPenetration;//スピンを貫通する
	public bool PlayerCameraOut_thisObjectDelete;//この敵オブジェクトがプレイヤーカメラから外れた時、消えるようにするか
}

[CustomEditor(typeof(AgainstPlayerSetting))]
public class AgainstPlayerSettingEdit : Editor
{
	private void OnEnable()
	{

	}
	public override void OnInspectorGUI()
	{
		AgainstPlayerSetting APS = target as AgainstPlayerSetting;
		APS.PlayerDamage = EditorGUILayout.IntSlider("ダメージ量",APS.PlayerDamage, 1, 4);
		APS.PlayerSearchRange = EditorGUILayout.FloatField("プレイヤーサーチ範囲",APS.PlayerSearchRange);

		APS.PlayerSpinPenetration = EditorGUILayout.Toggle("スピン貫通の可否(プレイヤーのスピンにあたったらダメージを受けるか)", APS.PlayerSpinPenetration);
		APS.PlayerCameraOut_thisObjectDelete = EditorGUILayout.Toggle("プレイヤーカメラから見えなくなった時、消えるようにするか", APS.PlayerCameraOut_thisObjectDelete);
		EditorUtility.SetDirty(target);
	}
}