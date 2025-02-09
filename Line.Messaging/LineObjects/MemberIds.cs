﻿namespace Line.Messaging;

/// <summary>
/// Response from Get group member user IDs API
/// </summary>
public class GroupMemberIds
{
    /// <summary>
    /// List of user IDs of the members in the group.
    /// Max: 100 user IDs
    /// </summary>
    public required string[] MemberIds { get; init; }

    /// <summary>
    /// continuationToken
    /// Only returned when there are more user IDs remaining in memberIds
    /// </summary>
    public string? Next { get; init; }
}