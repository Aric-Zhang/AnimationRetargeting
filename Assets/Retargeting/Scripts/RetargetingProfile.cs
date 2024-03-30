#define CREATE_DEFAULT_MANNEQUIN

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="NewRetargetingProfile",menuName ="Retargeting/Retargeting Profile")]
public class RetargetingProfile : ScriptableObject
{
#if(CREATE_DEFAULT_MANNEQUIN)
    public string[] boneNames = new string[] { "root", "pelvis", "spine_01", "spine_02", "spine_03", "clavicle_l", "upperarm_l", "lowerarm_l", "hand_l", "index_01_l", "index_02_l", "index_03_l", "middle_01_l", "middle_02_l", "middle_03_l", "pinky_01_l", "pinky_02_l", "pinky_03_l", "ring_01_l", "ring_02_l", "ring_03_l", "thumb_01_l", "thumb_02_l", "thumb_03_l", "lowerarm_twist_01_l", "upperarm_twist_01_l", "clavicle_r", "upperarm_r", "lowerarm_r", "hand_r", "index_01_r", "index_02_r", "index_03_r", "middle_01_r", "middle_02_r", "middle_03_r", "pinky_01_r", "pinky_02_r", "pinky_03_r", "ring_01_r", "ring_02_r", "ring_03_r", "thumb_01_r", "thumb_02_r", "thumb_03_r", "lowerarm_twist_01_r", "upperarm_twist_01_r", "neck_01", "head", "thigh_l", "calf_l", "calf_twist_01_l", "foot_l", "ball_l", "thigh_twist_01_l", "thigh_r", "calf_r", "calf_twist_01_r", "foot_r", "ball_r", "thigh_twist_01_r", "ik_foot_root", "ik_foot_l", "ik_foot_r", "ik_hand_root", "ik_hand_gun", "ik_hand_l", "ik_hand_r" };
#else
    public string[] boneNames;
#endif

}
