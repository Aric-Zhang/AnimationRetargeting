using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ZXTSkeleton))]
public class ZXTSkeletonEditor : Editor
{
    bool unfoldSkeletons;
    bool unfoldRetargetingBoneSettings;
    bool viewRefPose;

    Vector3[] cachedLocalPosition;
    Quaternion[] cachedLocalRotation;
    Vector3[] cachedLocalScale;


    private void OnDestroy()
    {
        if (viewRefPose) {
            HideRefPose();
        }
    }

    public override void OnInspectorGUI()
    {
        ZXTSkeleton skeleton = target as ZXTSkeleton;
        serializedObject.Update();
        unfoldRetargetingBoneSettings = EditorGUILayout.Foldout(unfoldRetargetingBoneSettings, new GUIContent("Retargeting"),true);

        if (unfoldRetargetingBoneSettings)
        {
            EditorGUILayout.BeginVertical("Helpbox");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("AutoMap"))
            {
                AutoMap();
            }
            if (GUILayout.Button("Clear"))
            {
                Clear();
            }
            GUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("retargetingProfile"), new GUIContent());
            if (EditorGUI.EndChangeCheck())
            {
                InitRetargetings();
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            //不加的话左边的箭头会冒出框
            EditorGUILayout.Space(7,false);

            SerializedProperty s_p_retargetings = serializedObject.FindProperty("retargetings");
            EditorGUILayout.PropertyField(s_p_retargetings, new GUIContent("Retargeting Bone Settings"),new GUILayoutOption[] { GUILayout.ExpandWidth(true)});
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                RetargetingProfile profile = serializedObject.FindProperty("retargetingProfile").objectReferenceValue as RetargetingProfile;
                SerializedProperty retargetings = serializedObject.FindProperty("retargetings");
                if (profile)
                {
                    int profile_bone_count = profile.boneNames.Length;
                    if (retargetings.arraySize == profile_bone_count)
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Manage Ref Pose"));
            if (GUILayout.Button(new GUIContent("Modify Pose"))) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Reset Pose"), false, ResetRefPose);
                menu.AddItem(new GUIContent("Use Current Pose"), false, UseCurrentPoseAsRefPose);
                menu.ShowAsContext();
            }
            if (viewRefPose)
            {
                if (GUILayout.Button("Hide Pose"))
                {
                    HideRefPose();
                    viewRefPose = false;
                }
            }
            else {
                if (GUILayout.Button("Show Pose")) {
                    ShowRefPose();
                    viewRefPose = true;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical(); 
        }
    }

    void InitRetargetings() {
        ZXTSkeleton skeleton = target as ZXTSkeleton;
        RetargetingProfile profile = serializedObject.FindProperty("retargetingProfile").objectReferenceValue as RetargetingProfile;
        SerializedProperty retargetings = serializedObject.FindProperty("retargetings");
        if (profile)
        {
            int profile_bone_count = profile.boneNames.Length;
            retargetings.arraySize = profile_bone_count;
            for (int i = 0; i < profile_bone_count; i++)
            {
                SerializedProperty s_p_retarget_bone_setting = retargetings.GetArrayElementAtIndex(i);
                SerializedProperty s_p_profile_bone_name = s_p_retarget_bone_setting.FindPropertyRelative("profileBoneName");
                s_p_profile_bone_name.stringValue = profile.boneNames[i];
                SerializedProperty s_p_skeleton = s_p_retarget_bone_setting.FindPropertyRelative("zxtSkeleton");
                s_p_skeleton.objectReferenceValue = skeleton;
                SerializedProperty s_p_index_in_skeleton = s_p_retarget_bone_setting.FindPropertyRelative("boneIndexInSkeleton");
                s_p_index_in_skeleton.intValue = -1;
            }
        }
        else
        {
            retargetings.arraySize = 0;
        }
        serializedObject.ApplyModifiedProperties();
    }

    void AutoMap() {
        ZXTSkeleton skeleton = target as ZXTSkeleton;
        if (skeleton.retargetingProfile)
        {
            List<Transform> bones = new List<Transform>(skeleton.Bones);
            SerializedProperty retargetings = serializedObject.FindProperty("retargetings");
            for (int i = 0; i < retargetings.arraySize; i++)
            {
                SerializedProperty s_p_retarget_bone_setting = retargetings.GetArrayElementAtIndex(i);
                SerializedProperty s_p_profile_bone_name = s_p_retarget_bone_setting.FindPropertyRelative("profileBoneName");
                for (int j = 0; j < bones.Count; j++)
                {
                    Transform bone_to_match = bones[j];
                    //不知道怎么匹配，就相等吧
                    if (s_p_profile_bone_name.stringValue.ToLower() == bone_to_match.name.ToLower())
                    {
                        SerializedProperty s_p_bone_index = s_p_retarget_bone_setting.FindPropertyRelative("boneIndexInSkeleton");
                        s_p_bone_index.intValue = j;
                        break;
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

    void Clear() {
        SerializedProperty retargetings = serializedObject.FindProperty("retargetings");
        for (int i = 0; i < retargetings.arraySize; i++)
        {
            SerializedProperty s_p_retarget_bone_setting = retargetings.GetArrayElementAtIndex(i);
            SerializedProperty s_p_bone_index = s_p_retarget_bone_setting.FindPropertyRelative("boneIndexInSkeleton");
            s_p_bone_index.intValue = -1;
        }
        serializedObject.ApplyModifiedProperties();
    }

    void ResetRefPose() {

        SerializedProperty retargetings = serializedObject.FindProperty("retargetings");
        SerializedProperty s_p_bones = serializedObject.FindProperty("skeletonBones");
        SerializedProperty s_p_defaultPositions = serializedObject.FindProperty("localPositions");
        SerializedProperty s_p_defaultEulers = serializedObject.FindProperty("localEulers");
        SerializedProperty s_p_defaultScales = serializedObject.FindProperty("localScales");
        SerializedProperty s_p_refLocalPositions = serializedObject.FindProperty("refPoseLocalPositions");
        SerializedProperty s_p_refLocalEulers = serializedObject.FindProperty("refPoseLocalEulers");
        SerializedProperty s_p_refLocalScales = serializedObject.FindProperty("refPoseLocalScales");

        int bone_counts = s_p_bones.arraySize;
        if (bone_counts == 0) return;
        for (int i = 0; i < bone_counts; i++) {
            Vector3 localPosition = s_p_defaultPositions.GetArrayElementAtIndex(i).vector3Value;
            Vector3 localEuler = s_p_defaultEulers.GetArrayElementAtIndex(i).vector3Value;
            Vector3 localScale = s_p_defaultScales.GetArrayElementAtIndex(i).vector3Value;
            s_p_refLocalPositions.GetArrayElementAtIndex(i).vector3Value = localPosition;
            s_p_refLocalEulers.GetArrayElementAtIndex(i).vector3Value = localEuler;
            s_p_refLocalScales.GetArrayElementAtIndex(i).vector3Value = localScale;
        }
        serializedObject.ApplyModifiedProperties();
    }

    void UseCurrentPoseAsRefPose() {
        ZXTSkeleton zxtSkeleton = target as ZXTSkeleton;
        SerializedProperty s_p_bones = serializedObject.FindProperty("skeletonBones");
        SerializedProperty s_p_refLocalPositions = serializedObject.FindProperty("refPoseLocalPositions");
        SerializedProperty s_p_refLocalEulers = serializedObject.FindProperty("refPoseLocalEulers");
        SerializedProperty s_p_refLocalScales = serializedObject.FindProperty("refPoseLocalScales");
        SerializedProperty s_p_refComponentPositions = serializedObject.FindProperty("refComponentPositions");
        SerializedProperty s_p_refComponentEulers = serializedObject.FindProperty("refComponentEulers");
        int bone_counts = s_p_bones.arraySize;
        if (bone_counts == 0) return;
        for (int i = 0; i < bone_counts; i++) {
            Transform bone = s_p_bones.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
            s_p_refLocalPositions.GetArrayElementAtIndex(i).vector3Value = bone.localPosition;
            s_p_refLocalEulers.GetArrayElementAtIndex(i).vector3Value = bone.localRotation.eulerAngles;
            s_p_refLocalScales.GetArrayElementAtIndex(i).vector3Value = bone.localScale;
            s_p_refComponentPositions.GetArrayElementAtIndex(i).vector3Value = zxtSkeleton.GetComponentSpacePosition(bone.position);
            s_p_refComponentEulers.GetArrayElementAtIndex(i).vector3Value = zxtSkeleton.GetComponentSpaceRotation(bone.rotation).eulerAngles;
        }
        serializedObject.ApplyModifiedProperties();
    }

    void HideRefPose() {
        SerializedProperty retargetings = serializedObject.FindProperty("retargetings");
        SerializedProperty s_p_bones = serializedObject.FindProperty("skeletonBones");
        int bone_counts = s_p_bones.arraySize;
        if (bone_counts == 0) return;
        for (int i = 0; i < bone_counts; i++)
        {
            Transform bone = s_p_bones.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
            bone.localPosition = cachedLocalPosition[i];
            bone.localRotation = cachedLocalRotation[i];
            bone.localScale = cachedLocalScale[i];
        }
        viewRefPose = false;
    }

    void ShowRefPose() {
        SerializedProperty s_p_bones = serializedObject.FindProperty("skeletonBones");
        SerializedProperty s_p_refLocalPositions = serializedObject.FindProperty("refPoseLocalPositions");
        SerializedProperty s_p_refLocalEulers = serializedObject.FindProperty("refPoseLocalEulers");
        SerializedProperty s_p_refLocalScales = serializedObject.FindProperty("refPoseLocalScales");
        int bone_counts = s_p_bones.arraySize;
        if (bone_counts == 0) return;
        //保存view ref pose 之前的姿势
        if (!viewRefPose)
        {
            cachedLocalPosition = new Vector3[bone_counts];
            cachedLocalRotation = new Quaternion[bone_counts];
            cachedLocalScale = new Vector3[bone_counts];
            for (int i = 0; i < bone_counts; i++)
            {
                Transform bone = s_p_bones.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                cachedLocalPosition[i] = bone.localPosition;
                cachedLocalRotation[i] = bone.localRotation;
                cachedLocalScale[i] = bone.localScale;
            }
        }
        for (int i = 0; i < bone_counts; i++)
        {
            Transform bone = s_p_bones.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
            bone.localPosition = s_p_refLocalPositions.GetArrayElementAtIndex(i).vector3Value;
            bone.localRotation = Quaternion.Euler(s_p_refLocalEulers.GetArrayElementAtIndex(i).vector3Value);
            bone.localScale = s_p_refLocalScales.GetArrayElementAtIndex(i).vector3Value;
        }
        viewRefPose = true;
    }
}


[CustomPropertyDrawer(typeof(RetargetingBoneSetting))]
public class RetargetingBoneSettingEditor : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        ZXTSkeleton skeleton = property.FindPropertyRelative("zxtSkeleton").objectReferenceValue as ZXTSkeleton;

        EditorGUI.BeginProperty(position, label, property);
        float op_button_width = 20;
        float prop_width = (position.width - op_button_width) / 3;
        float prop_height = position.height;
        EditorGUI.LabelField(new Rect(position.x, position.y, prop_width, prop_height), label);

        //Concat 生成骨骼选项
        string[] skeleton_names = skeleton.BoneNames;
        string[] options = new string[skeleton_names.Length + 1];
        options[0] = "null";
        skeleton_names.CopyTo(options, 1);

        int prev_bone_index = property.FindPropertyRelative("boneIndexInSkeleton").intValue;
        int new_bone_index = EditorGUI.Popup(new Rect(position.x + prop_width, position.y, prop_width, prop_height), prev_bone_index + 1, options) - 1;
        if (prev_bone_index != new_bone_index) {
            property.FindPropertyRelative("boneIndexInSkeleton").intValue = new_bone_index;
        }

        EditorGUI.PropertyField(new Rect(position.x + prop_width * 2, position.y, prop_width, prop_height), property.FindPropertyRelative("type"), new GUIContent());
        if (GUI.Button(new Rect(position.x + prop_width * 3, position.y, op_button_width, prop_height), new GUIContent("..."))) {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Set Child Recursively to Animation"), false, () => SetChildRecursivelyTo(skeleton, new_bone_index, BoneRetargetingType.animation));
            menu.AddItem(new GUIContent("Set Child Recursively to Skeleton"), false, () => SetChildRecursivelyTo(skeleton, new_bone_index, BoneRetargetingType.skeleton));
            menu.AddItem(new GUIContent("Set Child Recursively to Animation Scaled"), false, () => SetChildRecursivelyTo(skeleton, new_bone_index, BoneRetargetingType.animationScaled));
            menu.AddItem(new GUIContent("Set Child Recursively to Animation Relative"), false, () => SetChildRecursivelyTo(skeleton, new_bone_index, BoneRetargetingType.animationRelative));
            menu.AddItem(new GUIContent("Set Child Recursively to Orient And Scale"), false, () => SetChildRecursivelyTo(skeleton, new_bone_index, BoneRetargetingType.orientAndScale));
            menu.ShowAsContext();
        }

        EditorGUI.EndProperty();
    }

    void SetChildRecursivelyTo(ZXTSkeleton skeleton, int boneIndex, BoneRetargetingType type) {
        List<Transform> bones_in_skeleton = new List<Transform>(skeleton.Bones);
        Transform bone = bones_in_skeleton[boneIndex];
        SerializedObject s_skeleton = new SerializedObject(skeleton);
        SerializedProperty s_p_retargetings = s_skeleton.FindProperty("retargetings");
        List<Transform> children_list = new List<Transform>(bone.GetComponentsInChildren<Transform>());

        List<int> indicesInSkeletonToModify = new List<int>();
        foreach (Transform child_bone in children_list) {
            indicesInSkeletonToModify.Add(bones_in_skeleton.IndexOf(child_bone));
        }
        for (int i = 0; i < s_p_retargetings.arraySize; i++) {
            SerializedProperty s_p_retargetingBoneSetting = s_p_retargetings.GetArrayElementAtIndex(i);
            int boneIndexInSkeleton = s_p_retargetingBoneSetting.FindPropertyRelative("boneIndexInSkeleton").intValue;
            if (indicesInSkeletonToModify.Remove(boneIndexInSkeleton)) {
                s_p_retargetingBoneSetting.FindPropertyRelative("type").enumValueIndex = (int)type;
            }
        }
        s_skeleton.ApplyModifiedProperties();
    }
}