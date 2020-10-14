using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaterPallarel))]

public class WavePallarelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WaterPallarel WP = target as WaterPallarel;
        WP.WaveSetting = EditorGUILayout.Foldout(WP.WaveSetting, "波の設定");
        if (WP.WaveSetting)
        {
            WP.Width = EditorGUILayout.FloatField("横幅", WP.Width);
            WP.Bottom = EditorGUILayout.FloatField("深さ", WP.Bottom);
            WP.WavePower = EditorGUILayout.FloatField("波のパワー", WP.WavePower);
            WP.WaveSpeed = EditorGUILayout.FloatField("波のスピード（処理能力に注意）", WP.WaveSpeed);
            WP.SplashPower = EditorGUILayout.FloatField("水しぶきの飛散力", WP.SplashPower);

            WP.ExpandSetting = EditorGUILayout.Foldout(WP.ExpandSetting, "拡張機能");
            if (WP.ExpandSetting)
            {
                WP.Waver = EditorGUILayout.Toggle("常時波を揺らす", WP.Waver);
                //揺らす強さも設定する
            }
        }

        WP.OtherSetting = EditorGUILayout.Foldout(WP.OtherSetting, "その他の設定");
        if (WP.OtherSetting)
        {
            WP.splash = EditorGUILayout.ObjectField("水しぶきのパーティクル", WP.splash, typeof(GameObject), true) as GameObject;
            WP.mat = EditorGUILayout.ObjectField("テクスチャマテリアル", WP.mat, typeof(GameObject), true) as Material;
            WP.watermesh = EditorGUILayout.ObjectField("テクスチャマテリアル", WP.watermesh, typeof(GameObject), true) as GameObject;
        }
        EditorUtility.SetDirty(target);//この処理を忘れると変数の変更が反映されない呪いに掛かる
    }
}
