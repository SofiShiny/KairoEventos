# Docker Verification Quick Start

## Prerequisites

- Docker installed and running
- TypeScript compilation errors fixed
- Node.js 18+ installed

## Quick Test

### Windows (PowerShell)
```powershell
.\docker-verify-simple.ps1
```

### Linux/Mac (Bash)
```bash
bash docker-verify.test.sh
```

### Using npm
```bash
npm run docker:verify
```

## What Gets Tested

✅ Docker image builds successfully  
✅ Image size is <100MB  
✅ Container starts without errors  
✅ Nginx serves files correctly  
✅ SPA routing works (all routes serve index.html)  
✅ Security headers are present  
✅ Gzip compression is enabled  
✅ Container logs show no errors  

## Expected Results

All tests should pass:
```
==========================================
Test Summary
==========================================
Tests Passed: 8
Tests Failed: 0
==========================================
All tests passed!
```

## If Tests Fail

1. **Build fails**: Check TypeScript errors with `npm run build`
2. **Size too large**: Review Dockerfile and ensure multi-stage build
3. **Nginx not serving**: Check nginx.conf configuration
4. **SPA routing fails**: Verify `try_files` directive in nginx.conf

## Manual Verification

```bash
# Build
docker build -t frontend-unificado:test .

# Run
docker run -d --name test -p 8888:80 frontend-unificado:test

# Test
curl http://localhost:8888/
curl http://localhost:8888/eventos

# Cleanup
docker stop test && docker rm test && docker rmi frontend-unificado:test
```

## Full Documentation

See `docs/DOCKER-VERIFICATION.md` for complete documentation.
