// <copyright project="Assembly-CSharp" file="CubeInputSystem.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace NetCode.Saving.Sample
{
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial struct CubeInputSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<CubeSpawner>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var playerInput in SystemAPI.Query<RefRW<CubeInput>>().WithAll<GhostOwnerIsLocal>())
            {
                playerInput.ValueRW = default;
                if (Input.GetKey("left") || Input.GetKey(KeyCode.A))
                    playerInput.ValueRW.Horizontal -= 1;
                if (Input.GetKey("right") || Input.GetKey(KeyCode.D))
                    playerInput.ValueRW.Horizontal += 1;
                if (Input.GetKey("down") || Input.GetKey(KeyCode.S))
                    playerInput.ValueRW.Vertical -= 1;
                if (Input.GetKey("up") || Input.GetKey(KeyCode.W))
                    playerInput.ValueRW.Vertical += 1;
            }
        }
    }
}