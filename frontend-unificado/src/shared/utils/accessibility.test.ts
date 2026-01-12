import { describe, it, expect, vi } from 'vitest';
import {
  handleKeyboardClick,
  handleEscapeKey,
  getFocusableElements,
  announceToScreenReader,
  isVisibleToScreenReader,
  generateId,
} from './accessibility';

describe('Accessibility Utilities', () => {
  describe('handleKeyboardClick', () => {
    it('should call callback on Enter key', () => {
      const callback = vi.fn();
      const event = {
        key: 'Enter',
        preventDefault: vi.fn(),
      } as unknown as React.KeyboardEvent;

      handleKeyboardClick(event, callback);

      expect(callback).toHaveBeenCalledTimes(1);
      expect(event.preventDefault).toHaveBeenCalled();
    });

    it('should call callback on Space key', () => {
      const callback = vi.fn();
      const event = {
        key: ' ',
        preventDefault: vi.fn(),
      } as unknown as React.KeyboardEvent;

      handleKeyboardClick(event, callback);

      expect(callback).toHaveBeenCalledTimes(1);
      expect(event.preventDefault).toHaveBeenCalled();
    });

    it('should not call callback on other keys', () => {
      const callback = vi.fn();
      const event = {
        key: 'Tab',
        preventDefault: vi.fn(),
      } as unknown as React.KeyboardEvent;

      handleKeyboardClick(event, callback);

      expect(callback).not.toHaveBeenCalled();
      expect(event.preventDefault).not.toHaveBeenCalled();
    });
  });

  describe('handleEscapeKey', () => {
    it('should call callback on Escape key', () => {
      const callback = vi.fn();
      const event = {
        key: 'Escape',
        preventDefault: vi.fn(),
      } as unknown as React.KeyboardEvent;

      handleEscapeKey(event, callback);

      expect(callback).toHaveBeenCalledTimes(1);
      expect(event.preventDefault).toHaveBeenCalled();
    });

    it('should not call callback on other keys', () => {
      const callback = vi.fn();
      const event = {
        key: 'Enter',
        preventDefault: vi.fn(),
      } as unknown as React.KeyboardEvent;

      handleEscapeKey(event, callback);

      expect(callback).not.toHaveBeenCalled();
      expect(event.preventDefault).not.toHaveBeenCalled();
    });
  });

  describe('getFocusableElements', () => {
    it('should return all focusable elements', () => {
      const container = document.createElement('div');
      container.innerHTML = `
        <button>Button 1</button>
        <a href="#">Link</a>
        <input type="text" />
        <button disabled>Disabled Button</button>
        <div tabindex="0">Focusable Div</div>
        <div tabindex="-1">Non-focusable Div</div>
      `;

      const focusable = getFocusableElements(container);

      expect(focusable).toHaveLength(4); // button, link, input, div with tabindex="0"
    });
  });

  describe('announceToScreenReader', () => {
    it('should create and remove announcement element', () => {
      vi.useFakeTimers();

      announceToScreenReader('Test message', 'polite');

      const announcement = document.querySelector('[role="status"]');
      expect(announcement).toBeTruthy();
      expect(announcement?.textContent).toBe('Test message');
      expect(announcement?.getAttribute('aria-live')).toBe('polite');

      vi.advanceTimersByTime(1000);

      const removedAnnouncement = document.querySelector('[role="status"]');
      expect(removedAnnouncement).toBeFalsy();

      vi.useRealTimers();
    });

    it('should use assertive priority when specified', () => {
      announceToScreenReader('Urgent message', 'assertive');

      const announcement = document.querySelector('[role="status"]');
      expect(announcement?.getAttribute('aria-live')).toBe('assertive');

      // Cleanup
      if (announcement) {
        document.body.removeChild(announcement);
      }
    });
  });

  describe('isVisibleToScreenReader', () => {
    it('should return true for visible elements', () => {
      const element = document.createElement('div');
      expect(isVisibleToScreenReader(element)).toBe(true);
    });

    it('should return false for aria-hidden elements', () => {
      const element = document.createElement('div');
      element.setAttribute('aria-hidden', 'true');
      expect(isVisibleToScreenReader(element)).toBe(false);
    });

    it('should return false for hidden elements', () => {
      const element = document.createElement('div');
      element.hidden = true;
      expect(isVisibleToScreenReader(element)).toBe(false);
    });

    it('should return false for display:none elements', () => {
      const element = document.createElement('div');
      element.style.display = 'none';
      expect(isVisibleToScreenReader(element)).toBe(false);
    });

    it('should return false for visibility:hidden elements', () => {
      const element = document.createElement('div');
      element.style.visibility = 'hidden';
      expect(isVisibleToScreenReader(element)).toBe(false);
    });
  });

  describe('generateId', () => {
    it('should generate unique IDs', () => {
      const id1 = generateId('test');
      const id2 = generateId('test');
      const id3 = generateId('test');

      expect(id1).not.toBe(id2);
      expect(id2).not.toBe(id3);
      expect(id1).toMatch(/^test-\d+$/);
    });

    it('should use default prefix when not provided', () => {
      const id = generateId();
      expect(id).toMatch(/^a11y-\d+$/);
    });
  });
});
