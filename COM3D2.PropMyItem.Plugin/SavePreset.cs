﻿using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace COM3D2.PropMyItem.Plugin
{
    // Token: 0x0200000B RID: 11
    public class SavePreset
    {
        // Token: 0x04000058 RID: 88
        private Vector2 _savedAngle;

        // Token: 0x04000059 RID: 89
        private float _savedDistance = 1f;

        // Token: 0x04000057 RID: 87
        private Vector3 _savedPos;

        // Token: 0x04000055 RID: 85
        private string bg = string.Empty;

        // Token: 0x04000054 RID: 84
        private float fov;

        // Token: 0x04000056 RID: 86
        private bool isStock;

        // Token: 0x0400004D RID: 77
        private float px;

        // Token: 0x0400004E RID: 78
        private float py;

        // Token: 0x0400004F RID: 79
        private float pz;

        // Token: 0x04000053 RID: 83
        private float rw;

        // Token: 0x04000050 RID: 80
        private float rx;

        // Token: 0x04000051 RID: 81
        private float ry;

        // Token: 0x04000052 RID: 82
        private float rz;

        // Token: 0x06000048 RID: 72 RVA: 0x00007AEC File Offset: 0x00005CEC
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

        // Token: 0x06000049 RID: 73 RVA: 0x00007BBC File Offset: 0x00005DBC
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

        // Token: 0x0600004A RID: 74 RVA: 0x00007D28 File Offset: 0x00005F28
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

        // Token: 0x0600004B RID: 75 RVA: 0x00007E1C File Offset: 0x0000601C
        public Texture2D RenderThum(Camera f_cam, RenderTexture f_rtMain, RenderTexture f_rtSub, int width, int height)
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

        // Token: 0x0600004C RID: 76 RVA: 0x00007EFC File Offset: 0x000060FC
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

        // Token: 0x0600004D RID: 77 RVA: 0x00008058 File Offset: 0x00006258
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

        // Token: 0x0600004E RID: 78 RVA: 0x000080BC File Offset: 0x000062BC
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

        // Token: 0x0600004F RID: 79 RVA: 0x00008114 File Offset: 0x00006314
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
}