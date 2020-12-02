using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR//エディター実行時のみ行う
using UnityEditor;
#endif
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

	public EnemyObjectSetting EnemyObjectSetting;
	public EnemySpriteSetting EnemySpriteSetting;
	public ColliderSetting ColliderSetting;
	public RigidBodySetting RigidBodySetting;
	public AgainstPlayerSetting AgainstPlayerSetting;
}
//敵ステータスの設定(HP、リスポーン状態、）
//敵リジッドボディーの設定（質量、重力、コンスタント状態、コンスタントの解除条件）
//敵がダメージを受ける条件の設定



[Serializable]
public class EnemyObjectSetting
{
	public int HP = 1;
	public float Speed;
	public float Speed_InAir;
	public float Speed_OnIce; 
	public float MaxSpeed;
	public int JumpPower;
	public bool Reapawn;//やられてもリスポーン地点に再びカメラが入れば


	public Animation Animpatch;//Animatorでアニメーションさせる敵のアニメーションデータ
}

[Serializable]
public class EnemySpriteSetting
{
	public Sprite Image;//エディターに表示するスプライト
	public float Scale;//表示するときの倍率
}

[Serializable]
public class EnemyActionSetting
{
	//敵がどう行動するかの選択

	//移動するか
	public bool move;
	//ジャンプするか
	public bool Jump;
	
	//着地した時にちょっと加速するか
	//

	//がつがつ攻める
	//リスポーン地点でジャンプしまくる
	//
	//

	//特定の地点に来るまで待つか（眠り動作を入れ、イベントを受け入れるメソッドも作成しておく）
}

[Serializable]
public class ColliderSetting
{
	//基本的な当たり判定
	public float Circleoffset_y;
	public float radius;

	public bool BoxCol;//着地判定を有効にするか（接地できるようにする）

	public float Boxsize_x;
	public float Boxsize_y;
	public float Boxoffset_x;
	public float Boxoffset_y;
}

[Serializable]
public class RigidBodySetting
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

/*
//[CustomEditor(typeof(RigidBodySetting))]
public class RigidCustom : EditorWindow
{
	public static RigidBodySetting RS;

	[MenuItem("Tools/RigidCustom")]
	public static void Create()
    {
		GetWindow<RigidCustom>("敵のルーチンデータ");

		//EnemyDataまでのパス（わかりやすくするためにAssetにより近い場所に置いた。だから置き場所変えたらアカン）
		var path = "Assets/敵オブジェクトデータファイル/ScriptableObject/EnemyData.asset";
		RS = AssetDatabase.LoadAssetAtPath<RigidBodySetting>(path);





	}
}
*/
[Serializable]
public class AgainstPlayerSetting
{
	public int PlayerDamage;//ダメージ量（最大４。４は即死、大抵１）
	public float PlayerSearchRange;//プレイヤーのサーチ範囲
	public bool PlayerSpinPenetration;//スピンを貫通する
	public bool PlayerCameraOut_thisObjectDelete;//この敵オブジェクトがプレイヤーカメラから外れた時、消えるようにするか
}