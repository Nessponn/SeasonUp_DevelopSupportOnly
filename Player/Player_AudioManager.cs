using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_AudioManager : MonoBehaviour
{
    private AudioSource AS;
    // Start is called before the first frame update
    void Start()
    {
        AS = GetComponent<AudioSource>();
    }

    public void AudioPlayer(AudioClip AC)//各スクリプトに埋め込まれたオーディオファイルをここに代入して鳴らす
    {
        Debug.Log("なってるよ");
        AS.clip = AC;
        AS.Play();
    }
    public void AudioPlayerOnplayOneShot(AudioClip AC)//効果音として一度だけ鳴らすバージョン
    {
        AS.PlayOneShot(AC);
    }
    public void AudioStop()
    {
        AS.Stop();
    } 
}
