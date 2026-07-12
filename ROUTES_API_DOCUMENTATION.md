# Complete Routes API Documentation

## 📋 All Route Functions - Detailed Information

---

## 1️⃣ **GET /api/orders/pending**
### Get Pending Orders
**Purpose:** Retrieve all orders that are waiting to be assigned to routes

| Aspect | Details |
|--------|---------|
| **HTTP Method** | GET |
| **Endpoint** | `/api/orders/pending` |
| **Parameters** | None |
| **Request Body** | None |
| **Returns** | `List<OrderDto>` |
| **Status Code** | 200 OK |

**OrderDto Properties:**
```
- OrderId (int)
- CustomerId (int)
- OriginAddress (string)
- DestinationAddress (string)
- Weight (decimal)
- Volume (decimal)
- RequiredDate (DateTimeOffset)
- Price (decimal)
- Status (OrderStatus) → Should be 0 (Pending)
- DeliveryImageUrl (string?)
```

**Example Response:**
```json
[
  {
    "orderId": 1,
    "customerId": 5,
    "originAddress": "123 Main St, City A",
    "destinationAddress": "456 Oak Ave, City B",
    "weight": 2.5,
    "volume": 0.5,
    "requiredDate": "2024-01-20T10:00:00Z",
    "price": 29.99,
    "status": 0,
    "deliveryImageUrl": null
  }
]
```

---

## 2️⃣ **POST /api/routes/batch-optimize**
### Batch Optimize Routes
**Purpose:** Optimize ALL pending orders for a specific date into multiple routes

| Aspect | Details |
|--------|---------|
| **HTTP Method** | POST |
| **Endpoint** | `/api/routes/batch-optimize` |
| **Parameters** | None |
| **Request Body** | `BatchOptimizeRoutesCommand` |
| **Returns** | `List<int>` (Route IDs) |
| **Status Code** | 200 OK |

**Request Body Parameters:**
```json
{
  "date": "2024-01-20"
}
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `date` | DateOnly | ✅ Yes | The date to optimize routes for (format: YYYY-MM-DD) |

**What It Does:**
1. Fetches ALL pending orders with RequiredDate on the specified date
2. Loads all available vehicles and couriers
3. Finds couriers NOT already assigned to that date
4. Calls the optimization service to calculate best routes
5. Creates multiple routes (one per courier)
6. Adds pickup/delivery stops with estimated times
7. Marks orders as "Assigned" (status: 1)

**Returns:** List of created Route IDs
```json
[1, 2, 3]
```

---

## 3️⃣ **POST /api/routes/single-optimize** (or `/api/routes/optimize`)
### Single Route Optimize
**Purpose:** Optimize specific orders into ONE route

| Aspect | Details |
|--------|---------|
| **HTTP Method** | POST |
| **Endpoint** | `/api/routes/single-optimize` |
| **Parameters** | None |
| **Request Body** | `OptimizeRouteCommand` |
| **Returns** | `int` (Route ID) |
| **Status Code** | 200 OK |

**Request Body Parameters:**
```json
{
  "orderIds": [1, 2, 3],
  "date": "2024-01-20"
}
```

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `orderIds` | List<int> | ✅ Yes | List of order IDs to optimize |
| `date` | DateOnly | ✅ Yes | The date for the route (format: YYYY-MM-DD) |

**What It Does:**
1. Fetches the specified orders (must have RequiredDate on that date)
2. Loads available vehicles and couriers
3. Finds ONE available courier for that date
4. Calls optimization service with single vehicle/courier pair
5. Creates ONE route with the optimized order sequence
6. Adds pickup/delivery stops with ETAs
7. Marks orders as "Assigned"

**Returns:** The created Route ID
```json
5
```

**Validation:**
- All orders must have their `RequiredDate` matching the route date
- Must have at least one available vehicle
- Must have at least one available courier not already assigned that date

---

## 4️⃣ **GET /api/routes/date/{date}**
### Get Routes by Date
**Purpose:** Retrieve all routes created for a specific date

| Aspect | Details |
|--------|---------|
| **HTTP Method** | GET |
| **Endpoint** | `/api/routes/date/{date}` |
| **Parameters** | `date` (path) - format: YYYY-MM-DD |
| **Request Body** | None |
| **Returns** | `List<RouteDto>` |
| **Status Code** | 200 OK |

**Path Parameter:**
```
{date} = "2024-01-20"
```

**RouteDto Properties:**
```
- RouteId (int)
- CourierId (int)
- VehicleId (int?)
- Date (DateOnly)
- StartTime (TimeOnly)
- EndTime (TimeOnly)
- Items (List<RouteItemDto>)
  - RouteItemId (int)
  - OrderId (int)
  - StopOrder (int) → Sequence order
  - StopType (StopType) → Pickup (0) or Delivery (1)
  - EstimatedArrival (DateTimeOffset?)
