# EventPulse - Multi-Vendor Event Ticketing Platform

## Overview

EventPulse is a full-stack multi-vendor event ticketing platform that enables organizers to create and manage events while allowing attendees to browse, book, and manage tickets online. The platform provides secure authentication, QR-code-based ticketing, real-time seat updates, payment processing through Stripe, and analytics dashboards for organizers and administrators.

The project is built using a modern N-Tier Architecture to ensure scalability, maintainability, and separation of concerns.

---

## Features

### Attendee Features

* User Registration and Login
* JWT Authentication
* Browse and Search Events
* Event Filtering by Category, Date, and Location
* Event Details View
* Ticket Booking
* Stripe Payment Integration
* QR Code Ticket Generation
* My Tickets Portal
* Download Digital Tickets
* Real-Time Seat Availability Updates

### Organizer Features

* Create Events
* Edit Events
* Delete Events
* Upload Event Images
* View Organizer Dashboard
* Sales Analytics
* Revenue Tracking
* Attendee Management
* Event Check-In System

### Admin Features

* Manage Users
* Manage Categories
* Manage Events
* Platform Monitoring
* User Role Management
* Dashboard Analytics

---

## Technology Stack

### Frontend

* Angular 21
* TypeScript
* RxJS
* Bootstrap

### Backend

* .NET 10 Web API
* ASP.NET Core
* Entity Framework Core
* AutoMapper
* SignalR
* IMemoryCache

### Database

* PostgreSQL

### Authentication & Security

* JWT Access Tokens
* Refresh Tokens
* Role-Based Authorization

### Payment Processing

* Stripe Payment Gateway

### Other Libraries

* QR Code Generation
* SignalR Real-Time Communication

---

## System Architecture

The application follows an N-Tier Architecture:

```text
Client (Angular)
        │
        ▼
ASP.NET Core Web API
        │
        ▼
Business Logic Layer (BLL)
        │
        ▼
Data Access Layer (DAL)
        │
        ▼
PostgreSQL Database
```

## User Roles

### Attendee

* Browse Events
* Purchase Tickets
* View Tickets
* Download QR Tickets

### Organizer

* Create Events
* Manage Own Events
* View Sales Analytics
* Check-In Attendees

### Admin

* Manage Users
* Manage Categories
* Manage Events
* View Platform Statistics

---

## Key Functionalities

### Event Management

Organizers can create and manage events with:

* Event Details
* Capacity
* Pricing
* Category Assignment
* Event Banner Upload

### Ticket Booking

The booking system:

* Prevents Overselling
* Handles Concurrent Requests
* Generates Unique Tickets
* Creates QR Codes

### Payment Integration

Stripe is used for:

* Secure Payment Processing
* Payment Verification
* Booking Confirmation

### QR Ticket Generation

Each successful booking receives:

* Unique Ticket ID
* QR Code
* Digital Ticket Record

### Real-Time Seat Updates

SignalR is used to:

* Broadcast Seat Availability Changes
* Improve User Experience
* Reduce Refresh Requirements

---

## Performance Optimizations

### In-Memory Caching

Implemented using IMemoryCache for:

* Event Details
* Event Listings

Benefits:

* Reduced Database Load
* Faster Response Times
* Improved Scalability

### Query Optimization

Optimizations include:

* AsNoTracking for Read-Only Queries
* Pagination
* Filtering at Database Level
* Projection Instead of Unnecessary Includes
* Reduced N+1 Query Problems

### Soft Delete

Implemented for:

* Users
* Events
* Categories

Benefits:

* Data Recovery
* Audit Support
* Historical Tracking

---

## Security Features

* JWT Authentication
* Refresh Token Support
* Role-Based Authorization
* Password Hashing and Salting
* Secure API Endpoints
* Input Validation
* Global Exception Handling

---

## Installation

### Prerequisites

* .NET 10 SDK
* Node.js
* Angular CLI
* PostgreSQL

### Clone Repository

```bash
git clone <repository-url>
cd EventPulse
```

### Backend Setup

```bash
cd EventPulse.API
dotnet restore
dotnet ef database update
dotnet run
```

### Frontend Setup

```bash
cd eventpulse-client
npm install
ng serve
```

### Database Configuration

Update the connection string in:

```json
appsettings.json
```

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=EventPulseDb;Username=postgres;Password=yourpassword"
  }
}
```

---

## API Highlights

### Authentication

* Register
* Login
* Refresh Token
* Logout

### Events

* Get Events
* Get Event By Id
* Create Event
* Update Event
* Delete Event

### Bookings

* Create Booking
* Payment Success
* My Tickets

### Dashboard

* Organizer Analytics
* Revenue Statistics
* Attendee Statistics

---

## Challenges Solved

### Over-Selling Prevention

Implemented validation to ensure ticket quantities never exceed event capacity.

### Real-Time Updates

SignalR keeps seat availability synchronized across users.

### Performance

Implemented caching and query optimization to reduce response times and database load.

### Security

Implemented JWT authentication with refresh token support and role-based authorization.

---

## Future Enhancements

* Event Reviews & Ratings
* Email Notifications
* Multi-Language Support
* Event Recommendations
* Mobile Application
* Advanced Analytics
* Distributed Caching (Redis)

---

## Project Summary

EventPulse is a scalable event management and ticketing platform designed using modern software engineering principles and N-Tier Architecture. The project demonstrates full-stack development skills including authentication, authorization, payment processing, real-time communication, QR code generation, caching, database optimization, and secure API development.

The application provides a complete ecosystem for attendees, organizers, and administrators while maintaining performance, scalability, and maintainability.

---
