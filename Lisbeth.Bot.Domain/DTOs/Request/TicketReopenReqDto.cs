﻿namespace Lisbeth.Bot.Domain.DTOs.Request
{
    public class TicketReopenReqDto
    {
        public long? Id { get; set; }
        public ulong? OwnerId { get; set; }
        public ulong? GuildId { get; set; }
        public ulong? ChannelId { get; set; }
        public ulong? RequestedById { get; set; }
    }
}
