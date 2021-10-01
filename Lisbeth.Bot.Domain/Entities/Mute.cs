﻿using Lisbeth.Bot.Domain.Entities.Base;
using System;
using MikyM.Common.Domain.Entities;

namespace Lisbeth.Bot.Domain.Entities
{
    public class Mute : DiscordAggregateRootEntity
    {
        public DateTime? LiftedOn { get; set; }
        public DateTime? AppliedUntil { get; set; }
        public DateTime? AppliedOn { get; set; } = DateTime.UtcNow;
        public ulong AppliedById { get; set; }
        public ulong LiftedById { get; set; }
        public string Reason { get; set; } = "";

        public Mute()
        {
        }

        public Mute ShallowCopy()
        {
            return (Mute)this.MemberwiseClone();
        }
    }
}
