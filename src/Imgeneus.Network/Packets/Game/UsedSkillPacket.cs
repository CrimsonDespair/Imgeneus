﻿using Imgeneus.Network.Data;

namespace Imgeneus.Network.Packets.Game
{
    public struct UsedSkillPacket : IDeserializedPacket
    {
        public byte SkillNumber { get; }

        public UsedSkillPacket(IPacketStream packet)
        {
            SkillNumber = packet.Read<byte>();
        }
    }
}
