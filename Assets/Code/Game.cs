// <copyright project="Assembly-CSharp" file="Game.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using Unity.NetCode;

namespace NetCode.Saving.Sample
{
// Create a custom bootstrap, which enables auto-connect.
// The bootstrap can also be used to configure other settings as well as to
// manually decide which worlds (client and server) to create based on user input
    [UnityEngine.Scripting.Preserve]
    public class GameBootstrap : ClientServerBootstrap
    {
        public override bool Initialize(string defaultWorldName)
        {
            AutoConnectPort = 7978; // Enabled auto connect
            return base.Initialize(defaultWorldName); // Use the regular bootstrap
            
        }
    }
}