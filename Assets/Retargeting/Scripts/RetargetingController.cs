#define STILL_IN_TEST

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RetargetingController : MonoBehaviour
{
#if(STILL_IN_TEST)
    public AnimationClip testClip;
    [Range(0,1)]
    public float time;
    bool useAnimation;
#endif

    public ZXTSkeleton source;
    public ZXTSkeleton target;

    Transform[] sourceSkeletonBones;
    Transform[] targetSkeletonBones;
    SkeletonBone[] sourceComponentRefPoses;
    SkeletonBone[] targetComponentRefPoses;
    SkeletonBone[] sourceLocalRefPoses;
    SkeletonBone[] targetLocalRefPoses;

    int[] parentIndices;
    bool[] childInProfile;
    List<OrientAndScaleData> orientAndScaleDataList;
    BoneRetargetingType[] types;

    int bone_count = 0;

    [ContextMenu("Init")]
    public void Init()
    {
        if (!CheckCanRetarget()) return;
        RetargetingProfile profile = source.retargetingProfile;

        //获取source全部信息，骨骼顺序
        Transform[] sourceBoneInSkeletonOrder = source.Bones;

        Vector3[] sourceRefPositionsInSkeletonOrder = source.RefComponentPositions;
        Vector3[] sourceRefEulersInSkeletonOrder = source.RefComponentEulers;

        Vector3[] sourceLocalRefPositionsInSkeletonOrder = source.RefLocalPositions;
        Vector3[] sourceLocalRefEulersInSkeletonOrder = source.RefLocalEulers;
        Vector3[] sourceLocalRefScalesInSkeletonOrder = source.RefLocalScales;

        //获取target全部信息，骨骼顺序
        Transform[] targetBoneInSkeletonOrder = target.Bones;

        Vector3[] targetRefPositionsInSkeletonOrder = target.RefComponentPositions;
        Vector3[] targetRefEulersInSkeletonOrder = target.RefComponentEulers;

        Vector3[] targetLocalRefPositionsInSkeletonOrder = target.RefLocalPositions;
        Vector3[] targetLocalRefEulersInSkeletonOrder = target.RefLocalEulers;
        Vector3[] targetLocalRefScalesInSkeletonOrder = target.RefLocalScales;

        bone_count = profile.boneNames.Length;

        //骨骼顺序转profile顺序
        int[] source_indices = source.BoneIndicesInProfileBoneOrder;
        int[] target_indices = target.BoneIndicesInProfileBoneOrder;
        types = target.BoneRetargetingTypeInProfileOrder;

        sourceSkeletonBones = new Transform[bone_count];
        targetSkeletonBones = new Transform[bone_count];
        sourceComponentRefPoses = new SkeletonBone[bone_count];
        targetComponentRefPoses = new SkeletonBone[bone_count];
        sourceLocalRefPoses = new SkeletonBone[bone_count];
        targetLocalRefPoses = new SkeletonBone[bone_count];
        parentIndices = new int[bone_count];
        childInProfile = new bool[bone_count];
        orientAndScaleDataList = new List<OrientAndScaleData>();


        for (int i = 0; i < bone_count; i++) {

            int source_index = source_indices[i];
            if (source_index > -1)
            {
                sourceSkeletonBones[i] = sourceBoneInSkeletonOrder[source_index];
                SkeletonBone sourceComponentRefPose = new SkeletonBone();
                sourceComponentRefPose.position = sourceRefPositionsInSkeletonOrder[source_index];
                sourceComponentRefPose.rotation = Quaternion.Euler(sourceRefEulersInSkeletonOrder[source_index]);
                sourceComponentRefPose.scale = sourceLocalRefScalesInSkeletonOrder[source_index];
                sourceComponentRefPoses[i] = sourceComponentRefPose;

                SkeletonBone sourceLocalRefPose = new SkeletonBone();
                sourceLocalRefPose.position = sourceLocalRefPositionsInSkeletonOrder[source_index];
                sourceLocalRefPose.rotation = Quaternion.Euler(sourceLocalRefEulersInSkeletonOrder[source_index]);
                sourceLocalRefPose.scale = sourceLocalRefScalesInSkeletonOrder[source_index];
                sourceLocalRefPoses[i] = sourceLocalRefPose;
            }


            int target_index = target_indices[i];
            if (target_index > -1)
            {
                targetSkeletonBones[i] = targetBoneInSkeletonOrder[target_index];
                SkeletonBone targetComponentRefPose = new SkeletonBone();
                targetComponentRefPose.position = targetRefPositionsInSkeletonOrder[target_index];
                targetComponentRefPose.rotation = Quaternion.Euler(targetRefEulersInSkeletonOrder[target_index]);
                targetComponentRefPose.scale = targetLocalRefScalesInSkeletonOrder[target_index];
                targetComponentRefPoses[i] = targetComponentRefPose;

                SkeletonBone targetLocalRefPose = new SkeletonBone();
                targetLocalRefPose.position = targetLocalRefPositionsInSkeletonOrder[target_index];
                targetLocalRefPose.rotation = Quaternion.Euler(targetLocalRefEulersInSkeletonOrder[target_index]);
                targetLocalRefPose.scale = targetLocalRefScalesInSkeletonOrder[target_index];
                targetLocalRefPoses[i] = targetLocalRefPose;

                //存储父骨骼索引，如果没有找到则为-1
                parentIndices[i] = -1;
                if (targetSkeletonBones[i]) {
                    Transform targetBoneParent = targetSkeletonBones[i].parent;
                    if (targetBoneParent) {
                        for (int j = 0; j < i; j++) {
                            if (targetSkeletonBones[j] == targetBoneParent) {
                                parentIndices[i] = j;
                                childInProfile[j] = true;
                                if (types[j] == BoneRetargetingType.orientAndScale) {
                                    OrientAndScaleData data = orientAndScaleDataList.Find((o) => { return o.boneProfileIndex == j; });
                                    data.childIndices.Add(i);
                                    data.addUpChildLocalPosition = data.addUpChildLocalPosition + targetLocalRefPose.position;
                                }
                                break;
                            }
                        }
                    }
                }
                if (types[i] == BoneRetargetingType.orientAndScale) {
                    OrientAndScaleData data = new OrientAndScaleData();
                    data.boneProfileIndex = i;
                    data.parentIndex = parentIndices[i];
                    data.childIndices = new List<int>();
                    orientAndScaleDataList.Add(data);
                }
            }
        }

#if (STILL_IN_TEST)
        for (int i = 0; i < bone_count; i++) {
            Transform source_bone = sourceSkeletonBones[i];
            Transform target_bone = targetSkeletonBones[i];
            if (source_bone && target_bone) {
                target_bone.gameObject.name = source_bone.gameObject.name;
            }
        }
#endif
        Debug.Log("Init");
    }

    bool CheckCanRetarget() {
        if (source == null || target == null) return false;
        if (source.retargetingProfile != target.retargetingProfile) return false;
        return true;
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        if (bone_count == 0) { 
            
            return; 
        }
#if (STILL_IN_TEST)

        testClip.SampleAnimation(source.transform.root.gameObject, time * testClip.length);
        if (useAnimation)
        {
            testClip.SampleAnimation(target.transform.root.gameObject, time * testClip.length);
            return;
        }


#endif
        //iter 1
        for (int i = 0; i < bone_count; i++) {
            Transform targetBone = targetSkeletonBones[i];
            Transform sourceBone = sourceSkeletonBones[i];
            if (!targetBone || !sourceBone) continue;

            SkeletonBone targetComponentRefPose = targetComponentRefPoses[i];
            SkeletonBone sourceComponentRefPose = sourceComponentRefPoses[i];
            SkeletonBone targetLocalRefPose = targetLocalRefPoses[i];
            SkeletonBone sourceLocalRefPose = sourceLocalRefPoses[i];

            BoneRetargetingType type = types[i];

            //这三句为获取原动画信息
            Vector3 localScale = sourceBone.localScale;
            Quaternion componentRotation = source.GetComponentSpaceRotation(sourceBone.rotation);
            Quaternion localRotation = sourceBone.localRotation;
            Vector3 componentPosition = source.GetComponentSpacePosition(sourceBone.position);
            Vector3 localPosition = sourceBone.localPosition;

            switch (type) {
                case BoneRetargetingType.animation:
                    componentRotation = componentRotation * Quaternion.Inverse(sourceComponentRefPose.rotation) * targetComponentRefPose.rotation;
                    target.SetComponentSpaceRotation(targetBone, componentRotation);
                    target.SetComponentSpacePosition(targetBone, componentPosition);
                    targetBone.localScale = localScale;
                    break;
                case BoneRetargetingType.skeleton:
                    componentRotation = componentRotation * Quaternion.Inverse(sourceComponentRefPose.rotation) * targetComponentRefPose.rotation;
                    target.SetComponentSpaceRotation(targetBone, componentRotation);
                    targetBone.localPosition = targetLocalRefPose.position;
                    break;
                case BoneRetargetingType.animationRelative:

                    componentPosition = componentPosition - sourceComponentRefPose.position + targetComponentRefPose.position;
                    componentRotation = componentRotation * Quaternion.Inverse(sourceComponentRefPose.rotation) * targetComponentRefPose.rotation;
                    Vector3 local_scale = targetBone.localScale - sourceComponentRefPose.scale + targetComponentRefPose.scale;

                    target.SetComponentSpacePosition(targetBone, componentPosition);
                    target.SetComponentSpaceRotation(targetBone, componentRotation);
                    targetBone.localScale = local_scale;
                    break;

                case BoneRetargetingType.animationScaled:

                    componentRotation = componentRotation * Quaternion.Inverse(sourceComponentRefPose.rotation) * targetComponentRefPose.rotation;

                    float sourceRefBoneLength = sourceLocalRefPose.position.magnitude;
                    float targetRefBoneLength = targetLocalRefPose.position.magnitude;

                    target.SetComponentSpacePosition(targetBone, componentPosition);
                    target.SetComponentSpaceRotation(targetBone, componentRotation);

                    if (sourceRefBoneLength > 0.001)
                    {
                        targetBone.localPosition = targetBone.localPosition * (targetRefBoneLength / sourceRefBoneLength);
                    }

                    break;

                case BoneRetargetingType.orientAndScale:
                    componentRotation = componentRotation * Quaternion.Inverse(sourceComponentRefPose.rotation) * targetComponentRefPose.rotation;
                    //target.SetComponentSpaceRotation(targetBone, componentRotation);
                    target.SetComponentSpacePosition(targetBone, componentPosition);
                    break;
            }  
        }

        //iter 2
        foreach (OrientAndScaleData data in orientAndScaleDataList) {
            int bone_index = data.boneProfileIndex;

            Transform targetBone = targetSkeletonBones[bone_index];

            SkeletonBone targetComponentRefPose = targetComponentRefPoses[bone_index];
            SkeletonBone sourceComponentRefPose = sourceComponentRefPoses[bone_index];
            SkeletonBone targetLocalRefPose = targetLocalRefPoses[bone_index];
            SkeletonBone sourceLocalRefPose = sourceLocalRefPoses[bone_index];

            if (data.childIndices.Count > 0)
            {
                int num_child_bone = data.childIndices.Count;
                Vector3 newAddUpLocalPosition = Vector3.zero;
                Transform[] childBones = new Transform[num_child_bone];
                Vector3[] positions = new Vector3[num_child_bone];
                Quaternion[] rotations = new Quaternion[num_child_bone];
                for (int i = 0; i < num_child_bone; i++)
                {
                    int childBoneIndex = data.childIndices[i];
                    Transform childBone = targetSkeletonBones[childBoneIndex];
                    childBones[i] = childBone;
                    newAddUpLocalPosition = newAddUpLocalPosition + childBone.localPosition;
                    positions[i] = childBone.position;
                    rotations[i] = childBone.rotation;
                }
                if (data.addUpChildLocalPosition.sqrMagnitude > 0.001 && newAddUpLocalPosition.sqrMagnitude > 0.001)
                {
                    Vector3 world_from = targetBone.TransformDirection(data.addUpChildLocalPosition.normalized);
                    Vector3 world_to = targetBone.TransformDirection(newAddUpLocalPosition.normalized);
                    Quaternion dtq = Quaternion.FromToRotation(world_from, world_to);
                    targetBone.rotation = dtq * targetBone.rotation;
                    for (int i = 0; i < num_child_bone; i++)
                    {
                        Transform childBone = childBones[i];
                        childBone.position = positions[i];
                        childBone.rotation = rotations[i];
                    }
                }
            }
            else {
                Transform sourceBone = sourceSkeletonBones[bone_index];
                Quaternion componentRotation = source.GetComponentSpaceRotation(sourceBone.rotation);
                componentRotation = componentRotation * Quaternion.Inverse(sourceComponentRefPose.rotation) * targetComponentRefPose.rotation;
                target.SetComponentSpaceRotation(targetBone, componentRotation);
            }

            float sourceRefBoneLength = sourceLocalRefPose.position.magnitude;
            float targetRefBoneLength = targetLocalRefPose.position.magnitude;

            if (sourceRefBoneLength > 0.001)
            {
                targetBone.localPosition = targetBone.localPosition * (targetRefBoneLength / sourceRefBoneLength);
            }
        }
    }
}

public class OrientAndScaleData
{
    public int boneProfileIndex;
    public int parentIndex;
    public List<int> childIndices;
    public Vector3 addUpChildLocalPosition;
}