/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   11:31
	file base:	meshanimationbonegroup
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/

#region Namespace

using IGG.Animation.Model;
using System.Collections.Generic;

#endregion

//using System.Collections.Generic;

public class MeshAnimationBoneGroup
{
    public List<string> BoneNames;

    public Dictionary<string, MeshAnimationBoneTransform> Bones;

    public MeshAnimationBoneGroup(List<string> pBoneNames, int pBoneCount)
    {
        BoneNames = pBoneNames;
        Bones = new Dictionary<string, MeshAnimationBoneTransform>();
        foreach (string current in pBoneNames)
        {
            Bones[current] = new MeshAnimationBoneTransform();
        }
    }
}