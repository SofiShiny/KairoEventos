# Task 13 Verification Checklist

## Pre-Verification Steps

Before starting the infrastructure, ensure:

- [ ] Docker Desktop is running
- [ ] No services are using ports 5432, 27017, 5672, 8080, 8180, 15672
- [ ] `kairo-network` exists (or will be created by docker-compose)
- [ ] Gateway Dockerfile exists at `../Gateway/Dockerfile`
- [ ] Realm export exists at `./configs/keycloak/realm-export.json`

## Verification Steps

### 1. Validate Docker Compose Configuration

```bash
cd Infraestructura
docker-compose config --quiet
```

**Expected:** No errors (warnings about version are OK)

**Status:** ✅ PASSED

### 2. Start Infrastructure

```bash
docker-compose up -d
```

**Expected:** All services start without errors

**Check:**
- [ ] postgres starts first
- [ ] mongodb and rabbitmq start
- [ ] keycloak starts after postgres is healthy
- [ ] gateway starts after keycloak is healthy

### 3. Verify Service Status

```bash
docker-compose ps
```

**Expected Output:**
```
NAME                STATUS              PORTS
kairo-postgres      Up (healthy)        0.0.0.0:5432->5432/tcp
kairo-mongodb       Up (healthy)        0.0.0.0:27017->27017/tcp
kairo-rabbitmq      Up (healthy)        0.0.0.0:5672->5672/tcp, 0.0.0.0:15672->15672/tcp
kairo-keycloak      Up (healthy)        0.0.0.0:8180->8080/tcp
kairo-gateway       Up (healthy)        0.0.0.0:8080->8080/tcp
```

**Check:**
- [ ] All 5 services are running
- [ ] All services show "healthy" status
- [ ] Port mappings are correct

### 4. Verify PostgreSQL Databases

```bash
docker exec -it kairo-postgres psql -U postgres -c "\l"
```

**Expected:** Should list these databases:
- [ ] postgres (default)
- [ ] kairo_eventos
- [ ] kairo_asientos
- [ ] kairo_reportes
- [ ] keycloak ← NEW

### 5. Verify Keycloak Startup

```bash
docker logs kairo-keycloak | grep -i "import"
```

**Expected:** Should see realm import messages:
- [ ] "Importing realm from file"
- [ ] "Realm 'Kairo' imported"

### 6. Verify Keycloak Health

```bash
curl http://localhost:8180/health/ready
```

**Expected:** HTTP 200 OK with health status

**Check:**
- [ ] Returns 200 status code
- [ ] Response indicates healthy status

### 7. Verify Keycloak Admin Console

Open browser: http://localhost:8180

**Check:**
- [ ] Keycloak login page loads
- [ ] Can login with admin/admin
- [ ] Realm "Kairo" exists
- [ ] Clients "kairo-web" and "kairo-api" exist
- [ ] Roles "User", "Admin", "Organizator" exist
- [ ] Users "admin", "organizador", "usuario" exist

### 8. Verify Gateway Health

```bash
# Liveness check
curl http://localhost:8080/health/live

# Readiness check (includes Keycloak connectivity)
curl http://localhost:8080/health/ready
```

**Expected:** Both return HTTP 200 OK

**Check:**
- [ ] `/health/live` returns 200
- [ ] `/health/ready` returns 200
- [ ] Readiness check confirms Keycloak connectivity

### 9. Verify Gateway Logs

```bash
docker logs kairo-gateway
```

**Expected:** Should see:
- [ ] Application started successfully
- [ ] Listening on http://+:8080
- [ ] No authentication/authorization errors
- [ ] Successfully connected to Keycloak

### 10. Verify Network Connectivity

```bash
# Gateway can reach Keycloak
docker exec kairo-gateway curl -s http://keycloak:8080/health/ready

# Gateway can reach other services (if they were running)
docker exec kairo-gateway curl -s http://postgres:5432 || echo "Expected: connection refused (postgres doesn't serve HTTP)"
```

