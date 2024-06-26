﻿using MoviesBE.DTOs;
using MoviesBE.Entities;
using Neo4j.Driver;

namespace MoviesBE.Utilities.Conversions;

public static class UserConverter
{
    public static User ConvertNodeToUser(INode node)
    {
        ArgumentNullException.ThrowIfNull(node, nameof(node));

        return new User
        {
            Auth0Id = node.Properties.GetValueOrDefault("auth0Id", null).As<string>(),
            Email = node.Properties.GetValueOrDefault("email", null).As<string>(),
            FullName = node.Properties.GetValueOrDefault("fullName", null).As<string>(),
            ProfilePicture = node.Properties.GetValueOrDefault("profilePicture", null).As<string>(),
            EmailVerified = node.Properties.GetValueOrDefault("emailVerified", false).As<bool>(),
            Role = Enum.Parse<Role>(node.Properties.GetValueOrDefault("role", "User").As<string>()),
            Language = node.Properties.GetValueOrDefault("language", null).As<string>()
        };
    }

    public static User MapToUser(UserInfo userInfo)
    {
        ArgumentNullException.ThrowIfNull(userInfo, nameof(userInfo));

        return new User
        {
            Auth0Id = userInfo.Sub,
            Email = userInfo.Email,
            FullName = userInfo.Name,
            ProfilePicture = userInfo.Picture,
            EmailVerified = userInfo.EmailVerified,
            Role = Role.User,
            Language = userInfo.Locale
        };
    }
}