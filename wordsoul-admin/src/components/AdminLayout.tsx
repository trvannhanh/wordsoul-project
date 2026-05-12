'use client';

import React, { useState } from 'react';
import { Layout, Menu, Button, Dropdown, Avatar, theme as antTheme } from 'antd';
import {
  DashboardOutlined,
  UserOutlined,
  BookOutlined,
  SettingOutlined,
  LogoutOutlined,
  TrophyOutlined,
  AimOutlined,
  MenuUnfoldOutlined,
  MenuFoldOutlined,
} from '@ant-design/icons';
import { useAuth } from '@/contexts/AuthContext';
import { useRouter, usePathname } from 'next/navigation';

const { Header, Sider, Content } = Layout;

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  const [collapsed, setCollapsed] = useState(false);
  const { user, logout, isLoading } = useAuth();
  const router = useRouter();
  const pathname = usePathname();
  const {
    token: { colorBgContainer, borderRadiusLG },
  } = antTheme.useToken();

  // Prevent rendering before auth check completes to avoid hydration mismatch/flicker
  if (isLoading || !user) {
    return null;
  }

  const menuItems = [
    {
      key: '/dashboard',
      icon: <DashboardOutlined />,
      label: 'Dashboard',
    },
    {
      key: '/users',
      icon: <UserOutlined />,
      label: 'User Management',
    },
    {
      key: '/vocabularies',
      icon: <BookOutlined />,
      label: 'Vocabulary Library',
    },
    {
      key: '/quests',
      icon: <AimOutlined />,
      label: 'Quests & Events',
    },
    {
      key: '/game-balance',
      icon: <TrophyOutlined />,
      label: 'Game Balance',
    },
  ];

  // SuperAdmin exclusive features
  if (user.role === 'SuperAdmin') {
    menuItems.push({
      key: '/system-health',
      icon: <SettingOutlined />,
      label: 'System Health & SRS',
    });
  }

  const userMenu = {
    items: [
      {
        key: '1',
        label: (
          <div className="px-2 py-1">
            <div className="font-medium">{user.name}</div>
            <div className="text-xs text-gray-500">{user.role}</div>
          </div>
        ),
      },
      { type: 'divider' as const },
      {
        key: '2',
        icon: <LogoutOutlined />,
        label: 'Logout',
        onClick: logout,
        danger: true,
      },
    ],
  };

  return (
    <Layout className="min-h-screen">
      <Sider trigger={null} collapsible collapsed={collapsed} theme="light" className="border-r border-gray-200">
        <div className="h-16 m-4 flex items-center justify-center font-bold text-xl text-blue-600 truncate">
          {collapsed ? 'VA' : 'Vocamon Admin'}
        </div>
        <Menu
          mode="inline"
          selectedKeys={[pathname]}
          items={menuItems}
          onClick={({ key }) => router.push(key)}
          className="border-r-0"
        />
      </Sider>
      <Layout>
        <Header style={{ padding: 0, background: colorBgContainer }} className="flex justify-between items-center shadow-sm z-10 px-4">
          <Button
            type="text"
            icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            onClick={() => setCollapsed(!collapsed)}
            className="text-lg w-16 h-16"
          />
          <Dropdown menu={userMenu} placement="bottomRight" arrow>
            <div className="cursor-pointer flex items-center gap-2 hover:bg-gray-50 px-3 py-1 rounded-md transition-colors">
              <Avatar style={{ backgroundColor: '#1890ff' }} icon={<UserOutlined />} />
              <span className="font-medium hidden sm:block">{user.name}</span>
            </div>
          </Dropdown>
        </Header>
        <Content
          className="m-6 p-6 min-h-[280px]"
          style={{
            background: colorBgContainer,
            borderRadius: borderRadiusLG,
          }}
        >
          {children}
        </Content>
      </Layout>
    </Layout>
  );
}
