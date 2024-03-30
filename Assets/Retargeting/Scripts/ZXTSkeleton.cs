using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZXTSkeleton : MonoBehaviour
{
    [SerializeField]
    Transform[] skeletonBones;
    [SerializeField]

    Vector3[] localPositions;
    [SerializeField]
    Vector3[] refComponentPositions;

    [SerializeField]
    Vector3[] localEulers;
    [SerializeField]
    Vector3[] refComponentEulers;

    [SerializeField]
    Vector3[] localScales;
    //全局的scale不知道怎么算……

    [SerializeField]
    string[] boneNames;
    [SerializeField]
    Vector3[] refPoseLocalPositions;
    [SerializeField]
    Vector3[] refPoseLocalEulers;
    [SerializeField]
    Vector3[] refPoseLocalScales;

    private void Reset()
    {
        InitSkeleton();
    }

    public Transform[] Bones
    {
        get {
            if (skeletonBones.Length == 0) {
                InitSkeleton();
            }
            Transform[] copyed_bones = new Transform[skeletonBones.Length];
            skeletonBones.CopyTo(copyed_bones, 0);
            return copyed_bones;
        }
    }

    public string[] BoneNames {
        get
        {
            if (skeletonBones.Length == 0)
            {
                InitSkeleton();
            }
            string[] copyed_names = new string[skeletonBones.Length];
            boneNames.CopyTo(copyed_names, 0);
            return copyed_names;
        }
    }

    /// <summary>
    /// 得到ref pose所有骨骼的localposition
    /// </summary>
    public Vector3[] RefLocalPositions{
        get {
            Vector3[] copyed_ref_positions = new Vector3[skeletonBones.Length];
            refPoseLocalPositions.CopyTo(copyed_ref_positions, 0);
            return copyed_ref_positions;
        }
    }

    /// <summary>
    /// 得到ref pose骨骼的localrotation的euler
    /// </summary>
    public Vector3[] RefLocalEulers
    {
        get {
            Vector3[] copyed_ref_eulers = new Vector3[skeletonBones.Length];
            refPoseLocalEulers.CopyTo(copyed_ref_eulers, 0);
            return copyed_ref_eulers;
        }
    }

    /// <summary>
    /// 得到ref pose所有骨骼的localscale
    /// </summary>
    public Vector3[] RefLocalScales
    {
        get {
            Vector3[] copyed_ref_scales = new Vector3[skeletonBones.Length];
            refPoseLocalScales.CopyTo(copyed_ref_scales, 0);
            return copyed_ref_scales;
        }
    }

    public Vector3[] RefComponentPositions
    {
        get {
            Vector3[] copyed_ref_positions = new Vector3[skeletonBones.Length];
            refComponentPositions.CopyTo(copyed_ref_positions, 0);
            return refComponentPositions;
        }
    }

    public Vector3[] RefComponentEulers
    {
        get {
            Vector3[] copyed_ref_eulers = new Vector3[skeletonBones.Length];
            refComponentEulers.CopyTo(copyed_ref_eulers, 0);
            return copyed_ref_eulers;
        }
    }

    public RetargetingProfile retargetingProfile;

    [SerializeField]
    Transform matchBone;
    [SerializeField]
    RetargetingBoneSetting[] retargetings;

    public void SetComponentSpacePosition(Transform t, Vector3 position) {
        Vector3 worldPosition = transform.TransformPoint(position);
        t.position = worldPosition;
    }

    public void SetComponentSpaceRotation(Transform t, Quaternion rotation) {
        Quaternion worldRotation = transform.rotation * rotation;
        t.rotation = worldRotation;
    }

    public Vector3 GetComponentSpacePosition(Vector3 worldPosition) {
        Vector3 componentPosition = transform.InverseTransformPoint(worldPosition);
        return componentPosition;
    }

    public Quaternion GetComponentSpaceRotation(Quaternion worldRotation) {
        Quaternion componentRotation = Quaternion.Inverse(transform.rotation) * worldRotation;
        return componentRotation;
    }

    public int[] BoneIndicesInProfileBoneOrder
    {
        get {
            int[] indices = new int[retargetings.Length];
            for (int i = 0; i < retargetings.Length; i++) {
                indices[i] = retargetings[i].boneIndexInSkeleton;
            }
            return indices;
        }
    }

    public BoneRetargetingType[] BoneRetargetingTypeInProfileOrder
    {
        get {
            BoneRetargetingType[] types = new BoneRetargetingType[retargetings.Length];
            for (int i = 0; i < retargetings.Length; i++) {
                types[i] = retargetings[i].type;
            }
            return types;
        }
    }

    //scale不处理了,反正从component层面处理了也不对


    [ContextMenu("Init Skeleton")]
    void InitSkeleton()
    {
        Transform[] bones = GetComponentsInChildren<Transform>();
        skeletonBones = new Transform[bones.Length];
        boneNames = new string[bones.Length];

        localPositions = new Vector3[bones.Length];
        refComponentPositions = new Vector3[bones.Length];
        localEulers = new Vector3[bones.Length];
        refComponentEulers = new Vector3[bones.Length];
        localScales = new Vector3[bones.Length];

        refPoseLocalPositions = new Vector3[bones.Length];
        refPoseLocalEulers = new Vector3[bones.Length];
        refPoseLocalScales = new Vector3[bones.Length];

        for (int i = 0; i < bones.Length; i++)
        {
            skeletonBones[i] = bones[i];
            boneNames[i] = bones[i].name;
            localPositions[i] = bones[i].localPosition;
            refComponentPositions[i] = GetComponentSpacePosition(bones[i].position);
            localEulers[i] = bones[i].localRotation.eulerAngles;
            refComponentEulers[i] = GetComponentSpaceRotation(bones[i].rotation).eulerAngles;
            localScales[i] = bones[i].localScale;
        }
        localPositions.CopyTo(refPoseLocalPositions, 0);
        localEulers.CopyTo(refPoseLocalEulers, 0);
        localScales.CopyTo(refPoseLocalScales, 0);
    }
}

public enum BoneRetargetingType { 
    animation,
    skeleton,
    animationScaled,
    animationRelative,
    orientAndScale
}

[System.Serializable]
public class RetargetingBoneSetting{
    [HideInInspector]
    public string profileBoneName;

    /// <summary>
    /// 存储一个到父skeleton的引用
    /// </summary>
    [SerializeField]
    [HideInInspector]
    ZXTSkeleton zxtSkeleton;

    [HideInInspector]
    public int boneIndexInSkeleton = -1;  //-1表示null
    [HideInInspector]
    public BoneRetargetingType type = BoneRetargetingType.animation;

}
