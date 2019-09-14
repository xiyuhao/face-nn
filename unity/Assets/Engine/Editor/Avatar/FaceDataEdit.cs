﻿using CFUtilPoolLib;
using System.IO;
using UnityEditor;
using UnityEngine;
using CFEngine.Editor;

namespace XEditor
{
    [CustomEditor(typeof(FaceData))]
    public class FaceDataEdit : BaseEditor<FaceDataEdit>
    {
        private FaceData faceData;
        private bool[] folder = new bool[5];
        private bool[] bsub;
        private Object[] icons;
        private Object[] paintings;
        private const int max = 64;
        private const int maxv1 = 4;
        private int maxid = 0;
        private string prefix = @"Assets/BundleRes/UI/UISource/ui_faceicon/";
        int swap = 0;

        [MenuItem("Assets/Fashion/FaceData_Create")]
        static void CreateFaceData()
        {
            string path = "Assets/BundleRes/Config/FaceData.asset";
            if (!File.Exists(path))
            {
                FaceData fd = ScriptableObject.CreateInstance<FaceData>();
                CommonAssets.CreateAsset<FaceData>("Assets/BundleRes/Config", "FaceData", ".asset", fd);
            }
        }

        private void OnEnable()
        {
            for (int i = 0; i < folder.Length; i++)
            {
                folder[i] = true;
            }
            faceData = target as FaceData;
            bsub = new bool[max * 5];
            icons = new Object[max * 4];
            paintings = new Object[max * maxv1];
            swap = 0;
        }

