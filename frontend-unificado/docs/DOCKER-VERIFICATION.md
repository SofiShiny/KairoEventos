# Docker Verification Tests

This document describes the Docker verification tests for the Frontend Unificado application.

## Overview

The Docker verification tests ensure that:
1. The Docker image builds successfully
2. The image size is reasonable (<100MB)
3. Nginx serves files correctly
4. SPA routing works properly
5. Security headers are configured
6. Gzip compression is enabled

## Requirements Validated

- **Requirement 19.1**: Docker image builds correctly with multi-stage build
- **Requirement 19.6**: Image size is minimized (<100MB)

## Test Scripts

### 1. Shell Script (Linux/Mac)

```bash
bash docker-verify.test.sh
```

**Features:**
- Builds Docker image
- Checks image size
- Starts container
- Tests nginx functionality
- Verifies SPA routing
- Checks security headers
- Validates gzip compression
- Automatic cleanup

### 2. PowerShell Script (Windows)

```powershell
pwsh docker-verify.test.ps1
```

**Features:**
- Same as shell script but for Windows
- Uses PowerShell cmdlets
- Color-coded output
- Automatic cleanup

### 3. Vitest Integration Tests

```bash
npm run test:docker
```

**Features:**
- Integrated with Vitest test suite
- Can be run as part of CI/CD
- Programmatic verification
- Detailed assertions

## Test Cases

### Image Build Tests

1. **Build Success**: Verifies Docker image builds without errors
2. **Image Size**: Ensures image is <100MB
3. **Multi-stage Build**: Confirms Dockerfile uses multi-stage pattern
4. **Nginx Base**: Validates nginx:alpine is used

### Container Runtime Tests

5. **Container Start**: Verifies container starts successfully
6. **Health Check**: Confirms health check passes
7. **Port Exposure**: Validates port 80 is exposed
8. **No Errors**: Checks container logs for errors

### Nginx Functionality Tests

9. **File Serving**: Confirms nginx serves files (HTTP 200)
10. **Index.html**: Validates index.html is served correctly
11. **SPA Routing**: Tests that non-existent routes serve index.html
12. **Static Assets**: Verifies static files are served

### Performance Tests

13. **Gzip Compression**: Confirms gzip is enabled
14. **Cache Headers**: Validates cache headers for static assets
15. **No Cache for HTML**: Ensures index.html is not cached

### Security Tests

16. **X-Frame-Options**: Validates header is present
17. **X-Content-Type-Options**: Confirms nosniff is set
18. **X-XSS-Protection**: Checks XSS protection header
19. **Server Tokens**: Ensures server tokens are disabled
20. **Referrer Policy**: Validates referrer policy header

## Running Tests

### Prerequisites

- Docker installed and running
- Node.js 18+ (for Vitest tests)
- Bash or PowerShell (for script tests)

### Quick Start

```bash
# Build and verify using shell script
bash docker-verify.test.sh

# Or using PowerShell
pwsh docker-verify.test.ps1

# Or using npm
npm run docker:verify

# Or using Vitest
npm run test:docker
```

### CI/CD Integration

Add to your CI/CD pipeline:

```yaml
# GitHub Actions example
- name: Verify Docker Image
  run: npm run test:docker
```

## Test Output

### Success Output

```
==========================================
Docker Verification Tests
==========================================

Test 1: Building Docker image...
✓ PASS: Docker image builds successfully

Test 2: Checking image size...
Image size: 45.2MB
✓ PASS: Image size is reasonable (45.2MB)

Test 3: Starting container...
✓ PASS: Container starts successfully

Test 4: Checking if nginx serves files...
✓ PASS: Nginx serves files correctly (HTTP 200)

Test 5: Checking if index.html is served...
✓ PASS: index.html is served correctly

Test 6: Checking SPA routing...
✓ PASS: SPA routing works correctly (HTTP 200)

Test 7: Checking gzip compression...
✓ PASS: Gzip compression is enabled

Test 8: Checking security headers...
✓ PASS: X-Frame-Options header is present
✓ PASS: X-Content-Type-Options header is present

Test 9: Checking container health...
✓ PASS: Container health check passes

Test 10: Checking container logs for errors...
✓ PASS: Container logs show no errors

==========================================
Test Summary
==========================================
Tests Passed: 10
Tests Failed: 0
==========================================
All tests passed!
```

### Failure Output

If tests fail, you'll see detailed error messages:

```
✗ FAIL: Image size is too large (150MB, expected <100MB)
✗ FAIL: Nginx not serving files correctly (HTTP 500)
```

## Troubleshooting

### Image Size Too Large

If the image size exceeds 100MB:

1. Check if node_modules are being copied (should only copy dist)
2. Verify multi-stage build is working correctly
3. Ensure nginx:alpine is used (not nginx:latest)
4. Check for unnecessary files in the image

### Container Won't Start

If the container fails to start:

1. Check Docker logs: `docker logs frontend-unificado-test`
2. Verify port 8888 is not already in use
3. Ensure Docker daemon is running
4. Check Dockerfile syntax

### Nginx Not Serving Files

If nginx returns errors:

1. Verify dist folder was created during build
2. Check nginx.conf syntax
3. Ensure files are copied to correct location
4. Verify permissions on copied files

### SPA Routing Not Working

If SPA routing fails:

1. Check nginx.conf has `try_files $uri $uri/ /index.html;`
2. Verify location block is configured correctly
3. Test manually: `curl http://localhost:8888/eventos`

## Manual Verification

You can also verify manually:

```bash
# Build image
docker build -t frontend-unificado:test .

# Check size
docker images frontend-unificado:test

# Run container
docker run -d --name test-container -p 8888:80 frontend-unificado:test

# Test endpoints
curl http://localhost:8888/
curl http://localhost:8888/eventos
curl -I http://localhost:8888/

# Check logs
docker logs test-container

# Cleanup
docker stop test-container
docker rm test-container
docker rmi frontend-unificado:test
```

## Best Practices

1. **Run tests before pushing**: Always verify Docker image locally
2. **Check image size**: Keep image under 100MB for faster deployments
3. **Test SPA routing**: Ensure all routes serve index.html
4. **Verify security headers**: Confirm all security headers are present
5. **Monitor logs**: Check for errors or warnings in container logs

## Integration with Task 22.1

This verification suite fulfills Task 22.1 requirements:

- ✅ Test construcción de imagen
- ✅ Test tamaño de imagen es razonable (<100MB)
- ✅ Test nginx sirve archivos correctamente
- ✅ Requirements: 19.1, 19.6

## Next Steps

After verification passes:

1. Tag image for production: `docker tag frontend-unificado:test frontend-unificado:latest`
2. Push to registry: `docker push frontend-unificado:latest`
3. Deploy to environment
4. Monitor application logs

## References

- [Dockerfile](../Dockerfile)
- [nginx.conf](../nginx.conf)
- [docker-compose.yml](../docker-compose.yml)
- [DOCKER.md](../DOCKER.md)
- [DOCKER-QUICK-REFERENCE.md](../DOCKER-QUICK-REFERENCE.md)