**Check:**
- [ ] Gateway can resolve "keycloak" hostname
- [ ] Gateway can reach Keycloak health endpoint

### 11. Verify Environment Variables

```bash
# Check Gateway environment
docker exec kairo-gateway printenv | grep -E "(Keycloak|Cors)"
```

**Expected:** Should see:
- [ ] `Keycloak__Authority=http://keycloak:8080/realms/Kairo`
- [ ] `Keycloak__Audience=kairo-api`
- [ ] `Keycloak__MetadataAddress=http://keycloak:8080/realms/Kairo/.well-known/openid-configuration`
- [ ] `Cors__AllowedOrigins__0=http://localhost:5173`
- [ ] `Cors__AllowedOrigins__1=http://localhost:3000`

### 12. Verify Volume Mounts

```bash
# Check Keycloak realm mount
docker exec kairo-keycloak ls -la /opt/keycloak/data/import/

# Check PostgreSQL init script
docker exec kairo-postgres ls -la /docker-entrypoint-initdb.d/
```

**Expected:**
- [ ] realm-export.json exists in Keycloak container
- [ ] init.sql exists in PostgreSQL container

### 13. Test Gateway Routes (Optional - requires microservices)

```bash
# This will fail if microservices aren't running, but tests routing
curl -v http://localhost:8080/api/eventos/health 2>&1 | grep "503"
```

**Expected:** 503 Service Unavailable (microservices not running yet)

**Check:**
- [ ] Gateway accepts the request
- [ ] Returns 503 (not 404 - proves routing works)

## Cleanup (Optional)

To stop and remove everything:

```bash
# Stop services
docker-compose down

# Remove volumes (⚠️ deletes all data)
docker-compose down -v
```

## Common Issues and Solutions

### Issue: Keycloak fails to start
**Solution:**
1. Check PostgreSQL is healthy: `docker-compose ps postgres`
2. Check logs: `docker logs kairo-keycloak`
3. Verify keycloak database exists: `docker exec -it kairo-postgres psql -U postgres -l`

### Issue: Gateway fails to start
**Solution:**
1. Check Keycloak is healthy: `curl http://localhost:8180/health/ready`
2. Check Gateway logs: `docker logs kairo-gateway`
3. Verify Gateway build succeeded: `docker-compose build gateway`

### Issue: Realm not imported
**Solution:**
1. Check file exists: `ls -la Infraestructura/configs/keycloak/realm-export.json`
2. Check mount: `docker exec kairo-keycloak ls -la /opt/keycloak/data/import/`
3. Check logs: `docker logs kairo-keycloak | grep -i import`
4. Restart Keycloak: `docker-compose restart keycloak`

### Issue: Port already in use
**Solution:**
1. Find process using port: `netstat -ano | findstr "8080"` (Windows)
2. Stop the process or change port mapping in docker-compose.yml

### Issue: Gateway can't reach Keycloak
**Solution:**
1. Verify both on same network: `docker network inspect kairo-network`
2. Check Keycloak is healthy: `docker-compose ps keycloak`
3. Test from Gateway: `docker exec kairo-gateway curl http://keycloak:8080/health/ready`

## Success Criteria

All checks must pass:

- [x] Docker compose configuration is valid
- [ ] All 5 services start successfully
- [ ] All services reach "healthy" status
- [ ] PostgreSQL has keycloak database
- [ ] Keycloak imports realm successfully
- [ ] Keycloak admin console is accessible
- [ ] Gateway health endpoints respond
- [ ] Gateway can connect to Keycloak
- [ ] Environment variables are set correctly
- [ ] Volume mounts are working

## Next Steps

Once all verification steps pass:

1. Proceed to **Task 14**: Create .env.example file
2. Document any issues encountered
3. Update README.md with new services
4. Prepare for integration testing (Task 16)

## Notes

- First startup may take 1-2 minutes for Keycloak to initialize
- Keycloak health check has 60s start period to allow initialization
- Gateway waits for Keycloak to be healthy before starting
- All services use development credentials - change for production