```

**Example Response:**
```json
[
  {
    "routeId": 5,
    "courierId": 3,
    "vehicleId": 1,
    "date": "2024-01-20",
    "startTime": "09:00:00",
    "endTime": "17:30:00",
    "items": [
      {
        "routeItemId": 10,
        "orderId": 1,
        "stopOrder": 1,
        "stopType": 0,
        "estimatedArrival": "2024-01-20T09:15:00Z"
      },
      {
        "routeItemId": 11,
        "orderId": 1,
        "stopOrder": 2,
        "stopType": 1,
        "estimatedArrival": "2024-01-20T09:25:00Z"
      }
    ]
  }
]
```

---

## 5️⃣ **GET /api/routes/{routeId}**
### Get Route by ID
**Purpose:** Retrieve details of a specific route

| Aspect | Details |
|--------|---------|
| **HTTP Method** | GET |
| **Endpoint** | `/api/routes/{routeId}` |
| **Parameters** | `routeId` (path) - int |
| **Request Body** | None |
| **Returns** | `RouteDto` or `null` |
| **Status Code** | 200 OK or 404 Not Found |

**Path Parameter:**
```
{routeId} = 5
```

**Returns:** Complete RouteDto (same structure as #4)

---

## 6️⃣ **PATCH /api/routes/{routeId}/reoptimize**
### Reoptimize Existing Route
**Purpose:** Recalculate the optimization for an existing route (change stop order)

| Aspect | Details |
|--------|---------|
| **HTTP Method** | PATCH |
| **Endpoint** | `/api/routes/{routeId}/reoptimize` |
| **Parameters** | `routeId` (path) - int |
| **Request Body** | None (uses existing route data) |
| **Returns** | `int` (Route ID) |
| **Status Code** | 200 OK |

**What It Does:**
1. Loads the existing route with all its items
2. Extracts the orders from the route
3. Calls optimization service to recalculate the best sequence
4. Clears existing stops
5. Creates new stops based on optimized sequence
6. Updates estimated arrival times
7. Updates route start/end times

**Returns:** The same Route ID
```json
5
```

---

## 7️⃣ **GET /api/routes/{routeId}/map**
### Get Route Map with Google Maps Link
**Purpose:** Get a Google Maps navigation URL for the route and stop details

| Aspect | Details |
|--------|---------|
| **HTTP Method** | GET |
| **Endpoint** | `/api/routes/{routeId}/map` |
| **Parameters** | `routeId` (path) - int |
| **Request Body** | None |
| **Returns** | `RouteMapDto` |
| **Status Code** | 200 OK or 404 Not Found |

**RouteMapDto Properties:**
```
- NavigationUrl (string) → Google Maps URL
- Stops (List<RouteStopInfo>)
  - Address (string)
  - EstimatedArrival (DateTimeOffset?)
  - Lat (double?) → Latitude
  - Lng (double?) → Longitude
