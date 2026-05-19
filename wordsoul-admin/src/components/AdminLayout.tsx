'use client';

import React, { useState } from 'react';
import { Dropdown, Avatar, Tooltip } from 'antd';
import {
  DashboardOutlined,
  UserOutlined,
  SafetyOutlined,
  TeamOutlined,
  BookOutlined,
  SettingOutlined,
  LogoutOutlined,
  TrophyOutlined,
  AimOutlined,
  MenuUnfoldOutlined,
  MenuFoldOutlined,
  SunOutlined,
  MoonOutlined,
} from '@ant-design/icons';
import { useAuth } from '@/contexts/AuthContext';
import { useTheme } from '@/contexts/ThemeContext';
import { useRouter, usePathname } from 'next/navigation';

// ── Nav definition ────────────────────────────────────────────────────────────
type NavGroup = {
  section?: string;
  items: { key: string; icon: React.ReactNode; label: string }[];
  superAdminOnly?: boolean;
};

const NAV_GROUPS: NavGroup[] = [
  {
    items: [
      { key: '/dashboard', icon: <DashboardOutlined />, label: 'Dashboard' },
    ],
  },
  {
    section: 'User Management',
    items: [
      { key: '/users', icon: <UserOutlined />,   label: 'All Users' },
      { key: '/roles', icon: <SafetyOutlined />, label: 'Roles' },
      { key: '/groups', icon: <TeamOutlined />, label: 'User Groups' },
    ],
  },
  {
    section: 'Content',
    items: [
      { key: '/vocabularies', icon: <BookOutlined />, label: 'Vocabulary Library' },
      { key: '/quests',       icon: <AimOutlined />,  label: 'Quests & Achievements' },
      { key: '/gyms',         icon: <TrophyOutlined />, label: 'Gym Operations' },
    ],
  },
  {
    section: 'System',
    superAdminOnly: true,
    items: [
      { key: '/system-health', icon: <SettingOutlined />, label: 'System Health' },
    ],
  },
];

// ── Page titles (for header breadcrumb) ──────────────────────────────────────
const PAGE_TITLES: Record<string, string> = {
  '/dashboard':    'Dashboard',
  '/users':        'User Management',
  '/roles':         'Role Management',
  '/groups':        'User Groups',
  '/vocabularies': 'Vocabulary Library',
  '/quests':       'Quests & Achievements',
  '/gyms':         'Gym Operations',
  '/system-health':'System Health & Config',
};

// ── Sub-component: NavItem ────────────────────────────────────────────────────
function NavItem({
  navKey,
  icon,
  label,
  collapsed,
  isActive,
  onClick,
}: {
  navKey: string;
  icon: React.ReactNode;
  label: string;
  collapsed: boolean;
  isActive: boolean;
  onClick: () => void;
}) {
  return (
    <Tooltip title={collapsed ? label : ''} placement="right">
      <button
        onClick={onClick}
        className={`sidebar-nav-item${isActive ? ' active' : ''}`}
        style={{ justifyContent: collapsed ? 'center' : 'flex-start' }}
      >
        <span className="nav-icon">{icon}</span>
        {!collapsed && <span style={{ overflow: 'hidden', textOverflow: 'ellipsis' }}>{label}</span>}
      </button>
    </Tooltip>
  );
}

