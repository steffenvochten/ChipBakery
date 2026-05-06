/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: ['class', '[data-theme="dark"]'],
  content: [
    './**/*.{razor,html,cshtml}'
  ],
  theme: {
    extend: {
      colors: {
        bakery: {
          'crust': '#78350f',      /* Deep Amber/Brown */
          'dough': '#fef3c7',      /* Amber 100 */
          'cream': '#fffbeb',      /* Amber 50 */
          'pastel-pink': '#fdf2f2',
          'pastel-green': '#f0fdf4',
          'pastel-yellow': '#fefce8',
        }
      },
      fontFamily: {
        sans: ['Outfit', 'sans-serif'],
        serif: ['Playfair Display', 'serif'],
      },
      borderRadius: {
        '3xl': '1.5rem',
        '4xl': '2rem',
      }
    },
  },
  plugins: [],
}