```

**Example Response:**
```json
{
  "navigationUrl": "https://www.google.com/maps/dir/123%20Main%20St%2C%20City%20A/456%20Oak%20Ave%2C%20City%20B/789%20Pine%20Rd%2C%20City%20C",
  "stops": [
    {
      "address": "123 Main St, City A",
      "estimatedArrival": "2024-01-20T09:15:00Z",
      "lat": 40.7128,
      "lng": -74.0060
    },
    {
      "address": "456 Oak Ave, City B",
      "estimatedArrival": "2024-01-20T10:30:00Z",
      "lat": 40.7489,
      "lng": -73.9680
    },
    {
      "address": "789 Pine Rd, City C",
      "estimatedArrival": "2024-01-20T11:45:00Z",
      "lat": 40.7614,
      "lng": -73.9776
    }
  ]
}
```

---

## 🔄 **BONUS: POST /api/routes/optimize** (Original)
### Original Batch Optimize (All Non-Delivered Orders)
**Purpose:** Optimize all non-delivered/non-cancelled orders for a specific date (legacy)

| Aspect | Details |
|--------|---------|
| **HTTP Method** | POST |
| **Endpoint** | `/api/routes/optimize` |
| **Parameters** | None |
| **Request Body** | `OptimizeRoutesCommand` |
| **Returns** | `List<int>` (Route IDs) |
| **Status Code** | 200 OK |

**Request Body:**
```json
{
  "date": "2024-01-20"
}
```

**Differences from Batch Optimize:**
- Includes orders with status: Pending, Assigned (not InTransit, Delivered, Cancelled)
- More flexible for re-routing

---

---

# 🎯 **RECOMMENDED TEST FLOW - Step by Step**

## **Complete Testing Sequence to Get Google Maps Link:**

### **Step 1: Check Pending Orders** ✅
```
GET /api/orders/pending
```
**Goal:** Verify pending orders exist and note their order IDs and required dates
**Expected:** List of pending orders

---

### **Step 2: Create Routes (Choose ONE option)** 🚀

#### **Option A: Batch Optimize (Recommended for Testing)**
```
POST /api/routes/batch-optimize
{
  "date": "2024-01-20"
}
```
**Goal:** Create all possible routes for that date
**Expected:** `[1, 2, 3]` - list of Route IDs

**OR**

#### **Option B: Single Route Optimize**
```
POST /api/routes/single-optimize
{
  "orderIds": [1, 2, 3],
  "date": "2024-01-20"
}
```
**Goal:** Create ONE specific route with selected orders
**Expected:** `5` - a single Route ID

---

### **Step 3: Verify Routes Were Created** ✅
```
GET /api/routes/date/2024-01-20
```
**Goal:** Confirm routes were created and have items/stops
**Expected:** Routes with populated Items array (Pickup + Delivery stops)

---

### **Step 4: Get Google Maps Link** 🗺️
```
GET /api/routes/5/map
```
(Replace `5` with actual Route ID from Step 2)

**Goal:** Get the Google Maps navigation URL
**Expected:** 
```json
{
  "navigationUrl": "https://www.google.com/maps/dir/...",
  "stops": [...]
}
```

**Copy the `navigationUrl` and paste it in your browser to see the visual route!**

---

### **Step 5 (Optional): Reoptimize Route** 🔄
```
PATCH /api/routes/5/reoptimize
```
**Goal:** Change the stop order based on new optimization
**Expected:** Same route ID with updated stops order

---

### **Step 6: View Updated Map** 🗺️
```
GET /api/routes/5/map
```
**Goal:** Verify the new optimized route on Google Maps
**Expected:** Updated NavigationUrl with potentially different sequence

---

## 📊 **Order Status Codes:**
| Status | Value | Meaning |
|--------|-------|---------|
| Pending | 0 | Waiting for assignment |
| Assigned | 1 | Assigned to a route |
| InTransit | 2 | Currently being delivered |
| Delivered | 3 | Successfully delivered |
| Cancelled | 4 | Order cancelled |

---

## ⚠️ **Important Notes:**

1. **Date Format:** Always use `YYYY-MM-DD` (e.g., `2024-01-20`)
2. **RequiredDate Matters:** Routes only include orders with RequiredDate matching the route date
3. **Courier Availability:** Couriers can't have multiple routes on same date
4. **Vehicle Required:** Routes need available vehicles
5. **Service Time:** Each order has a ServiceTimeMinutes property (default: 10 min)
6. **Stops Order:** Each order creates TWO stops: Pickup then Delivery
7. **Google Maps Link:** Works best when you click and open it in a new browser tab

---

## 🔍 **Quick Reference Table:**

| Function | Use Case | Input | Output |
|----------|----------|-------|--------|
| Get Pending Orders | Find orders to route | - | Order list |
| Batch Optimize | Route all pending | Date | Route IDs |
| Single Optimize | Route specific orders | OrderIDs, Date | Route ID |
| Get Routes by Date | View all routes for date | Date | Routes with stops |
| Get Route by ID | View specific route | RouteID | Route details |
| Reoptimize Route | Change stop order | RouteID | Route ID |
| Get Route Map | View on Google Maps | RouteID | Map URL + stops |

---

**Ready to test? Start with Step 1!** 🚀
