// <copyright project="Assembly-CSharp" file="CubeInputAuthoring.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using Unity.Entities;
using UnityEngine;

namespace NetCode.Saving.Sample
{
    [DisallowMultipleComponent]
    public class CubeInputAuthoring : MonoBehaviour
    {
        class CubeInputBaking : Baker<CubeInputAuthoring>
        {
            public override void Bake(CubeInputAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<CubeInput>(entity);
            }
        }
    }
}