﻿using System.ComponentModel.DataAnnotations;
using MoviesBE.DTOs;

namespace MoviesBE.Entities;

public class User
{
    [Key]
    public string? Auth0Id { get; set; }

    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    public string? FullName { get; set; }

    public string? ProfilePicture { get; set; }

    [Required]
    public bool EmailVerified { get; set; }

    public Role Role { get; set; }

    public string? Language { get; set; }

    public List<int>? WatchlistMovieIds { get; set; }
}