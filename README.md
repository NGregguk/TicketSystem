# Ticketing System (ASP.NET Core MVC)

Internal IT ticketing system designed for Windows Server + IIS.

## Prereqs
- .NET 8 SDK
- SQL Server (Express is fine for dev)
- EF Core tooling (`dotnet tool install --global dotnet-ef`)
- Windows: IIS + ASP.NET Core Hosting Bundle (for deployment)

## Local setup
1. Update `ConnectionStrings:DefaultConnection` in `src/TicketingSystem/appsettings.json`.
2. Restore and apply migrations:
   ```bash
   dotnet restore
   dotnet ef database update --project src/TicketingSystem
   ```
3. Run the app:
   ```bash
   dotnet run --project src/TicketingSystem
   ```

## Seed users (Development only)
In Development, two users are created automatically:
- Admin: `admin@local.test` / `Admin123!`
- Requester: `requester@local.test` / `Requester123!`

Change these in `src/TicketingSystem/appsettings.json` under `SeedUsers`.

## Configuration
### SMTP
Configure SMTP in `appsettings.json`:
```
"Smtp": {
  "Host": "smtp.company.local",
  "Port": 587,
  "EnableSsl": true,
  "Username": "smtp-user",
  "Password": "smtp-password",
  "FromAddress": "it@company.com",
  "FromName": "IT Ticketing"
},
"Notifications": {
  "AdminMailbox": "it@company.com"
}
```
If SMTP is not configured, email is skipped with a warning log.

### Uploads
```
"Uploads": {
  "RootPath": "App_Data/Uploads",
  "MaxSizeBytes": 10485760,
  "AllowedExtensions": [".pdf", ".png", ".jpg", ".jpeg", ".txt", ".docx", ".xlsx"]
}
```

## IIS deployment (Windows Server)
1. Install IIS and the ASP.NET Core Hosting Bundle.
2. Publish:
   ```bash
   dotnet publish -c Release -o publish
   ```
3. Create an IIS site pointing to the `publish` folder.
4. App Pool: No Managed Code.
5. Ensure the app pool identity has write permissions to:
   - `App_Data/Uploads`
   - `logs`
6. HTTPS redirection is off by default. Enable if needed.
7. If behind a proxy/load balancer, enable forwarded headers in `Program.cs` and configure IIS accordingly.

## Roles & access
- Requester: create tickets, view own tickets, add public comments, close own tickets.
- Admin: view all tickets, assign, update status/priority/category, add internal notes, manage categories/users.

## Database
Migrations are included in `src/TicketingSystem/Migrations`.

## Notes
- Logs are written to console and `logs/log-*.txt`.
- Attachment uploads are stored on disk; metadata is stored in SQL Server.
