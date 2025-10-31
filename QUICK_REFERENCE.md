# MAP2ADAMOINT - Quick Reference Card

**For Your Demo Tomorrow** 🎯

---

## 🚀 Start Command

```bash
docker-compose up -d
```

---

## 📡 5 Key Demo Endpoints

```bash
# 1. Health Check
curl http://localhost:8085/health

# 2. PostgreSQL Connectivity
curl http://localhost:8085/debug/test-postgres

# 3. Oracle Connectivity (shows real data ✓)
curl http://localhost:8085/debug/test-oracle

# 4. Lookup Real Molecule from Oracle
curl http://localhost:8085/adamo/initial/gr/GR-50-0789-0

# 5. End-to-End Transformation
curl -X POST http://localhost:8085/transform/odorfamily/adamo-to-map/1
```

---

## 📊 Project Stats

- **31 Endpoints** operational
- **14 Entity Models** (8 ADAMO + 6 MAP Tool)
- **Both Databases** connected with real data
- **3 Integration Scenarios** ready
- **Zero Hardcoded Credentials** (all in appsettings.json)

---

## 🎯 Three Integration Options

1. **Generic Transformation** - Tools send JSON, we transform
2. **End-to-End** - We fetch, transform, write
3. **Bulk Migration** - One-time mass transfer (6 entity types)

---

## 📁 Key Files

- `docs/DEMO_READY.md` - Full demo script
- `docs/ALL_ENDPOINTS.md` - All 31 endpoints
- `docs/FINAL_SUMMARY.md` - Complete summary

---

## ✅ Verified Working

✓ Oracle: CONNECTED (5,824 sessions found)  
✓ PostgreSQL: CONNECTED  
✓ GR-50-0789-0: Real data returned  
✓ Transformations: Working

---

**GitHub:** https://github.com/Umanova-doo/maptool-adamo-middleware-api

**Good luck! 🚀**
