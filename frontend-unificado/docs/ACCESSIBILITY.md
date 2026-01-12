# Accessibility (A11y) Guide

This document outlines the accessibility features implemented in the Frontend Unificado application to ensure WCAG 2.1 Level AA compliance.

## Overview

The application follows Web Content Accessibility Guidelines (WCAG) 2.1 Level AA standards to ensure all users, including those with disabilities, can effectively use the system.

## Implemented Features

### 1. Semantic HTML Elements (Requirement 20.1)

All layouts and pages use proper semantic HTML5 elements:

- `<header>` - Used in MainLayout for the AppBar
- `<nav>` - Used for navigation menus (desktop and mobile)
- `<main>` - Used for primary content areas
- `<footer>` - Used for footer content
- `<article>` - Used for self-contained content
- `<section>` - Used for thematic groupings

**Example:**
```tsx
<Box component="header">
  <AppBar>...</AppBar>
</Box>

<Box component="main" id="main-content">
  <Outlet />
</Box>

<Box component="footer">
  <Typography>© 2024 Kairo</Typography>
</Box>
```

### 2. Image Alt Text (Requirement 20.2)

All images and icons have appropriate alternative text:

- Decorative icons use `aria-hidden="true"`
- Meaningful images have descriptive `alt` attributes
- Icon-only buttons have `aria-label` attributes

**Example:**
```tsx
// Decorative icon
<EventIcon aria-hidden="true" />

// Meaningful image
<Box role="img" aria-label="Kairo application logo">
  <EventIcon />
</Box>

// Icon button
<IconButton aria-label="Open navigation menu">
  <MenuIcon />
</IconButton>
```

### 3. Form Labels (Requirement 20.3)

All form inputs are properly associated with labels:

- Material-UI TextField automatically associates labels
- Custom inputs use `htmlFor` attribute
- Required fields are marked with `aria-required`

**Example:**
```tsx
<TextField
  label="Email"
  required
  inputProps={{
    'aria-required': true,
  }}
/>
```

### 4. Keyboard Navigation (Requirement 20.4)

Complete keyboard navigation support:

- **Tab**: Navigate between interactive elements
- **Enter/Space**: Activate buttons and links
- **Escape**: Close modals and menus
- **Arrow keys**: Navigate within menus and lists

**Features:**
- Skip link to jump to main content
- Focus indicators on all interactive elements
- Logical tab order
- Focus trap in modals

**Example:**
```tsx
// Skip link
<SkipLink href="#main-content">
  Skip to main content
</SkipLink>

// Focus trap in modal
<FocusTrap active={isOpen}>
  <Dialog>...</Dialog>
</FocusTrap>
```

### 5. Color Contrast (Requirement 20.5)

All text meets WCAG AA contrast requirements (4.5:1 minimum):

- Primary text: High contrast against background
- Secondary text: Minimum 4.5:1 contrast ratio
- Interactive elements: Clear visual distinction
- Error states: Red with sufficient contrast

**Theme Configuration:**
```typescript
const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2', // Contrast ratio: 4.5:1 on white
    },
    text: {
      primary: 'rgba(0, 0, 0, 0.87)', // Contrast ratio: 15.8:1
      secondary: 'rgba(0, 0, 0, 0.6)', // Contrast ratio: 7.0:1
    },
  },
});
```

### 6. ARIA Labels (Requirement 20.6)

Interactive elements without visible text have ARIA labels:

- Icon buttons: `aria-label`
- Navigation regions: `aria-label`
- Status messages: `aria-live`
- Menus: `aria-haspopup`, `aria-expanded`

**Example:**
```tsx
// Icon button
<IconButton aria-label="User menu">
  <AccountCircleIcon />
</IconButton>

// Navigation
<Box component="nav" aria-label="Main navigation">
  {/* Navigation items */}
</Box>

// Live region
<Alert role="alert" aria-live="assertive">
  Error message
</Alert>
```

## Components with Accessibility Features

### SkipLink

Allows keyboard users to skip to main content:

```tsx
import { SkipLink } from '@/shared/components';

<SkipLink href="#main-content">
  Skip to main content
</SkipLink>
```

### FocusTrap

Traps focus within modals and dialogs:

```tsx
import { FocusTrap } from '@/shared/components';

<FocusTrap active={isOpen}>
  <Dialog>
    {/* Dialog content */}
  </Dialog>
</FocusTrap>
```

### LoadingSpinner

Announces loading state to screen readers:

```tsx
<LoadingSpinner size="medium" />
// Renders: <CircularProgress aria-label="Loading..." role="status" />
```

### EmptyState

Announces empty states to screen readers:

```tsx
<EmptyState
  title="No events found"
  description="Try adjusting your filters"
/>
// Includes: role="status" aria-live="polite"
```

## Testing Accessibility

### Manual Testing

1. **Keyboard Navigation**
   - Tab through all interactive elements
   - Verify focus indicators are visible
   - Test skip link functionality
   - Verify modal focus trap

2. **Screen Reader Testing**
   - Test with NVDA (Windows) or VoiceOver (Mac)
   - Verify all content is announced
   - Check form labels and error messages
   - Verify ARIA labels on icon buttons

3. **Color Contrast**
   - Use browser DevTools to check contrast ratios
   - Verify text is readable in all states
   - Test with high contrast mode

### Automated Testing

Use accessibility testing tools:

```bash
# Install axe-core for automated testing
npm install --save-dev @axe-core/react

# Run accessibility tests
npm test
```

## Best Practices

### DO:
- ✅ Use semantic HTML elements
- ✅ Provide alt text for images
- ✅ Associate labels with inputs
- ✅ Ensure keyboard navigation works
- ✅ Maintain sufficient color contrast
- ✅ Add ARIA labels to icon buttons
- ✅ Use focus indicators
- ✅ Implement skip links

### DON'T:
- ❌ Use divs for buttons
- ❌ Remove focus outlines
- ❌ Use color alone to convey information
- ❌ Create keyboard traps (except in modals)
- ❌ Use low contrast text
- ❌ Forget alt text on images
- ❌ Use placeholder as label

## WCAG 2.1 Level AA Compliance

### Perceivable
- ✅ Text alternatives for non-text content
- ✅ Sufficient color contrast (4.5:1 minimum)
- ✅ Responsive and adaptable layouts

### Operable
- ✅ Keyboard accessible
- ✅ Enough time to read content
- ✅ No seizure-inducing content
- ✅ Navigable with clear focus indicators

### Understandable
- ✅ Readable and understandable text
- ✅ Predictable navigation
- ✅ Input assistance with error messages

### Robust
- ✅ Compatible with assistive technologies
- ✅ Valid HTML and ARIA usage

## Resources

- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [Material-UI Accessibility](https://mui.com/material-ui/guides/accessibility/)
- [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)
- [axe DevTools](https://www.deque.com/axe/devtools/)

## Support

For accessibility issues or questions, please contact the development team or file an issue in the project repository.
