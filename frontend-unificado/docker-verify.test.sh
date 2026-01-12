#!/bin/bash
# Docker Verification Test Script for Frontend Unificado
# Tests: Image build, size, and nginx functionality
# Requirements: 19.1, 19.6

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test counters
TESTS_PASSED=0
TESTS_FAILED=0

# Image name
IMAGE_NAME="frontend-unificado:test"
CONTAINER_NAME="frontend-unificado-test"

# Function to print test result
print_result() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✓ PASS${NC}: $2"
        ((TESTS_PASSED++))
    else
        echo -e "${RED}✗ FAIL${NC}: $2"
        ((TESTS_FAILED++))
    fi
}

# Function to cleanup
cleanup() {
    echo -e "\n${YELLOW}Cleaning up...${NC}"
    docker stop $CONTAINER_NAME 2>/dev/null || true
    docker rm $CONTAINER_NAME 2>/dev/null || true
    docker rmi $IMAGE_NAME 2>/dev/null || true
}

# Trap to ensure cleanup on exit
trap cleanup EXIT

echo "=========================================="
echo "Docker Verification Tests"
echo "=========================================="

# Test 1: Build Docker image
echo -e "\n${YELLOW}Test 1: Building Docker image...${NC}"
if docker build -t $IMAGE_NAME . > /dev/null 2>&1; then
    print_result 0 "Docker image builds successfully"
else
    print_result 1 "Docker image build failed"
    exit 1
fi

# Test 2: Check image size (<100MB)
echo -e "\n${YELLOW}Test 2: Checking image size...${NC}"
IMAGE_SIZE=$(docker images $IMAGE_NAME --format "{{.Size}}" | head -n 1)
IMAGE_SIZE_MB=$(docker images $IMAGE_NAME --format "{{.Size}}" | head -n 1 | sed 's/MB//' | sed 's/GB/*1024/' | bc 2>/dev/null || echo "0")

echo "Image size: $IMAGE_SIZE"

# Convert to MB if in GB
if [[ $IMAGE_SIZE == *"GB"* ]]; then
    IMAGE_SIZE_MB=$(echo "$IMAGE_SIZE" | sed 's/GB//' | awk '{print $1 * 1024}')
elif [[ $IMAGE_SIZE == *"MB"* ]]; then
    IMAGE_SIZE_MB=$(echo "$IMAGE_SIZE" | sed 's/MB//')
fi

# Check if size is reasonable (<100MB)
if (( $(echo "$IMAGE_SIZE_MB < 100" | bc -l) )); then
    print_result 0 "Image size is reasonable ($IMAGE_SIZE)"
else
    print_result 1 "Image size is too large ($IMAGE_SIZE, expected <100MB)"
fi

# Test 3: Start container
echo -e "\n${YELLOW}Test 3: Starting container...${NC}"
if docker run -d --name $CONTAINER_NAME -p 8888:80 $IMAGE_NAME > /dev/null 2>&1; then
    print_result 0 "Container starts successfully"
    
    # Wait for container to be ready
    echo "Waiting for container to be ready..."
    sleep 5
else
    print_result 1 "Container failed to start"
    exit 1
fi

# Test 4: Check if nginx is serving files
echo -e "\n${YELLOW}Test 4: Checking if nginx serves files...${NC}"
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8888/)
if [ "$HTTP_CODE" = "200" ]; then
    print_result 0 "Nginx serves files correctly (HTTP $HTTP_CODE)"
else
    print_result 1 "Nginx not serving files correctly (HTTP $HTTP_CODE)"
fi

# Test 5: Check if index.html is served
echo -e "\n${YELLOW}Test 5: Checking if index.html is served...${NC}"
RESPONSE=$(curl -s http://localhost:8888/)
if [[ $RESPONSE == *"<!doctype html>"* ]] || [[ $RESPONSE == *"<html"* ]]; then
    print_result 0 "index.html is served correctly"
else
    print_result 1 "index.html is not served correctly"
fi

# Test 6: Check SPA routing (non-existent route should serve index.html)
echo -e "\n${YELLOW}Test 6: Checking SPA routing...${NC}"
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8888/eventos)
if [ "$HTTP_CODE" = "200" ]; then
    print_result 0 "SPA routing works correctly (HTTP $HTTP_CODE)"
else
    print_result 1 "SPA routing not working (HTTP $HTTP_CODE)"
fi

# Test 7: Check gzip compression
echo -e "\n${YELLOW}Test 7: Checking gzip compression...${NC}"
CONTENT_ENCODING=$(curl -s -I -H "Accept-Encoding: gzip" http://localhost:8888/ | grep -i "content-encoding" | grep -i "gzip")
if [ -n "$CONTENT_ENCODING" ]; then
    print_result 0 "Gzip compression is enabled"
else
    print_result 1 "Gzip compression is not enabled"
fi

# Test 8: Check security headers
echo -e "\n${YELLOW}Test 8: Checking security headers...${NC}"
HEADERS=$(curl -s -I http://localhost:8888/)

# Check X-Frame-Options
if echo "$HEADERS" | grep -qi "X-Frame-Options"; then
    print_result 0 "X-Frame-Options header is present"
else
    print_result 1 "X-Frame-Options header is missing"
fi

# Check X-Content-Type-Options
if echo "$HEADERS" | grep -qi "X-Content-Type-Options"; then
    print_result 0 "X-Content-Type-Options header is present"
else
    print_result 1 "X-Content-Type-Options header is missing"
fi

# Test 9: Check health check endpoint
echo -e "\n${YELLOW}Test 9: Checking container health...${NC}"
sleep 5  # Wait for health check to run
HEALTH_STATUS=$(docker inspect --format='{{.State.Health.Status}}' $CONTAINER_NAME 2>/dev/null || echo "none")
if [ "$HEALTH_STATUS" = "healthy" ] || [ "$HEALTH_STATUS" = "none" ]; then
    print_result 0 "Container health check passes"
else
    print_result 1 "Container health check failed (status: $HEALTH_STATUS)"
fi

# Test 10: Check if container logs show no errors
echo -e "\n${YELLOW}Test 10: Checking container logs for errors...${NC}"
LOGS=$(docker logs $CONTAINER_NAME 2>&1)
if echo "$LOGS" | grep -qi "error"; then
    print_result 1 "Container logs contain errors"
else
    print_result 0 "Container logs show no errors"
fi

# Summary
echo -e "\n=========================================="
echo "Test Summary"
echo "=========================================="
echo -e "Tests Passed: ${GREEN}$TESTS_PASSED${NC}"
echo -e "Tests Failed: ${RED}$TESTS_FAILED${NC}"
echo "=========================================="

if [ $TESTS_FAILED -eq 0 ]; then
    echo -e "${GREEN}All tests passed!${NC}"
    exit 0
else
    echo -e "${RED}Some tests failed!${NC}"
    exit 1
fi
