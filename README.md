# MoviesBE: Advanced Movie Data Management Backend

MoviesBE leverages .NET 6 to offer a sophisticated backend solution for movie data management, integrating comprehensive data scraping, detailed entity modeling, and rich API endpoints for a dynamic movie information system.

## Key Features

- **Extensive Data Scraping**: Incorporates IMDb and Rotten Tomatoes scrapers to enrich movie data with ratings and additional metadata.
- **Dynamic TMDB Integration**: Fetches up-to-date movie details, enhancing the dataset with additional information beyond standard API calls.
- **Advanced Entity Modeling**: Utilizes complex entity relationships within a Neo4j graph database, modeling movies, genres, ratings, and user interactions in detail.
- **Automated Data Updates**: Background services for regular data updates and integrity checks ensure the database remains current and comprehensive.
- **Rich API Endpoints**: Offers a broad range of endpoints for accessing movie data, managing user profiles, and fetching personalized recommendations.

## Prerequisites

- .NET 6 SDK
- Neo4j Database Instance
- TMDB API Key for movie data access

## Installation & Setup

1. **Clone the Repository**

   ```bash
   git clone https://github.com/YourGithub/MoviesBE.git
   cd MoviesBE
   ```

2. **Restore Dependencies**

   ```bash
   dotnet restore
   ```

3. **Configure `appSettings.json`**

   Update the `appSettings.json` file with your TMDB API key, Neo4j database credentials, and scraper settings as shown below:

   ```json
   {
     "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
     "TMDB": {
       "ApiKey": "<TMDB_API_KEY>",
       "ApiReadAccessToken": "<TMDB_ACCESS_TOKEN>",
       "BaseUrl": "https://api.themoviedb.org/3/"
     },
     "Neo4j": {
       "Uri": "<NEO4J_URI>",
       "Username": "neo4j",
       "Password": "<NEO4J_PASSWORD>"
     },
     "IMDbScraper": { "Enabled": true, "IntervalHours": 24, "DelayMilliseconds": 10000 },
     "RTScraper": { "Enabled": true, "IntervalHours": 24, "DelayMilliseconds": 10000 },
     "MovieDataCompletion": { "Enabled": true, "IntervalHours": 24 }
   }
   ```

Note: Replace the placeholder values (enclosed in <>) with your actual TMDB API key, Neo4j credentials, etc.

## Advanced Data Management

### Entity Modeling and Repositories

The backend structures movie information into a rich set of entities such as Movies, Genres, Ratings, and User Interactions, managed through sophisticated repository classes. This design allows for intricate queries and operations, facilitating features like recommendations and detailed data analytics.

### Scraper Integration

IMDb and Rotten Tomatoes scrapers are integrated to periodically enrich the movie data. They are designed to be resilient and efficient, using headless browsers or API calls where possible, and implementing sophisticated parsing logic for reliability.

### TMDB Data Synchronization

A dedicated service interacts with the TMDB API to keep the movie data up to date. This includes not just basic movie details, but also images, videos, and external ratings, ensuring a comprehensive dataset.

### Neo4j Graph Database

All data is stored in a Neo4j graph database, enabling complex relationships between entities to be modeled and queried efficiently. This choice of database facilitates advanced features like graph-based recommendations and social graph analysis.

## API Overview

The system exposes RESTful endpoints for movie data access, user management, and dynamic recommendations. Authentication is managed via JWT tokens, and endpoints are designed with REST best practices in mind, ensuring ease of use and integration.

### Sample Endpoints

- **Get Movie by ID**: Retrieves detailed information about a movie, including cast, crew, and ratings.
- **User Registration and Authentication**: Manages user registration, login, and profile updates.
- **Bookmark Movies**: Allows users to bookmark their favorite movies, building a personalized watchlist.
- **Fetch Recommendations**: Generates personalized movie recommendations based on user preferences and interaction history.

## Getting Started

To get started with MoviesBE, follow the installation and setup instructions, then explore the API documentation for detailed endpoint descriptions and usage examples.
