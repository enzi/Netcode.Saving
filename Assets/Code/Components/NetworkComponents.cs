// <copyright project="Assembly-CSharp" file="NetworkComponents.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using Unity.Entities;
using Unity.NetCode;

namespace NetCode.Saving.Sample
{
    // unused but would be used to start
    // the auth process on the server for this sample the client is "trusted"
    // and the GoInGameRequest with the SteamId will be accepted
    public struct StartAuth : IComponentData
    {
        public ulong SteamId; // prototype way
        //public FixedString512Bytes AuthToken; // in a real game scenario an AuthToken would be sent to the server for it to verify
    }

    // RPC request from client to server for game to go "in game" and send snapshots / inputs
    public struct GoInGameRequest : IRpcCommand
    {
        public ulong SteamId;
    }
}