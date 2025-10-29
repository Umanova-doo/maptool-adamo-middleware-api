# Docker Database Connectivity Issue

## 🚨 Current Problem

Docker container cannot connect to your localhost databases (5433 and 4040).

```
✗ PostgreSQL: Failed to connect to 127.0.0.1:5433
✗ Oracle: Failed to connect to server
```

---

## 🔧 Solutions

### Option 1: Run API Locally (NOT in Docker) - EASIEST

**Since your databases are on localhost, run the API locally:**

```bash
# Stop Docker
docker-compose down

# Run locally
dotnet run
```

**Then test:**
```bash
curl http://localhost:8085/debug/test-postgres
curl http://localhost:8085/debug/test-oracle
```

This will use `appsettings.json` with direct `localhost:5433` and `localhost:4040` access.

---

### Option 2: Fix Docker Networking

#### Check 1: Are databases actually running?

```powershell
# Test PostgreSQL
Test-NetConnection -ComputerName localhost -Port 5433

# Test Oracle  
Test-NetConnection -ComputerName localhost -Port 4040
```

**Expected:** TcpTestSucceeded : True

#### Check 2: Windows Firewall

Make sure Windows Firewall allows Docker to access localhost ports:

```powershell
# Run as Administrator
New-NetFirewallRule -DisplayName "Allow Docker to PostgreSQL" -Direction Inbound -LocalPort 5433 -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "Allow Docker to Oracle" -Direction Inbound -LocalPort 4040 -Protocol TCP -Action Allow
```

#### Check 3: Database Binding

**PostgreSQL** must be listening on `0.0.0.0` (not just `127.0.0.1`):

Check `postgresql.conf`:
```
listen_addresses = '*'  # or '0.0.0.0'
```

**Oracle** must allow external connections.

#### Check 4: Use Host IP Instead

Find your machine's IP:
```powershell
ipconfig | findstr IPv4
```

Then update `appsettings.Docker.json`:
```json
{
  "ConnectionStrings": {
    "MapToolDb": "Host=192.168.1.XXX;Port=5433;...",
    "AdamoDb": "...Data Source=192.168.1.XXX:4040/FREEPDB1"
  }
}
```

---

### Option 3: Use Docker's Host Network Mode (Linux Only)

**Not available on Windows Docker Desktop** - skip this option.

---

## 🎯 Recommended Approach

### For Development: Run Locally

```bash
dotnet run
```

**Benefits:**
- ✅ Direct access to localhost:5433 and localhost:4040
- ✅ No Docker networking issues
- ✅ Easier debugging
- ✅ Faster iteration

### For Production/Server Deployment: Use Docker

When deploying to a server where databases are on different machines:

```json
{
  "ConnectionStrings": {
    "MapToolDb": "Host=postgres-server.company.com;Port=5432;...",
    "AdamoDb": "...Data Source=oracle-server.company.com:1521/ORCL"
  }
}
```

---

## 🧪 Quick Test

### Test 1: Run Locally

```bash
# Stop Docker
docker-compose down

# Run locally  
dotnet run

# Wait 3 seconds, then test
curl http://localhost:8085/debug/test-postgres
curl http://localhost:8085/debug/test-oracle
```

### Test 2: Check Database Accessibility

```powershell
# PostgreSQL
psql -h localhost -p 5433 -U postgres -d MAP23

# Oracle
sqlplus GIV_MAP/MapPassword123@localhost:4040/FREEPDB1
```

If these work, databases are running and accessible.

---

## 📋 Troubleshooting Checklist

- [ ] Are PostgreSQL and Oracle actually running on localhost?
- [ ] Can you connect to them manually (psql, sqlplus)?
- [ ] Are they listening on ports 5433 and 4040?
- [ ] Is Windows Firewall blocking Docker?
- [ ] Try running API locally with `dotnet run` instead of Docker

---

## 💡 **Recommendation**

**For your setup (databases on localhost), use:**

```bash
dotnet run
```

**NOT:**
```bash
docker-compose up
```

Docker is best when databases are on remote servers, not localhost.

---

**Try `dotnet run` and test the debug endpoints!** 🚀

