import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Azure Static Web Apps natively supports Next.js SSR/Hybrid.
  // We remove `output: 'export'` so dynamic routes like /[id] work without generateStaticParams.
  images: {
    unoptimized: true,
  },
};

export default nextConfig;
