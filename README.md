Therapy Center
A full-stack web application for managing a therapy clinic — appointments, patients, doctors, payments, and clinical findings.

Tech Stack
Layer	Technology
Frontend	React 18 + Vite 6 + React Router v6
Backend	ASP.NET Core 10 Web API
Database	MySQL (via EF Core / Pomelo)
Auth	JWT Bearer tokens with role-based policies
Tests	xUnit 
Roles
Admin — manages therapies, doctors, staff, slots, patients, appointments
Receptionist — registers patients, books appointments, views schedules
Doctor — views own appointments, writes clinical findings
Patient / Guardian — self-registers, books appointments online
Quick Start
1. Database
Ensure MySQL is running. Update the connection string in:


therapy-center-backend/TherapyCenter/appsettings.json
2. Backend

cd therapy-center-backend/TherapyCenter
dotnet run --launch-profile https
Runs at https://localhost:7226

3. Frontend

cd therapy-center-frontend
npm install
npm run dev
Runs at http://localhost:5173 — proxies /api to the backend.

4. Tests

dotnet test therapy-center-tests/TherapyCenter.Tests.csproj
Project Structure

therapy-center-backend/          # .NET Web API
  Controllers/                   # API endpoints (7 controllers)
  Services/                      # Business logic layer
  Repositories/                  # Data access layer
  Entities/                      # Database models (8 entities)
  Data/AppDbContext.cs           # EF Core context

therapy-center-frontend/         # React + Vite
  src/
    pages/                       # admin/, doctor/, patient/, staff/
    components/                  # Sidebar, ProtectedRoute, Modal
    context/AuthContext.jsx      # JWT auth state
    api/api.js                   # HTTP client

therapy-center-tests/            # xUnit tests (77 tests)
  Services/                      # One test file per service
API Overview
Controller	Endpoints
/api/auth	register, login, create-staff
/api/admin	therapies CRUD, doctors, staff, slots
/api/appointment	book, book-online, status, queries
/api/doctor	profiles, slots, appointments, findings
/api/patient	CRUD, guardian queries, self-profile
/api/payment	record, mark-paid, queries
/api/therapy	list all therapies