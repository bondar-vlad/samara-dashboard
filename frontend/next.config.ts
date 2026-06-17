import type { NextConfig } from "next";

// Backend (YARP gateway) base URL. Override with BACKEND_URL in the environment.
const BACKEND_URL = process.env.BACKEND_URL ?? "http://localhost:8080";

const nextConfig: NextConfig = {
  // Emit a self-contained server bundle for a slim Docker runtime image.
  output: "standalone",

  // Proxy all backend calls through the Next server so the browser stays
  // same-origin (no CORS). `/bff/*` -> gateway, which fans out to services:
  //   /bff/education/api/...  ->  education service
  //   /bff/analysis/api/...   ->  analysis service
  async rewrites() {
    return [
      {
        source: "/bff/:path*",
        destination: `${BACKEND_URL}/:path*`,
      },
    ];
  },
};

export default nextConfig;
