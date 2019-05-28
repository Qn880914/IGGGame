/********************************************************************
	created:	2017/02/10
	created:	10:2:2017   11:31
	file base:	MeshAnimationBoneTransform
	file ext:	cs
	author:		Horatio
	
	purpose:	
*********************************************************************/

#region Namespace

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace IGG.Animation.Model
{
    public class MeshAnimationBoneTransform
    {
        public List<Vector3> Positions;

        public List<Quaternion> Rotations;

        public MeshAnimationBoneTransform()
        {
            Positions = new List<Vector3>();
            Rotations = new List<Quaternion>();
        }
    }
}