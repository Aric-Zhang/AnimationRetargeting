#define USE_IMPORTOR
#define USE_NO_FBX

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;



namespace ZxtTool
{
#if(USE_IMPORTOR)
    public class CharacterFbxImporter : AssetPostprocessor
    {
        void OnpreprocessModel() {
            ModelImporter modelImporter = assetImporter as ModelImporter;
            modelImporter.clipAnimations = modelImporter.defaultClipAnimations;

            ModelImporterClipAnimation[] clip_animations = modelImporter.clipAnimations;
            for (int i = 0; i < clip_animations.Length; i++) { 
                //to-do:目前什么都没做，但肯定要做点什么，比如设置根运动类型之类的
            }

            modelImporter.animationType =  clip_animations.Length > 0 ? ModelImporterAnimationType.Generic: ModelImporterAnimationType.None;
            modelImporter.SaveAndReimport();
        }

        void OnPostprocessModel(GameObject g) 
        {
            ModelImporter modelImporter = assetImporter as ModelImporter;
            if (modelImporter == null) return;

            string dir = Path.GetDirectoryName(assetPath);



#if (USE_NO_FBX)

            if (g) {
                //得到骨骼(?)


                SkinnedMeshRenderer[] smrs = g.GetComponentsInChildren<SkinnedMeshRenderer>();
                MeshRenderer[] mrs = g.GetComponentsInChildren<MeshRenderer>();

                if (smrs.Length > 0)
                {
                    Mesh[] meshes = new Mesh[smrs.Length];
                    for (int i = 0; i < smrs.Length; i++)
                    {
                        GameObject skeleton = SkeletonTool.FormSkeleton(g);
                        GameObject skinObj = new GameObject();
                        SkinnedMeshRenderer smr = skinObj.AddComponent<SkinnedMeshRenderer>();
                        //to-do:在资源极端不规范的情况下，有可能出现local scale和lossy scale相差很大的情况，比如Root存在非等比缩放或者蒙皮在Root下边的层级，这种情况没考虑到
                        skinObj.transform.parent = skeleton.transform;

                        meshes[i] = smrs[i].sharedMesh;

                        string mesh_path = Path.Combine(dir, meshes[i].name + ".asset");
                        AssetDatabase.CreateAsset(meshes[i], mesh_path);

                        Material[] mats = smrs[i].sharedMaterials;
                        string[] mat_paths = new string[mats.Length];

                        for (int j = 0; j < mats.Length; j++)
                        {
                            mat_paths[j] = Path.Combine(dir, mats[j].name + ".mat");
                            string full_path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, mat_paths[j]);
                            if (File.Exists(full_path))
                            {
                                //不替换已经存在的同名资源
                                continue;
                            }
                            AssetDatabase.CreateAsset(mats[j], mat_paths[j]);
                        }

                        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(mesh_path);
                        //清空mat，虽然也许没必要
                        mats = new Material[mats.Length];
                        for (int j = 0; j < mats.Length; j++)
                        {
                            mats[j] = AssetDatabase.LoadAssetAtPath<Material>(mat_paths[j]);
                        }

                        GameObject skinObjOriginal = smrs[i].gameObject;
                        skinObj.transform.position = skinObjOriginal.transform.position;
                        skinObj.transform.rotation = skinObjOriginal.transform.rotation;
                        skinObj.transform.localScale = skinObjOriginal.transform.localScale;
                        skinObj.name = skinObjOriginal.name;

                        smr.sharedMesh = mesh;
                        smr.bones = SkeletonTool.MatchBonesByName(smrs[i].bones, skeleton);
                        smr.rootBone = SkeletonTool.MatchBoneByName(smrs[i].rootBone, skeleton);
                        smr.sharedMaterials = mats;

                        string prefab_path = Path.Combine(dir, meshes[i].name + ".prefab");

                        //prefab_path = AssetDatabase.GenerateUniqueAssetPath(prefab_path);
                        PrefabUtility.SaveAsPrefabAssetAndConnect(skeleton, prefab_path, InteractionMode.AutomatedAction);
                        Object.DestroyImmediate(skeleton);
                    }

                    //AssetDatabase.DeleteAsset(assetPath);
                    //meta已经被删除，但还是会弹一个“存在meta file”的警告
                    //AssetDatabase.DeleteAsset(Path.Combine(assetPath, ".meta"));
                    AssetDatabase.Refresh();
                }
                else if (mrs.Length > 0) {
                    Mesh[] meshes = new Mesh[mrs.Length];
                    string[] mesh_paths = new string[mrs.Length];
                    for (int i = 0; i < mrs.Length; i++) {
                        MeshFilter mesh_filter = mrs[i].GetComponent<MeshFilter>();
                        Mesh mesh = mesh_filter.sharedMesh;
                        mesh_paths[i] = Path.Combine(dir, mesh.name + ".asset");
                        Material[] mats = mrs[i].sharedMaterials;
                        string[] mat_paths = new string[mats.Length];
                        for (int j = 0; j < mats.Length; j++) {
                            string name = mats[j].name;
                            mat_paths[j] = Path.Combine(dir, name + ".mat");
                            string full_path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, mat_paths[j]);
                            if (!File.Exists(full_path))
                            {
                                //不替换已经存在的同名资源
                                AssetDatabase.CreateAsset(mats[j], mat_paths[j]);
                            }
                        }
                        for (int j = 0; j < mats.Length; j++) {
                            mats[j] = AssetDatabase.LoadAssetAtPath<Material>(mat_paths[j]);
                        }
                        AssetDatabase.CreateAsset(mesh, mesh_paths[i]);
                        meshes[i] = AssetDatabase.LoadAssetAtPath<Mesh>(mesh_paths[i]);
                        mesh_filter.sharedMesh = meshes[i];
                        mrs[i].sharedMaterials = mats;
                    }
                    string prefab_path = Path.Combine(dir, g.name + ".prefab");
                    PrefabUtility.SaveAsPrefabAssetAndConnect(g, prefab_path, InteractionMode.AutomatedAction);
                    //AssetDatabase.DeleteAsset(assetPath);
                    //meta已经被删除，但还是会弹一个“存在meta file”的警告
                    //AssetDatabase.DeleteAsset(Path.Combine(assetPath, ".meta"));
                    AssetDatabase.Refresh();
                }
            }
#endif
        }

        
        public static void OnPostprocessAllAssets(string[] importedAsset, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {

            bool processed_flag = false;

            foreach (string asset_path in importedAsset) {
                string dir = Path.GetDirectoryName(asset_path);
                Object main_asset = AssetDatabase.LoadMainAssetAtPath(asset_path);
                if (main_asset.GetType() == typeof(GameObject) && Path.GetExtension(asset_path).ToLower() == ".fbx") {
                    Object[] assets = AssetDatabase.LoadAllAssetsAtPath(asset_path);
                    List<Object> animation_clip_list = new List<Object>();
                    foreach (Object asset in assets)
                    {
                        if (asset.GetType() == typeof(AnimationClip))
                        {
                            //这种preview的clip完全没法拷贝序列化
                            if (!asset.name.StartsWith("__preview__")){
                                animation_clip_list.Add(asset);
                            }
                        }
                    }
                    foreach (AnimationClip animation_clip in animation_clip_list)
                    {
                        Object new_animation_clip = new AnimationClip();

                        //Debug.LogFormat("{0},{1}", animation_clip.GetType(), new_animation_clip.GetType());
                        EditorUtility.CopySerialized(animation_clip, new_animation_clip);
                        //感谢虚幻引擎官方提供了测试模型
                        if (new_animation_clip.name == "Unreal Take" || new_animation_clip.name == "")
                        {
                            new_animation_clip.name = Path.GetFileNameWithoutExtension(asset_path);
                        }
                        string animation_path = Path.Combine(dir, new_animation_clip.name + ".anim");
                        AssetDatabase.CreateAsset(new_animation_clip, animation_path);
                    }
                    AssetDatabase.DeleteAsset(asset_path);
                    processed_flag = true;
                }
            }
            if (processed_flag) {
                AssetDatabase.Refresh();
            }
        }
    }