        private void OnDisable()
        {
            bsub = null;
            swap = 0;
            for (int i = 0; i < max * 4; i++)
            {
                if (icons[i] != null)
                {
                    icons[i] = null;
                }
            }
            for (int i = 0; i < max * maxv1; i++)
            {
                if (paintings[i] != null)
                {
                    paintings[i] = null;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (faceData != null)
            {
                GUILayout.Label("Face Config Data", EditorStyles.boldLabel);
                GUIFacePart<HeadData>("脸型", 0, ref faceData.headData);
                GUIFacePart<SenseData>("五官", 1, ref faceData.senseData);
                GUIFacePart<PaintData>("妆容", 2, ref faceData.paintData);
                GUIFacePart<ShapeData>("时装", 3, ref faceData.shapeData);

                GUIShapeInfo();
                GUILayout.Space(5);
                if (GUILayout.Button("Save", GUILayout.MaxWidth(100)))
                {
                    faceData.OnSave();
                    CommonAssets.SaveAsset(faceData);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
        
        private void GUIFacePart<T>(string title, int idx, ref T[] t) where T : FaceBaseData, new()
        {
            folder[idx] = EditorGUILayout.Foldout(folder[idx], title, XEditorUtil.folderStyle);
            if (folder[idx])
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                if (GUILayout.Button("Add"))
                {
                    Add<T>(ref t, new T());
                }
                GUILayout.EndHorizontal();
                if (t != null)
                {
                    int del = -1;
                    for (int i = 0; i < t.Length; i++)
                    {
                        int indx = idx * max + i;
                        GUILayout.BeginHorizontal();
                        bsub[indx] = EditorGUILayout.Foldout(bsub[indx], i + "_" + t[i].name);
                        if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                        {
                            del = i;
                        }
                        GUILayout.EndHorizontal();
                        int len = t[i].properities != null ? t[i].properities.Length : 0;
                        for (int k = 0; k < len; k++)
                        {
                            if (t[i].properities[k] > maxid) maxid = t[i].properities[k];
                        }
                        if (bsub[indx])
                        {
                            if (typeof(T) == typeof(SenseData))
                            {
                                SenseData data = (t[i] as SenseData);
                                data.type = (SenseSubType)EditorGUILayout.EnumPopup("sub type", data.type);
                            }
                            else if (typeof(T) == typeof(PaintData))
                            {
                                PaintData data = (t[i] as PaintData);
                                data.type = (PaintSubType)EditorGUILayout.EnumPopup("sub type:", data.type);
                            }
                            else if (typeof(T) == typeof(ShapeData))
                            {
                                ShapeData data = (t[i] as ShapeData);
                                data.suitID = (uint)EditorGUILayout.IntField("suitid:", (int)data.suitID);
                                var row = XFashionLibrary.GetFashionsInfo(data.suitID);
                                if (row != null) data.name = row.name;
                                if (data.suitID == 0) EditorGUILayout.HelpBox("suit id can't be 0", MessageType.Error);
                            }
                            GUIFaceBase(t[i], indx, i);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("insert",GUILayout.Width(100), GUILayout.Height(14)))
                            {
                                if (swap < t.Length && i != swap)
                                {
                                    int max = Mathf.Max(swap, i);
                                    int min = Mathf.Min(swap, i);
                                    bool forward = max == swap;
                                    if (forward)
                                    {
                                        T temp = t[min];
                                        for (int j = min; j < max; j++)
                                        {
                                            t[j] = t[j + 1];
                                        }
                                        t[max] = temp;
                                    }
                                    else
                                    {
                                        T temp = t[max];
                                        for (int j = max - 1; j >= min; j--)
                                        {
                                            t[j + 1] = t[j];
                                        }
                                        t[min] = temp;
                                    }
                                }
                                swap = 0;
                            }
                            swap = EditorGUILayout.IntField(swap);
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    if (del >= 0) t = Remv<T>(t, del);
                }
            }
            EditorGUILayout.Space();
        }


        private void GUIFaceBase(FaceBaseData data, int indx, int ix)
        {
            data.name = EditorGUILayout.TextField("name", data.name);
            if (string.IsNullOrEmpty(data.name))
            {
                EditorGUILayout.HelpBox("name can not be null", MessageType.Warning);
            }
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("icon");
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            if (!string.IsNullOrEmpty(data.icon))
                icons[indx] = AssetDatabase.LoadAssetAtPath<Texture>(prefix + data.icon + ".png");
            icons[indx] = EditorGUILayout.ObjectField(icons[indx], typeof(Texture), true, GUILayout.Width(64), GUILayout.Height(64));
            if (icons[indx] != null)
            {
                data.icon = icons[indx].name;
                GUILayout.Label((icons[indx]).name);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if (!(data is ShapeData))
            {
                GUILayout.BeginHorizontal();
                bool v2add = false;
                if (data.v2Type == FaceV2Type.None) v2add = GUILayout.Button("AddV2Item", XEditorUtil.boldButtonStyle);
                if (GUILayout.Button("AddV1Item", XEditorUtil.boldButtonStyle))
                {
                    if (data.properities == null || data.properities.Length < maxv1)
                    {
                        Add<int>(ref data.properities, data is PaintData ? (maxid + 1) : 1);
                        Add<FaceValueType>(ref data.values, FaceValueType.BigSmall);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Warn", "You add item too much!", "OK");
                    }
                }
                GUILayout.EndHorizontal();
                if (v2add) data.v2Type = FaceV2Type.Position;
                if (data.v2Type != FaceV2Type.None)
                {
                    data.v2Type = (FaceV2Type)EditorGUILayout.EnumPopup("v2Type", data.v2Type);
                    data.v2ID = EditorGUILayout.IntField("v2ID1: ", data.v2ID);
                    data.v2ID2 = EditorGUILayout.IntField("v2ID2: ", data.v2ID2);
                }
                if (data.values != null)
                {
                    for (int i = data.values.Length - 1; i >= 0; i--)
                    {
                        if (data.values[i] == FaceValueType.None)
                        {
                            data.properities = Remv<int>(data.properities, i);
                            data.values = Remv<FaceValueType>(data.values, i);
                        }
                    }
                    if (data.properities.Length > data.values.Length)
                    {
                        for (int i = data.values.Length; i < data.properities.Length; i++)
                        {
                            data.properities = Remv<int>(data.properities, i);
                        }
                    }
                    for (int i = 0; i < data.properities.Length; i++)
                    {
                        GUIItem(ref data.values[i], ref data.properities[i]);
                    }
                    if (data is PaintData) GUIPaintItem(data, ix);
                }
            }
            data.camPos = EditorGUILayout.Vector3Field("camera pos", data.camPos);
            data.dummyRot = EditorGUILayout.FloatField("player rot", data.dummyRot);
            EditorGUILayout.Space();
        }


        private void GUIItem(ref FaceValueType type, ref int id)
        {
            GUILayout.BeginHorizontal();
            type = (FaceValueType)EditorGUILayout.EnumPopup(type, GUILayout.MaxWidth(80));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
            {
                type = FaceValueType.None;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            id = EditorGUILayout.IntField("  id", id);
            if (id > maxid) maxid = id;
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
        }

        private void GUIPaintItem(FaceBaseData data, int ix)
        {
            PaintData pdata = data as PaintData;
            EditorGUILayout.Space();
            if (pdata.offset == null || pdata.offset.Length != 4) pdata.offset = new Vector2[4];
            pdata.offset[0] = EditorGUILayout.Vector2Field("offset_tall  ", pdata.offset[0]);
            pdata.offset[1] = EditorGUILayout.Vector2Field("offset_giant ", pdata.offset[1]);
            pdata.offset[2] = EditorGUILayout.Vector2Field("offset_male  ", pdata.offset[2]);
            pdata.offset[3] = EditorGUILayout.Vector2Field("offset_female", pdata.offset[3]);
            if (pdata.offset[0] == Vector2.zero) pdata.offset[0] = 256 * Vector2.one;
            if (pdata.offset[1] == Vector2.zero) pdata.offset[1] = 256 * Vector2.one;
            if (pdata.offset[2] == Vector2.zero) pdata.offset[2] = 256 * Vector2.one;
            if (pdata.offset[3] == Vector2.zero) pdata.offset[3] = 256 * Vector2.one;
            GUILayout.BeginHorizontal();
            GUILayout.Label("texture");

            int pindx = ix;
            if (!string.IsNullOrEmpty(pdata.texture) && paintings[pindx] == null)
            {
                paintings[pindx] = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BundleRes/Knead/" + pdata.texture + "_female.tga");
            }
            paintings[pindx] = EditorGUILayout.ObjectField(paintings[pindx], typeof(Texture2D), true);
            if (paintings[pindx] != null)
            {
                string str = paintings[pindx].name;
                pdata.texture = str.Remove(str.LastIndexOf('_'));
                GUILayout.Label(pdata.texture);
            }
            else
            {
                pdata.texture = "";
            }
            GUILayout.EndHorizontal();
        }


        private void GUIShapeInfo()
        {
            folder[4] = EditorGUILayout.Foldout(folder[4], "体型", XEditorUtil.folderStyle);
            if (folder[4])
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                if (GUILayout.Button("Add"))
                {
                    Add<ShapeInfo>(ref faceData.shapeInfo, new ShapeInfo() { shape = RoleShape.FEMALE });
                }
                GUILayout.EndHorizontal();
                if (faceData.shapeInfo != null)
                {
                    for (int i = 0; i < faceData.shapeInfo.Length; i++)
                    {
                        int indx = 4 * max + i;
                        bsub[indx] = EditorGUILayout.Foldout(bsub[indx], i + "_" + faceData.shapeInfo[i].shape);
                        if (bsub[indx])
                        {
                            faceData.shapeInfo[i].shape = (RoleShape)EditorGUILayout.EnumPopup("shape", faceData.shapeInfo[i].shape);
                            faceData.shapeInfo[i].vinfo = EditorGUILayout.Vector4Field("info", faceData.shapeInfo[i].vinfo);
                        }
                    }
                }
            }
        }

        private void Add<T>(ref T[] arr, T item)
        {
            if (arr != null)
            {
                T[] narr = new T[arr.Length + 1];
                for (int i = 0; i < arr.Length; i++)
                {
                    narr[i] = arr[i];
                }
                narr[arr.Length] = item;
                arr = narr;
            }
            else
            {
                arr = new T[1];
                arr[0] = item;
            }
        }

        private T[] Remv<T>(T[] arr, int idx)
        {
            if (arr.Length > idx)
            {
                T[] narr = new T[arr.Length - 1];
                for (int i = 0; i < idx; i++)
                {
                    narr[i] = arr[i];
                }
                for (int i = idx + 1; i < arr.Length; i++)
                {
                    narr[i - 1] = arr[i];
                }
                return narr;
            }
            else
            {
                return arr;
            }
        }

    }
}