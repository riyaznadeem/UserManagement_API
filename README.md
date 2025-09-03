# UserManagement_API
The User Management API allows applications and administrators to programmatically manage users within a system. It provides endpoints to perform CRUD operations on user accounts, handle authentication and authorization, manage roles and permissions, and support account lifecycle activities such as registration, password resets, and deactivation.

Before Running the Project
1. Update the connection string in appsettings.json.
2. In Package Manager Console, select the Infrastructure project and run: "Update-Database"

After Successful Build or Migration
When Build or Update-Database runs successfully, three default users/roles are created automatically:
1. UserName : 'admin' , Password : '#itadmin'
2. UserName : 'user' , Password : '#itadmin'
3. UserName : 'read' , Password : '#itadmin'

User Management API Overview
Architecture & Design Principles
	•	Clean Architecture: Separation of concerns into layers:
	◦	Domain: Entities and business logic
	◦	Application: Use cases, commands, queries (CQRS), interfaces
	◦	Infrastructure: Data access, external services, implementations
	◦	API: Controllers, middleware, HTTP handling
	•	CQRS Pattern: Separate Commands (writes) from Queries (reads)
	•	Mediator Pattern: Using MediatR to decouple request handling
	•	Service-Based Design: Business logic encapsulated in services, injected via DI
Authentication & Authorization
	•	JWT-Based Authentication:
	◦	Users authenticate with JWT tokens signed with symmetric keys
	◦	Tokens include user roles for authorization
	•	Role-Based Access Control (RBAC):
	◦	API endpoints secured based on user roles (e.g., Admin, User)
	◦	Authorization middleware enforces permissions on endpoints
	•	Exception Handling:
	◦	Centralized middleware captures exceptions, returns structured error responses
	◦	Unauthorized or forbidden access returns appropriate HTTP status codes (401, 403)
User Management (CRUD)
	•	Create, Read, Update, Delete Users
	◦	Controller endpoints expose user management APIs secured by roles
	◦	User creation and update validate inputs, hash passwords securely
	◦	Uses DTOs and ViewModels to control exposed data
	•	Password Security:
	◦	Passwords hashed with salted hashing (e.g., ASP.NET Core Identity’s PasswordHasher)
	◦	Stored securely in the database (never plain text)
Audit Logging & IP Caching
	•	Audit Logs:
	◦	Track user actions like login, user updates, deletions
	◦	Logs include timestamps, user ID, action type, and IP address
	◦	Stored in database or external logging system
	•	IP Caching:
	◦	Cache IP addresses of users for security analysis or throttling
	◦	Can be implemented using in-memory cache or distributed cache like Redis
Dependency Injection (DI)
	•	All services, repositories, and DbContexts are registered via DI container
	•	Promotes testability and loose coupling
	•	Example: IUserService, IApplicationDbContext, IMediator injected into controllers or handlers
Technologies & Tools
	•	.NET 8
	•	Entity Framework Core for database access
	•	MediatR for CQRS and mediator pattern
	•	JWT Authentication via Microsoft.AspNetCore.Authentication.JwtBearer
	•	Swagger/OpenAPI for API documentation
	•	Localization support with ASP.NET Core localization
	•	Middleware for exception handling and request localization

Sample Flow for Get Users (Paginated)
	1.	API Controller receives GET /api/users with query params for pagination.
	2.	Controller creates GetUsersListQuery and sends it to IMediator.
	3.	GetUsersListQueryHandler queries database with EF Core including filtering and pagination.
	4.	Returns paginated PaginatedList<GetUserListViewModel> to controller.
	5.	Controller returns JSON response with user list.

Security Highlights
	•	Passwords are hashed and salted.
	•	JWT tokens carry claims and roles for fine-grained access.
	•	Audit logs capture who did what and when, including IP.
	•	Role-based authorization enforced at controller/action level.
	

