'use client';

import React from 'react';
import { AntdRegistry } from '@ant-design/nextjs-registry';
import { AuthProvider } from '@/contexts/AuthContext';
import { ThemeProvider, useTheme } from '@/contexts/ThemeContext';
import { ConfigProvider, App } from 'antd';

// Inner wrapper that can consume ThemeContext
function AntdConfigProvider({ children }: { children: React.ReactNode }) {
  const { antdTheme } = useTheme();
  return (
    <ConfigProvider theme={antdTheme}>
      <App>{children}</App>
    </ConfigProvider>
  );
}

export default function ClientProviders({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <AntdRegistry>
      <ThemeProvider>
        <AntdConfigProvider>
          <AuthProvider>
            {children}
          </AuthProvider>
        </AntdConfigProvider>
      </ThemeProvider>
    </AntdRegistry>
  );
}
