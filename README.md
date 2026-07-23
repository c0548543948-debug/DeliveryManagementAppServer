# Delivery Management System — Server

ASP.NET Core Web API לניהול מערכת משלוחים. מספק REST API ו-SignalR לאפליקציית Angular.

## טכנולוגיות

- **ASP.NET Core** — Minimal APIs
- **Entity Framework Core** — גישה לבסיס נתונים
- **SQL Server** — בסיס הנתונים
- **ASP.NET Identity** — ניהול משתמשים והרשאות
- **JWT** — אימות (Authentication)
- **SignalR** — תקשורת בזמן אמת (מיקום שליח)
- **MediatR** — CQRS pattern (Commands & Queries)
- **AutoMapper** — המרת entities ל-DTOs
- **Google Cloud Route Optimization API** — אופטימיזציה של מסלולים

## מבנה הפרויקט

```
src/
├── Domain/          # Entities, Enums (Order, Route, Courier, Vehicle...)
├── Application/     # Commands, Queries, DTOs, Interfaces (CQRS)
├── Infrastructure/  # EF Core, Services (JWT, RouteOptimization, ...)
└── Web/             # Endpoints, Hubs, Program.cs
```

הפרויקט בנוי על ארכיטקטורת **Clean Architecture**:
- `Domain` לא תלוי בשום שכבה אחרת
- `Application` מגדיר interfaces, `Infrastructure` מממש אותם
- `Web` הוא נקודת הכניסה

## תפקידי משתמשים

| תפקיד | הרשאות |
|-------|--------|
| `Administrator` | ניהול כל המערכת — הזמנות, שליחים, רכבים, מסלולים |
| `Courier` | צפייה במסלולים שלו, עדכון מיקום, עדכון סטטוס עצירות |
| `Customer` | יצירת הזמנות, מעקב אחר הזמנה בזמן אמת |

## Endpoints עיקריים

### אימות
```
POST /api/Users/login          התחברות — מחזיר JWT
POST /api/Users/register       הרשמה
POST /api/Users/refresh        רענון טוקן
PATCH /api/Users/me            עדכון שם פרטי/משפחה
```

### הזמנות
```
GET    /api/Orders             רשימת הזמנות (עם פילטור תאריך/סטטוס)
POST   /api/Orders             יצירת הזמנה חדשה
PATCH  /api/Orders/{id}/status עדכון סטטוס הזמנה
POST   /api/Orders/validate-address   אימות כתובת
POST   /api/Orders/calculate-price    חישוב מחיר
```

### מסלולים
```
GET    /api/Routes             רשימת מסלולים
POST   /api/Routes/create-optimized   יצירת מסלול אופטימלי
POST   /api/Routes/{id}/start  התחלת מסלול (שליח)
PATCH  /api/Routes/{id}/current-stop  עדכון עצירה נוכחית
```

### מעקב
```
POST   /api/Tracking/{orderId}/location   שליחת מיקום GPS
GET    /api/Tracking/{orderId}/progress   התקדמות מסלול
```
SignalR Hub: `ws://localhost:PORT/hubs/tracking`

## הגדרת סביבה

צור קובץ `src/Web/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DeliveryManagementAppDb": "Server=(localdb)\\mssqllocaldb;Database=DeliveryManagementDb;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "Secret": "your-secret-key-at-least-32-characters",
    "ExpiryMinutes": 43200,
    "Issuer": "DeliveryManagementApp",
    "Audience": "DeliveryManagementApp"
  },
  "GoogleMaps": {
    "ApiKey": "YOUR_GOOGLE_CLOUD_API_KEY"
  }
}
```

> ללא `GoogleMaps:ApiKey` המערכת עובדת עם fallback — חלוקת הזמנות פשוטה ללא אופטימיזציה גיאוגרפית.

## הפעלה

### הפעלה מקומית

**דרישות מקדמיות:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server או [LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (מגיע עם Visual Studio)

```bash
dotnet run --project src/Web
```

בהפעלה ראשונה (Development) בסיס הנתונים נוצר אוטומטית ומאוכלס בנתוני ברירת מחדל:
- משתמש מנהל: `administrator@localhost` / `Administrator1!`

### הפעלה עם Docker

```bash
# בנייה
docker build -t delivery-server .

# הרצה
docker run -p 8080:8080 \
  -e ConnectionStrings__DeliveryManagementAppDb="Server=host.docker.internal;Database=DeliveryManagementDb;User Id=sa;Password=..." \
  -e JwtSettings__Secret="your-secret-key-at-least-32-characters" \
  -e JwtSettings__Issuer="DeliveryManagementApp" \
  -e JwtSettings__Audience="DeliveryManagementApp" \
  -e GoogleMaps__ApiKey="YOUR_GOOGLE_CLOUD_API_KEY" \
  delivery-server
```

האפליקציה תהיה זמינה על `http://localhost:8080`.

> משתני הסביבה (env variables) עוקפים את `appsettings.json` — מפריד בין שכבות הוא `__` (קו תחתון כפול) במקום `:`.
> לדוגמה: `JwtSettings:Secret` → `JwtSettings__Secret`.

## סטטוסי הזמנה

```
Pending → Assigned → InTransit → Delivered
                              ↘ Cancelled
```

- `Pending` — הוזמנה, טרם שובצה למסלול
- `Assigned` — שובצה למסלול
- `InTransit` — השליח יצא לדרך
- `Delivered` — נמסרה ללקוח
