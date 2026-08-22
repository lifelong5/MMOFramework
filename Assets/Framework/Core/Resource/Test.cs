using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Test : MonoBehaviour
{
    AssetBundle ab;
    // Start is called before the first frame update
    void Start()
    {
        ABResManager.Instance.LoadResAsync<GameObject>("model", "Cube", (obj) =>
        {
            Instantiate(obj);
        }, false);
        //ABManager.Instance.LoadRes<GameObject>("model", "Cube", (obj) =>
        //{
        //    Instantiate(obj);
        //},true);
        //ABManager.Instance.UnLoad("model", true);
        //ABManager.Instance.UnLoadAllAssetBundle(true);
        //ab = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + "model");
        ////通过主包来获取到AB包的依赖
        //AssetBundle ABMain = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + "PC");
        //AssetBundleManifest ABMainfest = ABMain.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        //string[] dependencies = ABMainfest.GetAllDependencies("model");
        //foreach (string name in dependencies)
        //{
        //    AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + name);
        //}
        //GameObject cube = ab.LoadAsset<GameObject>("Cube");
        //Instantiate(cube);
        //StartCoroutine(loadAsset<GameObject>("model", "Cube"));
    }
    IEnumerator loadAsset<T>(string path,string name)
    {
        AssetBundleCreateRequest abc = AssetBundle.LoadFromFileAsync(Application.streamingAssetsPath + "/" + path);
        yield return abc;
        AssetBundleRequest abq = abc.assetBundle.LoadAssetAsync<GameObject>(name);
        yield return abq;
        Instantiate(abq.asset);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AssetBundle.UnloadAllAssetBundles(true);
        }
    }
}
