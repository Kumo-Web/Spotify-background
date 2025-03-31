import type { Config } from "tailwindcss";

export default {
  content: [
    "./src/pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        background: "var(--background)",
        foreground: "var(--foreground)",
        // primary: {
        //   DEFAULT: '#1e90ff',
        //   100: '#001d39',
        //   200: '#003972',
        // } Example
      },
      spacing: {
        'header': 'var(--header-height)',
      },
    },
  },
  plugins: [],
} satisfies Config;
