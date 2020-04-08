﻿using Imgeneus.Core.DependencyInjection;
using Imgeneus.Database;
using Imgeneus.Database.Constants;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Player;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Game
{
    /// <summary>
    /// The virtual representation of game world.
    /// </summary>
    public class GameWorld : IGameWorld
    {
        private readonly ILogger<GameWorld> _logger;

        public GameWorld()
        {
            _logger = DependencyContainer.Instance.Resolve<ILogger<GameWorld>>();
        }

        #region Players

        /// <inheritdoc />
        public event Action<Character> OnPlayerEnteredMap;

        /// <inheritdoc />
        public event Action<Character> OnPlayerMove;

        /// <inheritdoc />
        public event Action<int, Motion> OnPlayerMotion;

        /// <inheritdoc />
        public BlockingCollection<Character> Players { get; private set; } = new BlockingCollection<Character>();

        /// <inheritdoc />
        public Character LoadPlayer(int characterId)
        {
            using var database = DependencyContainer.Instance.Resolve<IDatabase>();
            var dbCharacter = database.Characters.Include(c => c.Skills).ThenInclude(cs => cs.Skill)
                                               .Include(c => c.Items).ThenInclude(ci => ci.Item)
                                               .Include(c => c.User)
                                               .FirstOrDefault(c => c.Id == characterId);
            var newPlayer = Character.FromDbCharacter(dbCharacter);

            Players.Add(newPlayer);
            _logger.LogDebug($"Player {newPlayer.Id} connected to game world");

            return newPlayer;
        }

        /// <inheritdoc />
        public async Task PlayerMoves(int characterId, MovementType movementType, float X, float Y, float Z, ushort angle)
        {
            var player = Players.FirstOrDefault(p => p.Id == characterId);
            player.PosX = X;
            player.PosY = Y;
            player.PosZ = Z;
            player.Angle = angle;
            OnPlayerMove?.Invoke(player);

            if (movementType == MovementType.Stopped)
            {
                using var database = DependencyContainer.Instance.Resolve<IDatabase>();
                var dbCharacter = database.Characters.Find(characterId);
                dbCharacter.Angle = angle;
                dbCharacter.PosX = X;
                dbCharacter.PosY = Y;
                dbCharacter.PosZ = Z;
                await database.SaveChangesAsync();
            }

            _logger.LogDebug($"Character {player.Id} moved to x={player.PosX} y={player.PosY} z={player.PosZ} angle={player.Angle}");
        }

        /// <inheritdoc />
        public Character LoadPlayerInMap(int characterId)
        {
            var player = Players.FirstOrDefault(p => p.Id == characterId);

            // TODO: implement maps. For now just notify other players, that new player arrived.

            OnPlayerEnteredMap?.Invoke(player);

            return player;
        }

        /// <inheritdoc />
        public void PlayerSendMotion(int characterId, Motion motion)
        {
            if (motion == Motion.None || motion == Motion.Sit)
            {
                var player = Players.First(p => p.Id == characterId);
                player.Motion = motion;
            }
            OnPlayerMotion?.Invoke(characterId, motion);
        }

        #endregion
    }
}
