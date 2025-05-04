// <copyright project="Assembly-CSharp" file="CubeAuthoring.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using NZCore.Saving;
using Unity.Entities;
using UnityEngine;

namespace NetCode.Saving.Sample
{
    [DisallowMultipleComponent]
    public class CubeAuthoring : MonoBehaviour
    {
        class Baker : Baker<CubeAuthoring>
        {
            public override void Bake(CubeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Cube>(entity);

                AddComponent<LoadPlayerRequest>(entity);

                var statEntity = CreateAdditionalEntity(TransformUsageFlags.None, false, "StatEntity");
                AddComponent(statEntity, new SavableEntity());
                AddComponent(statEntity, new IgnoreInWorldSaving());
                AddComponent(statEntity, new StatComponent() { Value = 1});
            }
        }
    }
}