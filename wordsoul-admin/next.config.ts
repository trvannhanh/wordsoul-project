import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // 1. Kích hoạt chế độ Static Export
  output: 'export',

  // 2. Tắt tối ưu hóa hình ảnh mặc định (vì Static Export không hỗ trợ Image Optimization API của Next.js Server)
  images: {
    unoptimized: true,
  },

  /* Các tùy chọn config khác của bạn (nếu có) */
};

export default nextConfig;
