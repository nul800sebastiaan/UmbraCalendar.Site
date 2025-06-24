# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

UmbraCalendar is an Umbraco CMS-based calendar application that aggregates and displays event information from Meetup.com. The project consists of:

- **Backend**: ASP.NET Core 8.0 Umbraco CMS application with custom services for Meetup integration
- **Frontend**: Astro-based client application for presentation
- **Data**: CosmosDB for event storage, SQLite for CMS data

## Architecture

### Core Components

- **Umbraco CMS**: Content management system with custom document types for events and pages
- **Meetup Integration**: GraphQL API client for fetching Meetup events via authorized services
- **CosmosDB Service**: Data persistence layer for event storage and caching
- **Hangfire**: Background job processing for scheduled event imports
- **Delivery API**: Umbraco's delivery API for headless content delivery
- **Astro Client**: Static site generator consuming the delivery API

### Key Services

- `MeetupService`: Handles Meetup API integration and data import
- `CosmosService`: Manages CosmosDB operations with caching
- `UpcomingMeetupService`: Processes upcoming events
- `RegisterServices`: Dependency injection configuration

### Content Types

- CalendarEvent: Individual event entries
- Events: Event listing pages  
- MeetupAddInfo: Additional meetup information
- Home: Homepage content

## Development Commands

### .NET Backend
```bash
# Build the application
dotnet build

# Run the application
dotnet run

# Restore dotnet tools (includes Swagger CLI)
dotnet tool restore

# Generate Swagger documentation
dotnet swagger tofile --output "wwwroot/swagger/delivery-swagger.json" "bin/Debug/net8.0/UmbraCalendar.dll" delivery
```

### Astro Client (in Client/ directory)
```bash
# Install dependencies
npm install

# Development server
npm run dev

# Build for production
npm run build

# Preview build
npm run preview

# Generate API client from Delivery API
npm run generate
```

### Docker
```bash
# Build container
docker build -t umbracalendar .

# Run container
docker run -p 8080:8080 umbracalendar
```

## Configuration

### Database
- **CMS Database**: SQLite (development) - `umbracoDbDSN` in appsettings.json
- **Event Storage**: CosmosDB - configured via `ExternalServices` section

### External Services
- **Meetup API**: Configured via Umbraco.AuthorizedServices
- **CosmosDB**: Connection configured in `ExternalServices:CosmosDb`

### Environment Files
- `appsettings.json`: Base configuration
- `appsettings.Development.json`: Development overrides
- `appsettings.Production.json`: Production overrides
- `Client/.env.development`: Astro development environment

## Key Directories

- `/Calendar/`: Calendar-specific models
- `/Client/`: Astro frontend application
- `/CosmosDb/`: CosmosDB service implementation
- `/DeliveryApi/`: Custom delivery API extensions
- `/Feed/`: RSS feed functionality
- `/Jobs/`: Hangfire background jobs
- `/Meetup/`: Meetup API integration
- `/Views/`: Umbraco view templates
- `/uSync/`: Umbraco content synchronization

## API Integration

### Meetup API
- **Endpoint**: `/gql-ext` (migrated from `/gql` in February 2025)
- Uses GraphQL queries for event data
- Authorized via Umbraco.AuthorizedServices
- Imports events via background jobs
- **Key Schema Changes**: 
  - `image` → `featuredEventPhoto`
  - `tickets` → `rsvps` 
  - `user` → `member`

### Delivery API
- Enabled for headless content delivery
- TypeScript client auto-generated from OpenAPI spec
- Consumed by Astro frontend

## Data Flow

1. **Import**: Hangfire jobs fetch events from Meetup API
2. **Storage**: Events stored in CosmosDB with caching
3. **Content**: Events managed via Umbraco backoffice
4. **Delivery**: Content exposed via Delivery API
5. **Frontend**: Astro client consumes API and generates static pages

## Testing and Linting

Currently no specific test or lint commands are configured. The project uses:
- TypeScript checking in Astro client: `astro check`
- .NET build validation: `dotnet build`

## Background Jobs

The application uses Hangfire for scheduled tasks:
- Event importing from Meetup API
- Data synchronization
- Cache management

Jobs are configured in `Jobs/Scheduler.cs` and executed by services in the `Meetup/` namespace.