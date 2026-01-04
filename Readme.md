# Applicant Tracking System (ATS) - .NET 8.0 Web API

A comprehensive Applicant Tracking System built with ASP.NET Core 8.0, integrating with OpenCATS, Apideck, Knit, and Merge.dev for seamless recruitment workflow management.

## Features

### Epic 1: Job Posting Management
- ✅ Create, edit, and delete job postings
- ✅ View all job postings with filtering
- ✅ Automatic synchronization with OpenCATS
- ✅ Job status management (Draft, Open, Closed, Archived)

### Epic 2: Automated Shortlisting
- ✅ View applicants by job posting
- ✅ Filter by education level
- ✅ Rank candidates by years of experience
- ✅ Automatic shortlisting based on criteria
- ✅ View shortlisted candidates

### Epic 3: Admin Dashboard
- ✅ Comprehensive analytics (jobs, applicants, shortlist stats)
- ✅ User management (create, update, activate/deactivate)
- ✅ Export reports in CSV format
- ✅ Real-time recruitment metrics

### Epic 4: Feedback & Notifications
- ✅ Update applicant status with history tracking
- ✅ Automated email notifications
- ✅ Configurable notification settings
- ✅ Status change tracking

## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server with Entity Framework Core
- **External Integrations**:
  - **OpenCATS**: Open-source ATS for job order management
  - **Apideck**: Unified API for HR integrations
  - **Knit**: API integration platform
  - **Merge.dev**: Unified ATS API for candidate management

## Project Structure

```
ATS.API/
├── Controllers/
│   ├── JobPostingsController.cs
│   ├── ApplicantsController.cs
│   └── AdminController.cs
├── Models/
│   └── Models.cs (JobPosting, Applicant, Application, User)
├── DTOs/
│   └── DTOs.cs (Data Transfer Objects)
├── Services/
│   ├── JobPostingService.cs
│   ├── ApplicantService.cs
│   ├── ShortlistingService.cs
│   ├── AnalyticsService.cs
│   ├── NotificationService.cs
│   ├── UserService.cs
│   └── External API Clients (OpenCATS, Apideck, Knit, Merge)
├── Data/
│   └── AtsDbContext.cs
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
├── Program.cs
└── appsettings.json
```

## Setup Instructions

### Prerequisites
- .NET 8.0 SDK
- SQL Server
- SMTP server credentials (for email notifications)
- API keys for external integrations

### Installation

1. **Clone the repository**
```bash
git clone <repository-url>
cd ATS.API
```

2. **Configure appsettings.json**
Update the following sections:
- Connection string for SQL Server
- External API credentials (OpenCATS, Apideck, Knit, Merge)
- SMTP settings for email notifications

3. **Apply database migrations**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

4. **Run the application**
```bash
dotnet run
```

The API will be available at `https://localhost:5001` (or the configured port).

## API Endpoints

### Job Postings
- `POST /api/jobpostings` - Create job posting
- `GET /api/jobpostings` - Get all job postings
- `GET /api/jobpostings/{id}` - Get job posting by ID
- `PUT /api/jobpostings/{id}` - Update job posting
- `DELETE /api/jobpostings/{id}` - Delete job posting
- `POST /api/jobpostings/{id}/close` - Close job posting
- `POST /api/jobpostings/sync` - Sync with OpenCATS

### Applicants
- `POST /api/applicants/apply` - Submit application
- `GET /api/applicants/job/{jobPostingId}` - Get applicants by job
- `GET /api/applicants/job/{jobPostingId}/filter/education` - Filter by education
- `GET /api/applicants/job/{jobPostingId}/rank` - Rank by experience
- `GET /api/applicants/job/{jobPostingId}/shortlisted` - Get shortlisted applicants
- `POST /api/applicants/job/{jobPostingId}/auto-shortlist` - Auto-shortlist candidates
- `PATCH /api/applicants/{applicationId}/status` - Update application status
- `POST /api/applicants/import` - Import candidates from external ATS

