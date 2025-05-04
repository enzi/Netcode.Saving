// <copyright project="Assembly-CSharp" file="LoadSystem.cs">
// Copyright © 2025 Thomas Enzenebner. All rights reserved.
// </copyright>

using NZCore;
using NZCore.Saving;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Windows;

namespace NetCode.Saving.Sample
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SavingSystemGroup))]
    [UpdateAfter(typeof(SaveGameSystem))]
    public partial class NetworkSaveLoadSystem : SystemBase
    {
        private NativeHashMap<ulong, Entity> pendingInstantiations;
        
        protected override void OnCreate()
        {
            pendingInstantiations = new NativeHashMap<ulong, Entity>(0, Allocator.Persistent);
            
            RequireForUpdate<CubeSpawner>();
            RequireForUpdate<SaveSystemRequestSingleton>();
        }

        protected override void OnDestroy()
        {
            pendingInstantiations.Dispose();
        }

        protected override void OnUpdate()
        {
            var prefab = SystemAPI.GetSingleton<CubeSpawner>().Cube;
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            var saveFileSystemRequests = SystemAPI.GetSingleton<SaveFileSystemRequestSingleton>();
            var saveSystemRequests = SystemAPI.GetSingleton<SaveSystemRequestSingleton>();

            // since spawning is deferred, the load requests from the save system
            // are iterated here, identifier is compared and then the connection will be setup
            foreach (var loadEntityResponse in saveSystemRequests.LoadEntityResponses)
            {
                if (ulong.TryParse(loadEntityResponse.Identifier.ToString(), out var steamId))
                {
                    var connectionEntity = pendingInstantiations[steamId];
                    var networkId = SystemAPI.GetComponent<NetworkId>(connectionEntity);
                    
                    SetupConnection(commandBuffer, loadEntityResponse.Entity, connectionEntity, networkId);

                    pendingInstantiations.Remove(steamId);
                }
                else
                {
                    Debug.LogError($"Could not parse {loadEntityResponse.Identifier}");
                }
            }
            
            // process any LoadPlayerRequest from GoInGame system
            foreach (var (playerReqRO, connectionEntity) in SystemAPI.Query<RefRO<LoadPlayerRequest>>().WithEntityAccess())
            {
                ref readonly var playerReq = ref playerReqRO.ValueRO;

                if (playerReq.SteamId != 0)
                {
                    // a real scenario wouldn't just load up client sent data 
                    var filePath = SaveFileSystem.GetDefaultSavePath($"char_{playerReq.SteamId}.sav");

                    if (File.Exists(filePath))
                    {
                        saveFileSystemRequests.AddLoadEntityRequest(new LoadEntityRequest()
                        {
                            Identifier = new FixedString64Bytes($"{playerReq.SteamId}"), // identifier is used to setup the connection later
                            FilePath = filePath
                        });
                        
                        // simple storage to associate steamId and connection entity for the deferred spawning process
                        // and connection setup
                        pendingInstantiations.Add(playerReq.SteamId, connectionEntity);
                    }
                    else
                    {
                        // no save game
                        // create default player prefab for connection
                        var player = commandBuffer.Instantiate(prefab);
                        var networkId = SystemAPI.GetComponent<NetworkId>(connectionEntity);
                        
                        SetupConnection(commandBuffer, player, connectionEntity, networkId);
                        
                        // Give each NetworkId their own spawn pos:
                        {
                            var isEven = (networkId.Value & 1) == 0;
                            const float halfCharacterWidthPlusHalfPadding = .55f;
                            const float spawnStaggeredOffset = 0.25f;
                            var staggeredXPos = networkId.Value * math.@select(halfCharacterWidthPlusHalfPadding, -halfCharacterWidthPlusHalfPadding, isEven) +
                                                math.@select(-spawnStaggeredOffset, spawnStaggeredOffset, isEven);
                            var preventZFighting = -0.01f * networkId.Value;

                            commandBuffer.SetComponent(player, LocalTransform.FromPosition(new float3(staggeredXPos, preventZFighting, 0)));
                        }
                    }
                }
                
                // disable LoadPlayerRequest when done
                EntityManager.SetComponentEnabled<LoadPlayerRequest>(connectionEntity, false);
            }
            
            commandBuffer.Playback(EntityManager);
            
            var requestSingleton = SystemAPI.GetSingleton<SaveSystemRequestSingleton>();

            // any entity with an enabled NetworkSave will be serialized here
            foreach (var (steamIdRO, legBuffer, connectionEntity) in SystemAPI.Query<RefRO<SteamId>, DynamicBuffer<LinkedEntityGroup>>()
                         .WithAll<NetworkSave>().WithEntityAccess())
            {
                if (steamIdRO.ValueRO.Value != 0)
                {
                    var steamId = steamIdRO.ValueRO.Value;
                    
                    foreach (var child in legBuffer)
                    {
                        if (child.Value == connectionEntity)
                        {
                            continue;
                        }

                        // simple tag-check to serialize the correct entity
                        if (SystemAPI.HasComponent<Cube>(child.Value))
                        {
                            requestSingleton.SaveEntityRequest(new SaveEntityRequest()
                            {
                                Entity = child.Value,
                                Compress = true,
                                FileName = new FixedString64Bytes($"char_{steamId}.sav")
                            });
                        }

                        break;
                    }
                }
                
                // disable NetworkSave when done
                EntityManager.SetComponentEnabled<NetworkSave>(connectionEntity, false);
            }
        }

        private void SetupConnection(EntityCommandBuffer commandBuffer, Entity player, Entity connectionEntity, NetworkId networkId)
        {
            // Associate the instantiated prefab with the connected client's assigned NetworkId
            commandBuffer.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });

            // Add the player to the linked entity group so it is destroyed automatically on disconnect
            commandBuffer.AppendToBuffer(connectionEntity, new LinkedEntityGroup { Value = player });
        }
    }
}