#endif
        public static class SkeletonTool
    {
        public static GameObject FormSkeleton(GameObject g) {
            //剔除所有的SkinnedMeshRenderer，剩下的看作都是骨骼
            if (g.GetComponent<SkinnedMeshRenderer>() == null)
            {
                GameObject skeleton_bone = new GameObject(g.name);
                skeleton_bone.transform.position = g.transform.position;
                skeleton_bone.transform.rotation = g.transform.rotation;
                skeleton_bone.transform.localScale = g.transform.localScale;

                for (int i = 0; i < g.transform.childCount; i++) {
                    GameObject childBone = FormSkeleton(g.transform.GetChild(i).gameObject);
                    if (childBone) {
                        childBone.transform.parent = skeleton_bone.transform;
                    }
                }
                return skeleton_bone;
            }
            return null;
        }

        public static Transform[] MatchBonesByName(Transform[] sourceBones, GameObject targetSkeleton) {
            Transform[] match = new Transform[sourceBones.Length];
            Transform[] target_transforms = targetSkeleton.GetComponentsInChildren<Transform>();
            for (int i = 0; i < sourceBones.Length; i++) {
                for (int j = 0; j < target_transforms.Length; j++) {
                    if(sourceBones[i].name == target_transforms[j].name){
                        match[i] = target_transforms[j];
                        break;
                    }
                }
            }
            return match;
        }

        public static Transform MatchBoneByName(Transform sourceBone, GameObject targetSkeleton) {
            Transform[] target_transforms = targetSkeleton.GetComponentsInChildren<Transform>();
            foreach (Transform target_bone in target_transforms) {
                if (target_bone.name == sourceBone.name) {
                    return target_bone;
                }
            }
            return null;
        }
    }
}

