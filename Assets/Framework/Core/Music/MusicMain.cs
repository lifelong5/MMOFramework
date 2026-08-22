
using UnityEngine;


public class MusicMain : MonoBehaviour
{
    private float v;
    private AudioSource sound2;
#if UNITY_EDITOR
    private void OnGUI()
    {
        if (GUILayout.Button("播放BGM"))
        {
            MusicManager.Instance.PlayBKMusic("BK01");
        }
        if (GUILayout.Button("StopBGM"))
        {
            MusicManager.Instance.StopBKMusic();
        }
        if (GUILayout.Button("PauseBGM"))
        {
            MusicManager.Instance.PauseBKMusic();
        }
        //v = GUILayout.HorizontalSlider(v, 0, 1);
        //MusicManager.Instance.ChangeBKMusicVolume(v);
        if (GUILayout.Button("播放Sound1 非循环"))
        {
            MusicManager.Instance.PlaySound("Sound1");
        }
        if (GUILayout.Button("播放Sound2 循环"))
        {
            MusicManager.Instance.PlaySound("Sound2", true, (sound) =>
            {
                sound2 = sound;
            });
        }
        if (GUILayout.Button("暂停Sound2"))
        {
            MusicManager.Instance.StopSound(sound2);
        }
        if (GUILayout.Button("暂停所有音效"))
        {
            MusicManager.Instance.StopOrPlaySound(false);
        }
        if (GUILayout.Button("播放所有音效"))
        {
            MusicManager.Instance.StopOrPlaySound(true);
        }
    }
#endif
}
