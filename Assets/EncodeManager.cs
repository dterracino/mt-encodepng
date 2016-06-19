using UnityEngine;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using Moxie.PngEncoder;

public class EncodeManager : MonoBehaviour
{
    #region Public interface

    public static EncodeManager Instance
    {
        get
        {
            if (s_instance == null)
            {
                var go = new GameObject("EncodeManager");
                s_instance = go.AddComponent<EncodeManager>();
            }

            return s_instance;
        }
    }

    #endregion

    #region Private state

    private static EncodeManager s_instance;
    private AutoResetEvent m_finishedEvent;
    private Action<byte[]> m_callback;
    private byte[] m_encodedImage;
    private bool m_inProgress;

    #endregion

    #region Public interface

    public void Awake()
    {
        m_finishedEvent = new AutoResetEvent(false);
        s_instance = this;
    }

    public void Update()
    {
        if (m_finishedEvent.WaitOne(0))
        {
            // Image encoding has finished! Notify the caller.
            m_callback(m_encodedImage);
            m_encodedImage = null;
            m_inProgress = false;
        }
    }

    public void EncodeImage(Texture2D texture, int compressionLevel, Action<byte[]> callback)
    {
        if (m_inProgress)
            return;

        var format = texture.format;
        if (format != TextureFormat.ARGB32)
            throw new NotSupportedException("Unsupported texture format " + format.ToString());

        m_callback = callback;
        m_inProgress = true;
        var rawBytes = texture.GetRawTextureData();

        var width = texture.width;
        var height = texture.height;
        ThreadPool.QueueUserWorkItem(_ => EncodeImage(rawBytes, width, height, compressionLevel));
    }

    #endregion

    #region Private implementation

    private void EncodeImage(byte[] rawImageBytes, int width, int height, int compressionLevel)
    {
        // Convert to int array (which the encoder wants) and flip vertically, because otherwise it's upside down!
        var rawImageInt = new int[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var srcPos = (x * 4) + (y * width * 4);
                var a = rawImageBytes[srcPos];
                var r = rawImageBytes[srcPos + 1];
                var g = rawImageBytes[srcPos + 2];
                var b = rawImageBytes[srcPos + 3];
                int val = (a << 24) | (r << 16) | (g << 8) | b;
                var dstPos = x + (height - y - 1) * width;
                rawImageInt[dstPos] = val;
            }
        }

        // Encode the image.
        var pngEncoder = new PngEncoder(rawImageInt, width, height, false, PngEncoder.FILTER_NONE, compressionLevel);
        m_encodedImage = pngEncoder.Encode(false);

        // Notify the main thread.
        m_finishedEvent.Set();
    }

    #endregion
}
