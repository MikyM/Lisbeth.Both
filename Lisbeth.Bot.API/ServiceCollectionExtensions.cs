﻿using DSharpPlus;
using DSharpPlus.Interactivity.Enums;
using Lisbeth.Bot.Application.Discord.ApplicationCommands;
using Lisbeth.Bot.Application.Discord.EventHandlers;
using Lisbeth.Bot.Application.Discord.SlashCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MikyM.Discord;
using MikyM.Discord.Extensions.CommandsNext;
using MikyM.Discord.Extensions.Interactivity;
using MikyM.Discord.Extensions.SlashCommands;
using OpenTracing;
using OpenTracing.Mock;
using System;
using System.Collections.Generic;

namespace Lisbeth.Bot.API
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureDiscord(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ITracer>(_ => new MockTracer());
            services.AddDiscord(options =>
            {
                options.Token = Environment.GetEnvironmentVariable("LisbethTstToken");
                options.Intents = DiscordIntents.All;
            });
            services.AddDiscordHostedService();

            #region commands
            services.AddDiscordSlashCommands(_ => { }, extension =>
            {
                extension.RegisterCommands<MuteApplicationCommands>(790631933758799912);
                extension.RegisterCommands<BanApplicationCommands>(790631933758799912);
                extension.RegisterCommands<TicketSlashCommands>(790631933758799912);
                extension.RegisterCommands<AdminUtilSlashCommands>(790631933758799912);
                extension.RegisterCommands<PruneApplicationCommands>(790631933758799912);
            });
            services.AddDiscordInteractivity(options =>
            {
                options.PaginationBehaviour = PaginationBehaviour.WrapAround;
                options.ResponseBehavior = InteractionResponseBehavior.Ack;
                options.AckPaginationButtons = true;
                options.Timeout = TimeSpan.FromMinutes(2);
            });
            services.AddDiscordCommandsNext(options =>
            {
                options.StringPrefixes = new List<string>() { "!" };
                options.CaseSensitive = false;
                options.DmHelp = false;
                options.EnableDms = false;
                options.EnableMentionPrefix = true;
                options.IgnoreExtraArguments = true;
                options.EnableDefaultHelp = false;
            });

            #endregion



            #region events

            services.AddDiscordSlashCommandsEventsSubscriber<SlashCommandEvents>();

            #endregion
        }
    }
}
