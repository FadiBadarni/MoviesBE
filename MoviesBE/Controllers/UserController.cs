﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoviesBE.Services.UserService;

namespace MoviesBE.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpPost("{movieId:int}/bookmark")]
    public async Task<ActionResult> BookmarkMovie(int movieId, [FromQuery] string userId)
    {
        var savedMovieId = await _userService.BookmarkMovie(userId, movieId);
        return Ok(new { MovieId = savedMovieId });
    }

    [Authorize]
    [HttpGet("{userId}/watchlist")]
    public async Task<ActionResult<List<int>>> FetchWatchlist(string userId)
    {
        var watchlist = await _userService.FetchWatchlist(userId);
        return Ok(watchlist);
    }

    [Authorize]
    [HttpDelete("{movieId:int}/unbookmark")]
    public async Task<ActionResult> UnbookmarkMovie(int movieId, [FromQuery] string userId)
    {
        var unbookmarkedMovieId = await _userService.UnbookmarkMovie(userId, movieId);
        return Ok(new { MovieId = unbookmarkedMovieId, Message = "Movie unbookmarked successfully." });
    }
}