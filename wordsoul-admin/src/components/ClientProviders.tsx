'use client';

import React from 'react';
import { AntdRegistry } from '@ant-design/nextjs-registry';
import { AuthProvider } from '@/contexts/AuthContext';
import { ConfigProvider } from 'antd';
import theme from '@/theme/themeConfig';

export default function ClientProviders({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <AntdRegistry>
      <ConfigProvider theme={theme}>
        <AuthProvider>
          {children}
        </AuthProvider>
      </ConfigProvider>
    </AntdRegistry>
  );
}
