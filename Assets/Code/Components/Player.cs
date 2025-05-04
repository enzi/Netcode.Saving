// <copyright project="Assembly-CSharp" file="Player.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace NetCode.Saving.Sample
{
    public struct Cube : IComponentData
    {
    }

    public struct SteamId : IComponentData
    {
        public ulong Value;
    }

    public struct NetworkSave : IComponentData, IEnableableComponent
    {
    }
    
    public struct CubeInput : IInputComponentData
    {
        public int Horizontal;
        public int Vertical;

        public FixedString512Bytes ToFixedString() => $"→{Horizontal} ↑{Vertical}";
    }
    
    public struct CubeSpawner : IComponentData
    {
        public Entity Cube;
    }
    
    public struct LoadPlayerRequest : IComponentData, IEnableableComponent
    {
        public ulong SteamId;
    }

    public struct StatComponent : IComponentData
    {
        public int Value;
    }
}