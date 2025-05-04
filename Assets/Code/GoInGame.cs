// <copyright project="Assembly-CSharp" file="GoInGame.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace NetCode.Saving.Sample
{
    /// <summary>
    /// This allows sending RPCs between a stand alone build and the editor for testing purposes in the event when you finish this example
    /// you want to connect a server-client stand alone build to a client configured editor instance.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [CreateAfter(typeof(RpcSystem))]
    public partial struct SetRpcSystemDynamicAssemblyListSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            SystemAPI.GetSingletonRW<RpcCollection>().ValueRW.DynamicAssemblyList = true;
            state.Enabled = false;
        }
    }

    // When client has a connection with network id, go in game and tell server to also go in game
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct GoInGameClientSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<NetworkId>()
                .WithNone<NetworkStreamInGame>();
            state.RequireForUpdate(state.GetEntityQuery(builder));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (id, /*authRequest,*/ entity) in SystemAPI.Query<RefRO<NetworkId>/*, RefRO<StartAuth>*/>().WithEntityAccess().WithNone<NetworkStreamInGame>())
            {
                commandBuffer.AddComponent<NetworkStreamInGame>(entity);
                var req = commandBuffer.CreateEntity();
                commandBuffer.AddComponent(req, new GoInGameRequest()
                {
                    SteamId = 10000 //authRequest.ValueRO.SteamId
                });
                commandBuffer.AddComponent(req, new SendRpcCommandRequest { TargetConnection = entity });
            }

            commandBuffer.Playback(state.EntityManager);
        }
    }

    // When server receives go in game request, go in game and delete request
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GoInGameServerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<GoInGameRequest>()
                .WithAll<ReceiveRpcCommandRequest>();
            state.RequireForUpdate(state.GetEntityQuery(builder));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var worldName = state.WorldUnmanaged.Name;

            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (reqSrc, req, reqEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<GoInGameRequest>>().WithEntityAccess())
            {
                var connectionEntity = reqSrc.ValueRO.SourceConnection;
               
                Debug.Log($"'{worldName}' setting connection '{connectionEntity}' to in game!");
                
                commandBuffer.AddComponent<NetworkStreamInGame>(connectionEntity);
                commandBuffer.AddComponent(connectionEntity, new SteamId { Value = req.ValueRO.SteamId });
                commandBuffer.AddComponent<NetworkSave>(connectionEntity);
                commandBuffer.SetComponentEnabled<NetworkSave>(connectionEntity, false);
                commandBuffer.AddComponent(connectionEntity, new LoadPlayerRequest()
                {
                    SteamId = req.ValueRO.SteamId
                });
                commandBuffer.SetComponentEnabled<LoadPlayerRequest>(connectionEntity, true);
                
                commandBuffer.DestroyEntity(reqEntity);
            }

            commandBuffer.Playback(state.EntityManager);
        }
    }
}