// ── Main Component ────────────────────────────────────────────────────────────
export default function AdminLayout({ children }: { children: React.ReactNode }) {
  const [collapsed, setCollapsed] = useState(false);
  const { user, logout, isLoading } = useAuth();
  const { colorScheme, toggleColorScheme } = useTheme();
  const router = useRouter();
  const pathname = usePathname();

  if (isLoading || !user) return null;

  const isSuperAdmin = user.role === 'SuperAdmin';
  const visibleGroups = NAV_GROUPS.filter(g => !g.superAdminOnly || isSuperAdmin);
  const allKeys = visibleGroups.flatMap(g => g.items.map(i => i.key));

  // Match active key: handle sub-routes like /vocabularies/123
  const activeKey = Object.keys(PAGE_TITLES).find(k =>
    k === '/' ? pathname === '/' : pathname.startsWith(k)
  ) ?? '';

  const pageTitle = Object.entries(PAGE_TITLES).find(([k]) =>
    pathname.startsWith(k)
  )?.[1] ?? '';

  const userMenuItems = [
    {
      key: 'info',
      label: (
        <div style={{ padding: '4px 0', lineHeight: '1.4' }}>
          <div style={{ fontWeight: 600, fontSize: 13, color: 'var(--text-primary)' }}>
            {user.name}
          </div>
          <div style={{ fontSize: 11, color: 'var(--text-muted)' }}>{user.email}</div>
        </div>
      ),
      disabled: true,
    },
    { type: 'divider' as const },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: 'Sign out',
      danger: true,
      onClick: logout,
    },
  ];

  // Role badge style
  const roleBadgeStyle: React.CSSProperties = {
    fontSize: 10,
    fontWeight: 600,
    padding: '1px 6px',
    borderRadius: 3,
    background: isSuperAdmin ? 'var(--accent-subtle)' : 'var(--bg-muted)',
    color: isSuperAdmin ? 'var(--accent)' : 'var(--text-secondary)',
    border: `1px solid ${isSuperAdmin ? 'var(--accent-border)' : 'var(--border)'}`,
    whiteSpace: 'nowrap' as const,
  };

  return (
    <div className="admin-shell">
      {/* ── Sidebar ─────────────────────────────────────────────────────────── */}
      <aside className={`admin-sidebar${collapsed ? ' collapsed' : ''}`}>
        {/* Logo */}
        <div className="sidebar-logo">
          <svg width="22" height="22" viewBox="0 0 22 22" fill="none" style={{ flexShrink: 0 }}>
            <rect width="22" height="22" rx="6" fill="var(--accent)" opacity="0.12" />
            <path d="M6 7h10M6 11h6M6 15h8" stroke="var(--accent)" strokeWidth="1.8" strokeLinecap="round" />
          </svg>
          {!collapsed && (
            <>
              <span className="sidebar-logo-text">Vocamon</span>
              <span className="sidebar-logo-badge">Admin</span>
            </>
          )}
        </div>

        {/* Nav */}
        <nav className="sidebar-nav">
          {visibleGroups.map((group, gi) => (
            <React.Fragment key={gi}>
              {group.section && !collapsed && (
                <div className="sidebar-section-label">{group.section}</div>
              )}
              {group.items.map(item => (
                <NavItem
                  key={item.key}
                  navKey={item.key}
                  icon={item.icon}
                  label={item.label}
                  collapsed={collapsed}
                  isActive={activeKey === item.key}
                  onClick={() => router.push(item.key)}
                />
              ))}
            </React.Fragment>
          ))}
        </nav>

        {/* Footer */}
        <div className="sidebar-footer">
          {collapsed ? (
            <Tooltip title={`${user.name} · ${user.role}`} placement="right">
              <Avatar
                size={32}
                style={{ background: 'var(--accent-subtle)', color: 'var(--accent)', cursor: 'default' }}
              >
                {user.name.charAt(0).toUpperCase()}
              </Avatar>
            </Tooltip>
          ) : (
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <Avatar
                size={30}
                style={{ background: 'var(--accent-subtle)', color: 'var(--accent)', flexShrink: 0, fontSize: 12, fontWeight: 600 }}
              >
                {user.name.charAt(0).toUpperCase()}
              </Avatar>
              <div style={{ flex: 1, minWidth: 0 }}>
                <div style={{ fontSize: 12, fontWeight: 600, color: 'var(--text-primary)', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                  {user.name}
                </div>
                <div style={roleBadgeStyle}>{user.role}</div>
              </div>
            </div>
          )}
        </div>
      </aside>

      {/* ── Main ────────────────────────────────────────────────────────────── */}
      <main className={`admin-main${collapsed ? ' sidebar-collapsed' : ''}`}>
        {/* Header */}
        <header className="admin-header">
          {/* Left */}
          <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
            <button
              onClick={() => setCollapsed(c => !c)}
              style={{
                background: 'none',
                border: 'none',
                cursor: 'pointer',
                color: 'var(--text-muted)',
                padding: '4px 6px',
                borderRadius: 'var(--radius-sm)',
                display: 'flex',
                alignItems: 'center',
                transition: 'color var(--transition), background var(--transition)',
              }}
              onMouseEnter={e => {
                (e.currentTarget as HTMLElement).style.background = 'var(--bg-muted)';
                (e.currentTarget as HTMLElement).style.color = 'var(--text-primary)';
              }}
              onMouseLeave={e => {
                (e.currentTarget as HTMLElement).style.background = 'none';
                (e.currentTarget as HTMLElement).style.color = 'var(--text-muted)';
              }}
            >
              {collapsed ? <MenuUnfoldOutlined style={{ fontSize: 16 }} /> : <MenuFoldOutlined style={{ fontSize: 16 }} />}
            </button>
            {pageTitle && (
              <span style={{ fontSize: 14, fontWeight: 600, color: 'var(--text-primary)' }}>
                {pageTitle}
              </span>
            )}
          </div>

          {/* Right */}
          <div style={{ display: 'flex', alignItems: 'center', gap: 4 }}>
            {/* Theme Toggle */}
            <Tooltip title={colorScheme === 'light' ? 'Switch to dark mode' : 'Switch to light mode'}>
              <button
                onClick={toggleColorScheme}
                style={{
                  background: 'none',
                  border: 'none',
                  cursor: 'pointer',
                  color: 'var(--text-muted)',
                  padding: '6px 8px',
                  borderRadius: 'var(--radius-sm)',
                  display: 'flex',
                  alignItems: 'center',
                  fontSize: 15,
                  transition: 'color var(--transition), background var(--transition)',
                }}
                onMouseEnter={e => {
                  (e.currentTarget as HTMLElement).style.background = 'var(--bg-muted)';
                  (e.currentTarget as HTMLElement).style.color = 'var(--text-primary)';
                }}
                onMouseLeave={e => {
                  (e.currentTarget as HTMLElement).style.background = 'none';
                  (e.currentTarget as HTMLElement).style.color = 'var(--text-muted)';
                }}
              >
                {colorScheme === 'light' ? <MoonOutlined /> : <SunOutlined />}
              </button>
            </Tooltip>

            {/* User Dropdown */}
            <Dropdown
              menu={{ items: userMenuItems }}
              placement="bottomRight"
              trigger={['click']}
            >
              <button
                style={{
                  background: 'none',
                  border: 'none',
                  cursor: 'pointer',
                  display: 'flex',
                  alignItems: 'center',
                  gap: 8,
                  padding: '4px 8px',
                  borderRadius: 'var(--radius-sm)',
                  transition: 'background var(--transition)',
                }}
                onMouseEnter={e => { (e.currentTarget as HTMLElement).style.background = 'var(--bg-muted)'; }}
                onMouseLeave={e => { (e.currentTarget as HTMLElement).style.background = 'none'; }}
              >
                <Avatar
                  size={28}
                  style={{ background: 'var(--accent-subtle)', color: 'var(--accent)', fontSize: 11, fontWeight: 600 }}
                >
                  {user.name.charAt(0).toUpperCase()}
                </Avatar>
                <span style={{ fontSize: 13, fontWeight: 500, color: 'var(--text-primary)' }}>
                  {user.name}
                </span>
              </button>
            </Dropdown>
          </div>
        </header>

        {/* Page Content */}
        <div className="admin-content">
          {children}
        </div>
      </main>
    </div>
  );
}
