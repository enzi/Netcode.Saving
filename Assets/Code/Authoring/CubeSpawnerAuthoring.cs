// <copyright project="Assembly-CSharp" file="CubeSpawnerAuthoring.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using Unity.Entities;
using UnityEngine;

namespace NetCode.Saving.Sample
{
    [DisallowMultipleComponent]
    public class CubeSpawnerAuthoring : MonoBehaviour
    {
        public GameObject Cube;

        class Baker : Baker<CubeSpawnerAuthoring>
        {
            public override void Bake(CubeSpawnerAuthoring authoring)
            {
                CubeSpawner component = default(CubeSpawner);
                component.Cube = GetEntity(authoring.Cube, TransformUsageFlags.Dynamic);
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, component);
            }
        }
    }
}
