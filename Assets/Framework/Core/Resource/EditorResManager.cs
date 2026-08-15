using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 编辑器下使用的资源加载
/// 需要注意的是 path是需要带后缀的 并且需要在Assets/Editor/ArtRes/文件夹下
/// </summary>
public class EditorResManager : Singleton<EditorResManager>
{
    private EditorResManager() { }
    private string root = "Assets/Editor/ArtRes/";//路径
    /// <summary>
    /// 加载某个资源 同步的
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T LoadEditorRes<T>(string path) where T : Object
    {
#if UNITY_EDITOR
        string assetPath = root + path;
        return AssetDatabase.LoadAssetAtPath<T>(assetPath);
#else
        return null;
#endif
    }
    /// <summary>
    /// 加载图集中的某个图片
    /// </summary>
    /// <param name="path"></param>
    /// <param name="spriteName"></param>
    /// <returns></returns>
    public Sprite LoadSprite(string path,string spriteName)
    {
#if UNITY_EDITOR
        string assetPath = root + path;
        Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);//加载图集中的所有资源
        foreach(Object sprite in sprites)
        {
            if(sprite.name == spriteName)
            {
                return sprite as Sprite;
            }
        }
        return null;
#else
        return null;
#endif
    }
    /// <summary>
    /// 加载图集 频繁使用某个图集中的图片时候使用
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Dictionary<string,Sprite> LoadAllSprite(string path)
    {
#if UNITY_EDITOR
        Dictionary<string, Sprite> spriteDic = new Dictionary<string, Sprite>();
        string assetPath = root + path;
        Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
        foreach (Object sprite in sprites)
        {
            spriteDic.Add(sprite.name, sprite as Sprite);
        }
        return spriteDic;
#else
        return null;
#endif
    }
}
