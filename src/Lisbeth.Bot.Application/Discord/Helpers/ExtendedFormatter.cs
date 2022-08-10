﻿// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
// Copyright (c) 2015 Mike Santiago
// Copyright (c) 2016-2021 DSharpPlus Contributors
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Globalization;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Lisbeth.Bot.Application.Discord.Helpers;

/// <summary>
/// Contains markdown formatting helpers.
/// </summary>
public static  class ExtendedFormatter
{
    private static Regex MdSanitizeRegex { get; } = new Regex(@"([`\*_~<>\[\]\(\)""@\!\&#:\|])", RegexOptions.ECMAScript);
    private static Regex MdStripRegex { get; } = new Regex(@"([`\*_~\[\]\(\)""\|]|<@\!?\d+>|<#\d+>|<@\&\d+>|<:[a-zA-Z0-9_\-]:\d+>)", RegexOptions.ECMAScript);

    /// <summary>
    /// Creates a block of code.
    /// </summary>
    /// <param name="content">Contents of the block.</param>
    /// <param name="language">Language to use for highlighting.</param>
    /// <returns>Formatted block of code.</returns>
    public static string BlockCode(string content, string language = "")
        => $"```{language}\n{content}\n```";

    /// <summary>
    /// Creates inline code snippet.
    /// </summary>
    /// <param name="content">Contents of the snippet.</param>
    /// <returns>Formatted inline code snippet.</returns>
    public static string InlineCode(string content)
        => $"`{content}`";

    /// <summary>
    /// Creates a rendered timestamp.
    /// </summary>
    /// <param name="time">The time from now.</param>
    /// <param name="format">The format to render the timestamp in. Defaults to relative.</param>
    /// <returns>A formatted timestamp.</returns>
    public static string Timestamp(TimeSpan time, TimestampFormat format = TimestampFormat.RelativeTime)
        => Timestamp(DateTimeOffset.UtcNow + time, format);

    /// <summary>
    /// Creates a rendered timestamp.
    /// </summary>
    /// <param name="time">The time from now.</param>
    /// <param name="format">The format to render the timestamp in. Defaults to relative.</param>
    /// <returns>A formatted timestamp.</returns>
    public static string Timestamp(DateTime time, TimestampFormat format = TimestampFormat.RelativeTime)
        => Timestamp(new DateTimeOffset(time.ToUniversalTime()), format);

    /// <summary>
    /// Creates a rendered timestamp.
    /// </summary>
    /// <param name="time">Timestamp to format.</param>
    /// <param name="format">The format to render the timestamp in. Defaults to relative.</param>
    /// <returns>A formatted timestamp.</returns>
    public static string Timestamp(DateTimeOffset time, TimestampFormat format = TimestampFormat.RelativeTime)
        => $"<t:{time.ToUnixTimeSeconds()}:{(char)format}>";

    /// <summary>
    /// Creates bold text.
    /// </summary>
    /// <param name="content">Text to bolden.</param>
    /// <returns>Formatted text.</returns>
    public static string Bold(string content)
        => $"**{content}**";

    /// <summary>
    /// Creates italicized text.
    /// </summary>
    /// <param name="content">Text to italicize.</param>
    /// <returns>Formatted text.</returns>
    public static string Italic(string content)
        => $"*{content}*";

    /// <summary>
    /// Creates spoiler from text.
    /// </summary>
    /// <param name="content">Text to spoilerize.</param>
    /// <returns>Formatted text.</returns>
    public static string Spoiler(string content)
        => $"||{content}||";

    /// <summary>
    /// Creates underlined text.
    /// </summary>
    /// <param name="content">Text to underline.</param>
    /// <returns>Formatted text.</returns>
    public static string Underline(string content)
        => $"__{content}__";

    /// <summary>
    /// Creates strikethrough text.
    /// </summary>
    /// <param name="content">Text to strikethrough.</param>
    /// <returns>Formatted text.</returns>
    public static string Strike(string content)
        => $"~~{content}~~";

