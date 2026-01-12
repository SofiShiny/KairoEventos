/**
 * Docker Verification Tests for Frontend Unificado
 * Tests: Image build, size, and nginx functionality
 * Requirements: 19.1, 19.6
 * 
 * Note: These tests require Docker to be installed and running.
 * Run with: npm run test:docker
 */

import { describe, it, expect, beforeAll, afterAll } from 'vitest';
import { execSync } from 'child_process';

const IMAGE_NAME = 'frontend-unificado:test';
const CONTAINER_NAME = 'frontend-unificado-test';
const TEST_PORT = 8889;

describe('Docker Image Verification', () => {
  beforeAll(() => {
    // Cleanup any existing test containers/images
    try {
      execSync(`docker stop ${CONTAINER_NAME}`, { stdio: 'ignore' });
      execSync(`docker rm ${CONTAINER_NAME}`, { stdio: 'ignore' });
      execSync(`docker rmi ${IMAGE_NAME}`, { stdio: 'ignore' });
    } catch {
      // Ignore errors if containers/images don't exist
    }
  });

  afterAll(() => {
    // Cleanup after tests
    try {
      execSync(`docker stop ${CONTAINER_NAME}`, { stdio: 'ignore' });
      execSync(`docker rm ${CONTAINER_NAME}`, { stdio: 'ignore' });
      execSync(`docker rmi ${IMAGE_NAME}`, { stdio: 'ignore' });
    } catch {
      // Ignore cleanup errors
    }
  });

  it('should build Docker image successfully', () => {
    // Requirement 19.1: Docker image builds correctly
    expect(() => {
      execSync(`docker build -t ${IMAGE_NAME} .`, {
        cwd: process.cwd(),
        stdio: 'pipe',
      });
    }).not.toThrow();
  }, 120000); // 2 minute timeout for build

  it('should have reasonable image size (<100MB)', () => {
    // Requirement 19.6: Minimize image size
    const output = execSync(`docker images ${IMAGE_NAME} --format "{{.Size}}"`, {
      encoding: 'utf-8',
    }).trim();

    console.log(`Image size: ${output}`);

    // Parse size
    let sizeMB = 0;
    if (output.includes('MB')) {
      sizeMB = parseFloat(output.replace('MB', ''));
    } else if (output.includes('GB')) {
      sizeMB = parseFloat(output.replace('GB', '')) * 1024;
    }

    expect(sizeMB).toBeLessThan(100);
  });

  it('should start container successfully', () => {
    expect(() => {
      execSync(
        `docker run -d --name ${CONTAINER_NAME} -p ${TEST_PORT}:80 ${IMAGE_NAME}`,
        { stdio: 'pipe' }
      );
    }).not.toThrow();

    // Wait for container to be ready
    execSync('sleep 5', { stdio: 'ignore' });
  });

  it('should serve files via nginx', async () => {
    // Requirement 19.1: Nginx serves files correctly
    const response = await fetch(`http://localhost:${TEST_PORT}/`);
    expect(response.status).toBe(200);
  });

  it('should serve index.html', async () => {
    const response = await fetch(`http://localhost:${TEST_PORT}/`);
    const html = await response.text();
    
    expect(html).toMatch(/<!doctype html>|<html/i);
  });

  it('should support SPA routing', async () => {
    // Non-existent routes should serve index.html
    const response = await fetch(`http://localhost:${TEST_PORT}/eventos`);
    expect(response.status).toBe(200);
    
    const html = await response.text();
    expect(html).toMatch(/<!doctype html>|<html/i);
  });

  it('should enable gzip compression', async () => {
    const response = await fetch(`http://localhost:${TEST_PORT}/`, {
      headers: {
        'Accept-Encoding': 'gzip',
      },
    });
    
    const contentEncoding = response.headers.get('content-encoding');
    expect(contentEncoding).toMatch(/gzip/i);
  });

  it('should include security headers', async () => {
    const response = await fetch(`http://localhost:${TEST_PORT}/`);
    
    // Check for security headers
    expect(response.headers.get('x-frame-options')).toBeTruthy();
    expect(response.headers.get('x-content-type-options')).toBeTruthy();
  });

  it('should have healthy container status', () => {
    // Wait for health check to run
    execSync('sleep 5', { stdio: 'ignore' });
    
    const healthStatus = execSync(
      `docker inspect --format='{{.State.Health.Status}}' ${CONTAINER_NAME}`,
      { encoding: 'utf-8' }
    ).trim();

    // Health status should be healthy or none (if health check not configured)
    expect(['healthy', 'none', '']).toContain(healthStatus);
  });

  it('should have no errors in container logs', () => {
    const logs = execSync(`docker logs ${CONTAINER_NAME}`, {
      encoding: 'utf-8',
    });

    // Logs should not contain error messages
    expect(logs.toLowerCase()).not.toMatch(/error/);
  });
});

describe('Docker Image Structure', () => {
  it('should use multi-stage build', () => {
    const dockerfile = execSync('cat Dockerfile', { encoding: 'utf-8' });
    
    // Check for multi-stage build pattern
    expect(dockerfile).toMatch(/FROM.*AS builder/i);
    expect(dockerfile).toMatch(/FROM nginx/i);
  });

  it('should copy built files to nginx html directory', () => {
    const dockerfile = execSync('cat Dockerfile', { encoding: 'utf-8' });
    
    expect(dockerfile).toMatch(/COPY.*\/usr\/share\/nginx\/html/);
  });

  it('should expose port 80', () => {
    const dockerfile = execSync('cat Dockerfile', { encoding: 'utf-8' });
    
    expect(dockerfile).toMatch(/EXPOSE 80/);
  });

  it('should have health check configured', () => {
    const dockerfile = execSync('cat Dockerfile', { encoding: 'utf-8' });
    
    expect(dockerfile).toMatch(/HEALTHCHECK/i);
  });
});

describe('Nginx Configuration', () => {
  it('should have SPA routing configuration', () => {
    const nginxConf = execSync('cat nginx.conf', { encoding: 'utf-8' });
    
    // Check for try_files directive for SPA routing
    expect(nginxConf).toMatch(/try_files.*\/index\.html/);
  });

  it('should have gzip compression enabled', () => {
    const nginxConf = execSync('cat nginx.conf', { encoding: 'utf-8' });
    
    expect(nginxConf).toMatch(/gzip on/);
  });

  it('should have cache configuration for static assets', () => {
    const nginxConf = execSync('cat nginx.conf', { encoding: 'utf-8' });
    
    expect(nginxConf).toMatch(/expires/);
    expect(nginxConf).toMatch(/Cache-Control/);
  });

  it('should have security headers configured', () => {
    const nginxConf = execSync('cat nginx.conf', { encoding: 'utf-8' });
    
    expect(nginxConf).toMatch(/X-Frame-Options/);
    expect(nginxConf).toMatch(/X-Content-Type-Options/);
    expect(nginxConf).toMatch(/X-XSS-Protection/);
  });

  it('should disable server tokens', () => {
    const nginxConf = execSync('cat nginx.conf', { encoding: 'utf-8' });
    
    expect(nginxConf).toMatch(/server_tokens off/);
  });
});