### Admin
- `GET /api/admin/analytics` - Get recruitment analytics
- `GET /api/admin/analytics/jobs` - Get job analytics
- `GET /api/admin/analytics/applicants` - Get applicant analytics
- `GET /api/admin/analytics/shortlist` - Get shortlist analytics
- `GET /api/admin/users` - Get all users
- `POST /api/admin/users` - Create user
- `PUT /api/admin/users/{id}` - Update user
- `DELETE /api/admin/users/{id}` - Delete user
- `POST /api/admin/users/{id}/activate` - Activate user
- `POST /api/admin/users/{id}/deactivate` - Deactivate user
- `GET /api/admin/export/applications` - Export applications
- `GET /api/admin/export/shortlist` - Export shortlist
- `GET /api/admin/notifications/settings/{userId}` - Get notification settings
- `PUT /api/admin/notifications/settings/{userId}` - Update notification settings

## External Integrations

### OpenCATS Integration
OpenCATS is an open-source ATS that provides:
- Job order management
- Candidate tracking
- Resume parsing

**Configuration**: Set `ExternalApis:OpenCats:BaseUrl` in appsettings.json

### Apideck Integration
Apideck provides unified API access to multiple HR systems:
- ATS integrations
- HRIS systems
- Background check services

**Configuration**: Set API key and consumer ID in appsettings.json

### Knit Integration
Knit enables integration with various recruitment platforms:
- Multi-platform candidate sourcing
- Unified data synchronization

**Configuration**: Set API key in appsettings.json

### Merge.dev Integration
Merge.dev provides unified ATS API access:
- Candidate management
- Application tracking
- Status synchronization across platforms

**Configuration**: Set API key in appsettings.json

## Email Notifications

The system sends automated emails for:
- Application confirmation
- Status updates
- Shortlist notifications

Configure SMTP settings in `appsettings.json` under the `Smtp` section.

## Database Schema

### Main Tables
- **JobPostings**: Stores job vacancy information
- **Applicants**: Stores candidate information
- **Applications**: Links applicants to job postings
- **ApplicationStatusHistory**: Tracks status changes
- **Users**: HR users and administrators
- **NotificationSettings**: User notification preferences

## User Stories Mapping

| User Story | Endpoint | Feature |
|------------|----------|---------|
| US-1.1 | POST /api/jobpostings | Create job posting |
| US-1.2 | PUT /api/jobpostings/{id} | Edit job posting |
| US-1.3 | DELETE /api/jobpostings/{id} | Delete job posting |
| US-1.4 | GET /api/jobpostings | View all job postings |
| US-2.1 | GET /api/applicants/job/{id} | View applicants |
| US-2.2 | GET /api/applicants/job/{id}/filter/education | Filter by education |
| US-2.3 | GET /api/applicants/job/{id}/rank | Rank by experience |
| US-2.4 | GET /api/applicants/job/{id}/shortlisted | View shortlisted |
| US-3.1 | GET /api/admin/analytics | View analytics |
| US-3.2 | GET /api/admin/users | Manage users |
| US-3.3 | GET /api/admin/export/* | Export reports |
| US-4.1 | PATCH /api/applicants/{id}/status | Update status |
| US-4.2 | (Automatic) | Email notifications |
| US-4.3 | PUT /api/admin/notifications/settings/{id} | Manage notifications |

## Development

### Running Tests
```bash
dotnet test
```

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Rollback migration
dotnet ef database update PreviousMigrationName
```

### Building for Production
```bash
dotnet publish -c Release -o ./publish
```

## Security Considerations

- Implement authentication and authorization (JWT, Identity)
- Validate all input data
- Sanitize user-generated content
- Use HTTPS in production
- Implement rate limiting
- Secure API keys and credentials

## Future Enhancements

- Authentication & authorization (JWT tokens)
- Role-based access control (RBAC)
- Resume parsing with AI/ML
- Interview scheduling
- Video interview integration
- Advanced analytics and reporting
- Mobile application
- Multi-language support

## License

[Your License Here]

## Support

For issues and questions, please contact [support email or create an issue]
