using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace COM3D2.PropMyItem.Plugin;

public class SavePreset
{
    private Vector2 _savedAngle;

    private float _savedDistance = 1f;

    private Vector3 _savedPos;

    private string bg = string.Empty;

    private float fov;

    private bool isStock;

    private float px;

    private float py;

    private float pz;

    private float rw;

    private float rx;

    private float ry;

    private float rz;

    public Texture2D ThumShot(Camera camera, Maid f_maid)
    {
        var width = 138;
        var height = 200;
        var transform = CMT.SearchObjName(f_maid.body0.m_Bones.transform, "Bip01 HeadNub");
        camera.transform.position =
            transform.TransformPoint(transform.localPosition + new Vector3(0.38f, 1.07f, 0f));
        camera.transform.rotation = transform.rotation * Quaternion.Euler(90f, 0f, 90f);
        camera.fieldOfView = 30f;
        var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        renderTexture.filterMode = FilterMode.Bilinear;
        renderTexture.antiAliasing = 8;
        var f_rtSub = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        var result = RenderThum(camera, renderTexture, f_rtSub, width, height);
        Restore();
        return result;
    }

    public void SaveRestore()
    {
        _savedPos = GameMain.Instance.MainCamera.GetTargetPos();
        _savedAngle = GameMain.Instance.MainCamera.GetAroundAngle();
        _savedDistance = GameMain.Instance.MainCamera.GetDistance();
        px = GameMain.Instance.MainCamera.transform.position.x;
        py = GameMain.Instance.MainCamera.transform.position.y;
        pz = GameMain.Instance.MainCamera.transform.position.z;
        rx = GameMain.Instance.MainCamera.transform.rotation.x;
        ry = GameMain.Instance.MainCamera.transform.rotation.y;
        rz = GameMain.Instance.MainCamera.transform.rotation.z;
        rw = GameMain.Instance.MainCamera.transform.rotation.w;
        fov = GameMain.Instance.MainCamera.camera.fieldOfView;
        bg = GameMain.Instance.BgMgr.GetBGName();
        GameMain.Instance.BgMgr.DeleteBg();
        isStock = true;
    }

    public void Restore()
    {
        if (isStock)
        {
            isStock = false;
            GameMain.Instance.BgMgr.ChangeBg(bg);
            GameMain.Instance.MainCamera.transform.position.Set(px, py, pz);
            GameMain.Instance.MainCamera.transform.rotation.Set(rx, ry, rz, rw);
            GameMain.Instance.MainCamera.camera.fieldOfView = fov;
            GameMain.Instance.MainCamera.SetTargetPos(_savedPos);
            GameMain.Instance.MainCamera.SetAroundAngle(_savedAngle);
            GameMain.Instance.MainCamera.SetDistance(_savedDistance);
        }
    }

    public Texture2D RenderThum(Camera f_cam, RenderTexture f_rtMain, RenderTexture f_rtSub,
        int width, int height)
    {
        var targetTexture = f_cam.targetTexture;
        var enabled = f_cam.enabled;
        f_cam.targetTexture = f_rtMain;
        f_cam.enabled = true;
        f_cam.Render();
        f_cam.enabled = false;
        var texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
        var active = RenderTexture.active;
        RenderTexture.active = f_rtSub;
        GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
        GL.PushMatrix();
        GL.LoadPixelMatrix(0f, width, height, 0f);
        Graphics.DrawTexture(new Rect(0f, 0f, width, height), f_rtMain);
        GL.PopMatrix();
        texture2D.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = active;
        f_cam.targetTexture = targetTexture;
        f_cam.enabled = enabled;
        return texture2D;
    }

    public CharacterMgr.Preset PresetSave(Maid f_maid, CharacterMgr.PresetType f_type)
    {
        var preset = new CharacterMgr.Preset();
        var texture2D = ThumShot(GameMain.Instance.MainCamera.camera, f_maid);
        var memoryStream = new MemoryStream();
        var binaryWriter = new BinaryWriter(memoryStream);
        binaryWriter.Write("CM3D2_PRESET");
        binaryWriter.Write(1160);
        binaryWriter.Write((int)f_type);
        if (texture2D != null)
        {
            var array = texture2D.EncodeToPNG();
            binaryWriter.Write(array.Length);
            binaryWriter.Write(array);
        }
        else
        {
            binaryWriter.Write(0);
        }

        f_maid.SerializeProp(binaryWriter);
        f_maid.SerializeMultiColor(binaryWriter);
        f_maid.SerializeBody(binaryWriter);
        var text = string.Concat("pre_", f_maid.status.lastName, f_maid.status.firstName, "_",
            DateTime.Now.ToString("yyyyMMddHHmmss"));
        text = UTY.FileNameEscape(text);
        text += ".preset";
        var text2 = Path.GetFullPath(".\\") + "Preset";
        if (!Directory.Exists(text2)) Directory.CreateDirectory(text2);

        File.WriteAllBytes(text2 + "\\" + text, memoryStream.ToArray());
        memoryStream.Dispose();
        preset.texThum = texture2D;
        preset.strFileName = text;
        preset.ePreType = f_type;
        return preset;
    }

    public string GetNewestFileName(string folderName)
    {
        var files = Directory.GetFiles(folderName, "*.preset", SearchOption.TopDirectoryOnly);
        var path = string.Empty;
        var t = DateTime.MinValue;
        foreach (var text in files)
        {
            var fileInfo = new FileInfo(text);
            if (fileInfo.LastWriteTime > t)
            {
                t = fileInfo.LastWriteTime;
                path = text;
            }
        }

        return Path.GetFileName(path);
    }

    public byte[] LoadMenuInternal(string filename)
    {
        byte[] result;
        try
        {
            using (var afileBase = GameUty.FileOpen(filename))
            {
                if (!afileBase.IsValid()) throw new Exception();

                result = afileBase.ReadAll();
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }

        return result;
    }

    public int GetPriority(string fileName)
    {
        var result = 0;
        using (var memoryStream = new MemoryStream(LoadMenuInternal(fileName), false))
        {
            using (var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
            {
                binaryReader.ReadString();
                binaryReader.ReadInt32();
                binaryReader.ReadString();
                binaryReader.ReadString();
                binaryReader.ReadString();
                binaryReader.ReadString();
                binaryReader.ReadInt32();
                for (;;)
                {
                    int num = binaryReader.ReadByte();
                    if (num == 0) break;

                    var a = binaryReader.ReadString();
                    var array = new string[num - 1];
                    for (var i = 0; i < num - 1; i++) array[i] = binaryReader.ReadString();

                    if (a == "priority") result = int.Parse(array[0]);
                }
            }
        }

        return result;
    }
}