    /// <summary>
    /// Creates a URL that won't create a link preview.
    /// </summary>
    /// <param name="url">Url to prevent from being previewed.</param>
    /// <returns>Formatted url.</returns>
    public static string EmbedlessUrl(Uri url)
        => $"<{url}>";

    /// <summary>
    /// Creates a masked link. This link will display as specified text, and alternatively provided alt text. This can only be used in embeds.
    /// </summary>
    /// <param name="text">Text to display the link as.</param>
    /// <param name="url">Url that the link will lead to.</param>
    /// <param name="altText">Alt text to display on hover.</param>
    /// <returns>Formatted url.</returns>
    public static string MaskedUrl(string text, Uri url, string altText = "")
        => $"[{text}]({url}{(!string.IsNullOrWhiteSpace(altText) ? $" \"{altText}\"" : "")})";

    /// <summary>
    /// Escapes all markdown formatting from specified text.
    /// </summary>
    /// <param name="text">Text to sanitize.</param>
    /// <returns>Sanitized text.</returns>
    public static string Sanitize(string text)
        => MdSanitizeRegex.Replace(text, m => $"\\{m.Groups[1].Value}");

    /// <summary>
    /// Removes all markdown formatting from specified text.
    /// </summary>
    /// <param name="text">Text to strip of formatting.</param>
    /// <returns>Formatting-stripped text.</returns>
    public static string Strip(string text)
        => MdStripRegex.Replace(text, m => string.Empty);

    /// <summary>
    /// Creates a mention for specified user or member. Can optionally specify to resolve nicknames.
    /// </summary>
    /// <param name="user">User to create mention for.</param>
    /// <param name="nickname">Whether the mention should resolve nicknames or not.</param>
    /// <returns>Formatted mention.</returns>
    public static string Mention(DiscordUser user, bool nickname = false)
        => nickname
            ? $"<@!{user.Id.ToString(CultureInfo.InvariantCulture)}>"
            : $"<@{user.Id.ToString(CultureInfo.InvariantCulture)}>";

    /// <summary>
    /// Creates a mention for specified channel.
    /// </summary>
    /// <param name="channel">Channel to mention.</param>
    /// <returns>Formatted mention.</returns>
    public static string Mention(DiscordChannel channel)
        => $"<#{channel.Id.ToString(CultureInfo.InvariantCulture)}>";

    /// <summary>
    /// Creates a mention for specified role.
    /// </summary>
    /// <param name="role">Role to mention.</param>
    /// <returns>Formatted mention.</returns>
    public static string Mention(DiscordRole role)
        => $"<@&{role.Id.ToString(CultureInfo.InvariantCulture)}>";

    /// <summary>
    /// Creates a mention for a specified discord object type based on passed Id.
    /// </summary>
    /// <param name="id">Id of the object.</param>
    /// <param name="type">Type of the object to which the Id belongs.</param>
    /// <returns>Formatted mention.</returns>
    public static string Mention(ulong id, DiscordEntity type)
        => type switch
        {
            DiscordEntity.Guild => throw new NotSupportedException(),
            DiscordEntity.Channel => $"<#{id}>",
            DiscordEntity.Member => $"<@!{id}>",
            DiscordEntity.User => $"<@{id}>",
            DiscordEntity.Role => $"<&!{id}>",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

    /// <summary>
    /// Creates a custom emoji string.
    /// </summary>
    /// <param name="emoji">Emoji to display.</param>
    /// <returns>Formatted emoji.</returns>
    public static string Emoji(DiscordEmoji emoji)
        => $"<:{emoji.Name}:{emoji.Id.ToString(CultureInfo.InvariantCulture)}>";

    /// <summary>
    /// Creates a url for using attachments in embeds. This can only be used as an Image URL, Thumbnail URL, Author icon URL or Footer icon URL.
    /// </summary>
    /// <param name="filename">Name of attached image to display</param>
    /// <returns></returns>
    public static string AttachedImageUrl(string filename)
        => $"attachment://{filename}";
}
