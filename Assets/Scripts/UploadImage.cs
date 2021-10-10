using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
public class UploadImage : MonoBehaviour
{
    public Texture2D texture;
    public string 上传地址 = "http://localhost/savetofile.asp";
    public string 保存路径= "User/test.png";

    private void OnGUI()
    {
        if (GUILayout.Button("使用 IMultipartFormSection 上传")) //IMultipartFormSection
        {
            UploadSomeImage();
        }else if (GUILayout.Button("使用 WWWForm 上传")) // WWWForm
        {
            UploadSomeImageForm();
        }
    }
    private void UploadSomeImage()
    {
        Debug.Log($"{nameof(UploadImage)}: 开始上传");
        IMultipartFormSection multipart = new MultipartFormFileSection("file", texture.EncodeToPNG(), 保存路径, "image/png");
        var files = new List<IMultipartFormSection>();
        files.Add(multipart);
        UnityWebRequest request = UnityWebRequest.Post(上传地址, files);
        _= HandleRequestAsync(request);
    }

    private void UploadSomeImageForm()
    {
        Debug.Log($"{nameof(UploadImage)}: 开始上传");
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", texture.EncodeToPNG(), 保存路径, "image/png");
        UnityWebRequest request = UnityWebRequest.Post(上传地址, form);
        _= HandleRequestAsync(request);
    }

    private async UniTask HandleRequestAsync(UnityWebRequest request)
    {
        var result = await request.SendWebRequest();
        var msg = result.downloadHandler.text;
        Debug.Log($"{nameof(UploadImage)}: 操作完成！");
        if (msg == "200")
        {
            Debug.Log($"{nameof(UploadImage)}: 文件上传成功，访问地址如下：\n <a href= mujian>http://localhost/{保存路径} </a> ");
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
            var file = Path.Combine(Application.streamingAssetsPath, "WebServer/web", 保存路径);
            if (File.Exists(file))
            {
                var obj = UnityEditor.AssetDatabase.LoadMainAssetAtPath(file.Substring(file.IndexOf("Assets")));
                UnityEditor.EditorGUIUtility.PingObject(obj);
            }
#endif
        }
    }
}
