import { Link } from '@mui/material';
import { type ReactNode } from 'react';

interface SkipLinkProps {
  href: string;
  children: ReactNode;
}

/**
 * SkipLink - Accessibility component for keyboard navigation
 * Allows users to skip to main content
 * 
 * Requirements:
 * - 20.1: Semantic HTML elements
 * - 20.4: Complete keyboard navigation
 */
export function SkipLink({ href, children }: SkipLinkProps) {
  return (
    <Link
      href={href}
      sx={{
        position: 'absolute',
        left: '-9999px',
        zIndex: 999,
        padding: '1rem',
        backgroundColor: 'primary.main',
        color: 'primary.contrastText',
        textDecoration: 'none',
        '&:focus': {
          left: '50%',
          top: '1rem',
          transform: 'translateX(-50%)',
          outline: '3px solid',
          outlineColor: 'secondary.main',
        },
      }}
    >
      {children}
    </Link>
  );
}
