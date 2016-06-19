using UnityEngine;
using System.Collections;
using System.IO;

public class Demo : MonoBehaviour
{
    private bool m_takeScreenshot;

    public void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 200.0f, 32.0f), "Take Screenshot"))
            m_takeScreenshot = true;
    }

    public void OnPostRender()
    {
        if (!m_takeScreenshot)
            return;

        m_takeScreenshot = false;
        var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();
        EncodeManager.Instance.EncodeImage(texture, 5, OnTextureEncoded);
    }

    private void OnTextureEncoded(byte[] png)
    {
        if (png == null || png.Length == 0)
        {
            Debug.LogError("Error encoding texture to PNG! Encoded bytes are null!");
            return;
        }

        var path = Application.persistentDataPath;
        var filename = Path.ChangeExtension(Path.GetRandomFileName(), "png");
        var fullpath = Path.Combine(path, filename);
        Debug.Log("Saving screenshot to " + fullpath);

        using (var bw = new BinaryWriter(File.Open(fullpath, FileMode.Create)))
        {
            bw.Write(png);
        }
    }
}
