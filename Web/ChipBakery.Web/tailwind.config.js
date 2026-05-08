/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: ['class', '[data-theme="dark"]'],
  content: [
    './**/*.{razor,html,cshtml}'
  ],
  theme: {
    extend: {
      colors: {
        paper:        'rgb(var(--paper) / <alpha-value>)',
        'paper-alt':  'rgb(var(--paper-alt) / <alpha-value>)',
        kraft:        'rgb(var(--kraft) / <alpha-value>)',
        ink:          'rgb(var(--ink) / <alpha-value>)',
        'ink-muted':  'rgb(var(--ink-muted) / <alpha-value>)',
        chocolate:    'rgb(var(--chocolate) / <alpha-value>)',
        raspberry:    'rgb(var(--raspberry) / <alpha-value>)',
        basil:        'rgb(var(--basil) / <alpha-value>)',
        blueberry:    'rgb(var(--blueberry) / <alpha-value>)',
        lemon:        'rgb(var(--lemon) / <alpha-value>)',
        tape:         'rgb(var(--tape) / <alpha-value>)',
        bakery: {
          'crust':         'rgb(var(--chocolate) / <alpha-value>)',
          'dough':         'rgb(var(--paper) / <alpha-value>)',
          'cream':         'rgb(var(--paper-alt) / <alpha-value>)',
          'pastel-pink':   'rgb(var(--paper) / <alpha-value>)',
          'pastel-green':  'rgb(var(--paper) / <alpha-value>)',
          'pastel-yellow': 'rgb(var(--paper-alt) / <alpha-value>)',
        }
      },
      fontFamily: {
        sans:    ['"DM Sans"', 'system-ui', 'sans-serif'],
        serif:   ['Fraunces', 'Georgia', 'serif'],
        display: ['Caveat', 'cursive'],
        mono:    ['"Courier Prime"', 'ui-monospace', 'monospace'],
      },
      borderRadius: {
        '3xl': '1.5rem',
        '4xl': '2rem',
      },
      boxShadow: {
        'paper':   '0 1px 2px rgb(var(--ink) / 0.06), 0 4px 12px rgb(var(--ink) / 0.05)',
        'tape':    '0 2px 6px rgb(var(--ink) / 0.12)',
        'sticker': '3px 4px 0 rgb(var(--ink) / 0.10)',
        'note':    '4px 5px 0 rgb(var(--ink) / 0.08)',
        'deep':    '0 10px 28px rgb(var(--ink) / 0.12)',
        'press':   'inset 0 2px 4px rgb(var(--ink) / 0.10)',
      },
      keyframes: {
        'wobble-hover': {
          '0%, 100%': { transform: 'rotate(0deg)' },
          '25%':      { transform: 'rotate(-1deg)' },
          '75%':      { transform: 'rotate(1deg)' },
        },
        'sketch-in': {
          '0%':   { strokeDashoffset: '100' },
          '100%': { strokeDashoffset: '0' },
        },
        'fade-up': {
          '0%':   { opacity: '0', transform: 'translateY(8px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
      },
      animation: {
        'wobble-hover': 'wobble-hover 0.4s ease-in-out',
        'sketch-in':    'sketch-in 1s ease-out forwards',
        'fade-up':      'fade-up 0.35s ease-out',
      },
    },
  },
  plugins: [],
}
